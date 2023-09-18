using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SocialForceAgent : MonoBehaviour {
    private NavMeshAgent navMeshAgent;
    private Vector3 target => navMeshAgent.nextPosition;
    private Vector3 velocity => navMeshAgent.velocity;
    private float desiredSpeed => navMeshAgent.speed;

    private AgentsSpawnHandler agentsSpawnHandler;
    

    private float radius => navMeshAgent.radius;
    public const float relaxationTime = 0.3f;

    private static readonly string[] ifcWallTags = { "IfcWallStandardCase" };
    private void Start() {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        agentsSpawnHandler = FindObjectOfType<AgentsSpawnHandler>();
    }

    private void FixedUpdate() {
        if (navMeshAgent == null) {
            return;
        }

        Vector3 socialForce = CalculateSocialForce();
        Vector3 newVelocity = navMeshAgent.velocity + socialForce;
        newVelocity += Vector3.ClampMagnitude(newVelocity, desiredSpeed);
        navMeshAgent.velocity += socialForce;
        navMeshAgent.velocity = Vector3.ClampMagnitude(navMeshAgent.velocity, desiredSpeed);
    }
    
    private Vector3 CalculateSocialForce() {
        return AgentInteractForce() + WallInteractionForce();
    }

    private IEnumerable<SocialForceAgent> GetCloseAgents() {
        List<SocialForceAgent> agents = new List<SocialForceAgent>();
        foreach (Transform agentTransform in agentsSpawnHandler.GetAgentsParent().transform) {
            SocialForceAgent agent = agentTransform.GetComponent<SocialForceAgent>();
            if (agent != null) {
                agents.Add(agent);
            }
        }
        return agents.ToArray();
    }

    private static bool isIfcWall(IFCData ifcData) {
        return ifcWallTags.Contains(ifcData.IFCClass);
    }

    private IEnumerable<Vector3> GetCloseWallsPoints() {
        RaycastHit[] hits = Physics.SphereCastAll(this.transform.position, 2, Vector3.up, 2);
        List<Vector3> obstaclesPoints = new List<Vector3>();
        foreach (RaycastHit hit in hits) {
            IFCData ifcData = hit.collider.GetComponent<IFCData>();
            if (ifcData != null && isIfcWall(ifcData)) {
                obstaclesPoints.Add(hit.point);
            }
        }
        return obstaclesPoints.ToArray();
    }

     private Vector3 AgentInteractForce() { 
        const float maxInteractionDistance = 2f;
        // Constant Values Based on (Moussaid et al., 2009)
        const float lambda = 2.0f;    // Weight reflecting relative importance of velocity vector against position vector
        const float gamma = 0.35f;    // Speed interaction
        const float nPrime = 3.0f;    // Angular interaction
        const float n = 2.0f;         // Angular interaction
        const float A = 4.5f;         // Modal parameter A

        Vector3 distanceToAgent, vectorToAgent, interactionDirection, interactionNormal, force;
        float B, theta, forceVelocity, forceTheta;
        int K;

        force = new Vector3(0.0f, 0.0f, 0.0f);

        foreach (SocialForceAgent otherAgent in GetCloseAgents()) {
            if (otherAgent == this) continue;
            distanceToAgent = otherAgent.transform.position - this.transform.position;
            if (Vector3.SqrMagnitude(distanceToAgent) > maxInteractionDistance * maxInteractionDistance) {
                continue;
            }

            vectorToAgent = distanceToAgent.normalized;

            // Compute Interaction Vector Between Agent i and j
            // Formula: interactionDirection = Lambda * (Velocity_i - Velocity_j) + directionIJ
            interactionDirection = lambda * (this.velocity - otherAgent.velocity) + vectorToAgent;

            // Compute Modal Parameter B
            // Formula: B = Gamma * ||interactionDirection||
            B = gamma *  Vector3.Magnitude(interactionDirection);

            // Compute Interaction Direction
            // Formula: interactionDirection = interactionDirection / ||interactionDirection||
            interactionDirection.Normalize();

            // Compute Angle Between Interaction Direction (interactionDirection) and Vector Pointing from Agent i to j (directionIJ)
            theta = Vector3.Angle(interactionDirection, vectorToAgent);

            // Compute Sign of Angle 'theta'
            // Formula: K = theta / |theta|
            K = (theta == 0) ? 0 : (int)(theta / Mathf.Abs(theta));

            // Compute Amount of Deceleration
            // Formula: forceVelocity = -A * Math.Exp(-distanceIJ.Length() / B - ((NPrime * B * theta) * (NPrime * B * theta)))
            forceVelocity = -A * Mathf.Exp(-Vector3.Magnitude(distanceToAgent) / B - ((nPrime * B * theta) * (nPrime * B * theta)));

            // Compute Amount of Directional Changes
            // Formula: forceTheta = -A * K * Math.Exp(-distanceIJ.Length() / B - ((N * B * theta) * (N * B * theta)))
            forceTheta = -A * K * Mathf.Exp(-Vector3.Magnitude(distanceToAgent) / B - ((n * B * theta) * (n * B * theta)));

            // Compute Normal Vector of Interaction Direction Oriented to the Left
            interactionNormal = new Vector3(-interactionDirection.z, interactionDirection.y, interactionDirection.x);
            // interactionNormal = new Vector3(-interactionDirection.y, interactionDirection.x, 0.0f);

            // Compute Interaction Force
            // Formula: force = forceVelocity * interactionDirection + forceTheta * interactionNormal
            force += forceVelocity * interactionDirection + forceTheta * interactionNormal;
        }
        return force;
    }

     private Vector3 WallInteractionForce() {
         const int repulsionCoefficient = 3;
         const float decayCoefficient = 0.1f;

         Vector3 minWallAgentVector = new(0f, 0f, 0f);
         float distanceSquared, minDistanceSquared = float.PositiveInfinity, distanceToWall, interactionForce;

         foreach (Vector3 nearestPoint in GetCloseWallsPoints()) {
             Vector3 wallAgentVector = this.transform.position - nearestPoint;
             distanceSquared = wallAgentVector.sqrMagnitude;

             if (distanceSquared < minDistanceSquared) {
                 minDistanceSquared = distanceSquared;
                 minWallAgentVector = wallAgentVector;
             }
         }

         distanceToWall = Mathf.Sqrt(minDistanceSquared) - radius; // Distance between wall and agent i

         // Compute Interaction Force
         // Formula: interactionForce = RepulsionCoefficient * exp(-distanceToWall / DecayCoefficient)
         interactionForce = repulsionCoefficient * Mathf.Exp(-distanceToWall / decayCoefficient);
         minWallAgentVector.Normalize();

         return interactionForce * minWallAgentVector;
     }


}