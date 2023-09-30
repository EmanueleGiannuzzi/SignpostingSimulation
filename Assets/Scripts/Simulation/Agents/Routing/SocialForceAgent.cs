using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SocialForceAgent : MonoBehaviour {
    private static readonly List<SocialForceAgent> agents = new ();
    
    private NavMeshAgent navMeshAgent;
    private Vector3 target => navMeshAgent.steeringTarget;
    private Vector3 velocity => navMeshAgent.velocity;
    private float radius => navMeshAgent.radius;
    private float maxSpeed => navMeshAgent.speed;
    private NormalDistribution desiredSpeed;

    // private AgentsSpawnHandler agentsSpawnHandler;
    
    private const float maxInteractionDistance = 2f;
    private const float relaxationTime = 0.54f; 
    private const float speedStdDeviation = 0.19f; 
    

    private static readonly string[] ifcWallTags = { "IfcWallStandardCase" };
    private void Awake() {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        // agentsSpawnHandler = FindObjectOfType<AgentsSpawnHandler>();

        navMeshAgent.updatePosition = false;
        desiredSpeed = new NormalDistribution(maxSpeed, speedStdDeviation);
        
        agents.Add(this);
    }

    private void OnDestroy() {
        agents.Remove(this);
    }

    // private void FixedUpdate() {
    //     if (navMeshAgent == null) {
    //         return;
    //     }
    //
    //     Vector3 socialForce = calculateSocialForce();
    //     socialForce = Vector3.ClampMagnitude(socialForce, maxSpeed);
    //
    //     navMeshAgent.velocity += socialForce / relaxationTime;
    // }

    private void Update() {
        if (navMeshAgent == null) {
            return;
        }
        moveAgent(Time.deltaTime);
    }

    private void moveAgent(float stepTime) {
        Vector3 acceleration = calculateSocialForce();
        var agentPosition = this.transform.position;
        Vector3 newVelocity = velocity + acceleration;

        Vector3.ClampMagnitude(newVelocity, desiredSpeed);
        navMeshAgent.velocity = newVelocity;

        navMeshAgent.transform.position = agentPosition + velocity * stepTime;
        // navMeshAgent.nextPosition = this.transform.position + velocity * stepTime;
    }
    
    private Vector3 driving;
    private Vector3 agentInteract;
    private Vector3 wallInteract;

    private void OnDrawGizmos() {
        var agentPosition = this.transform.position;
        DebugExtension.DebugArrow(agentPosition + Vector3.up, driving, Color.green);
        DebugExtension.DebugArrow(agentPosition + Vector3.up, wallInteract, Color.red);
        DebugExtension.DebugArrow(agentPosition + Vector3.up, agentInteract, Color.blue);
    }

    private Vector3 calculateSocialForce() {
        driving = this.drivingForce();
        agentInteract = this.agentInteractForce();
        wallInteract = this.wallInteractForce();

        
        return driving + agentInteract + wallInteract;
    }

    private Vector3 drivingForce() {
        Vector3 agentPosition = this.transform.position;
        Vector3 targetDirection = target - agentPosition;
        targetDirection.Normalize();
        Vector3 drivingForce = ((maxSpeed * targetDirection) - velocity) / relaxationTime;

        return drivingForce;
    }

    private static IEnumerable<SocialForceAgent> getCloseAgents() {
        return agents;
    }
    
    private IEnumerable<Vector3> getCloseWallsPoints() {
        List<Vector3> obstaclesPoints = new List<Vector3>();

        Vector3 agentPosition = this.transform.position + (Vector3.up * 0.10f);
        const int rays = 32;
        Vector3 direction = Vector3.forward;
        for (int i = 0; i < rays; i++) {
            Ray ray = new Ray(agentPosition, direction);
            direction = Quaternion.AngleAxis(360f/rays, Vector3.up) * direction;
            Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, 1 << 11);
            if (hit.collider != null) {
                obstaclesPoints.Add(hit.point);
            }
        }
        
        return obstaclesPoints.ToArray();
    }

     private Vector3 agentInteractForce() { 
        // Constant Values Based on (Moussaid et al., 2009)
        const float lambda = 2.0f;    // Weight reflecting relative importance of velocity vector against position vector
        const float gamma = 0.35f;    // Speed interaction
        const float nPrime = 3.0f;    // Angular interaction
        const float n = 2.0f;         // Angular interaction
        const float A = 4.5f;         // Modal parameter A

        Vector3 agentPosition = this.transform.position;

        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);

        foreach (SocialForceAgent otherAgent in getCloseAgents()) {
            if (otherAgent == this) continue;
            Vector3 distance_ij = otherAgent.transform.position - agentPosition;
            if (Vector3.SqrMagnitude(distance_ij) > maxInteractionDistance * maxInteractionDistance) {
                continue;
            }

            Vector3 e_ij = distance_ij.normalized;

            // Compute Interaction Vector Between Agent i and j
            // Formula: interactionDirection = Lambda * (Velocity_i - Velocity_j) + directionIJ
            Vector3 D_ij = lambda * (this.velocity - otherAgent.velocity) + e_ij;

            // Compute Modal Parameter B
            // Formula: B = Gamma * ||interactionDirection||
            float B = gamma *  Vector3.Magnitude(D_ij);

            // Compute Interaction Direction
            // Formula: interactionDirection = interactionDirection / ||interactionDirection||
            Vector3 t_ij = D_ij.normalized;

            // Compute Angle Between Interaction Direction (interactionDirection) and Vector Pointing from Agent i to j (directionIJ)
            float theta = Vector3.Angle(t_ij, e_ij) * Mathf.Deg2Rad;

            // // Compute Sign of Angle 'theta'
            // // Formula: K = theta / |theta|
            // int K = theta == 0 ? 0 : (int)(theta / Mathf.Abs(theta));
            //
            //
            // // Compute Amount of Deceleration
            // // Formula: forceVelocity = -A * Math.Exp(-distanceIJ.Length() / B - ((NPrime * B * theta) * (NPrime * B * theta)))
            // float forceVelocity = -A * Mathf.Exp(-Vector3.Magnitude(distance_ij) / B - ((nPrime * B * theta) * (nPrime * B * theta)));
            //
            // // Compute Amount of Directional Changes
            // // Formula: forceTheta = -A * K * Math.Exp(-distanceIJ.Length() / B - ((N * B * theta) * (N * B * theta)))
            // float forceTheta = -A * K * Mathf.Exp(-Vector3.Magnitude(distance_ij) / B - ((n * B * theta) * (n * B * theta)));
            //
            // // Compute Normal Vector of Interaction Direction Oriented to the Left
            // // Vector3 interactionNormal = new Vector3(-interactionDirection.z, interactionDirection.y, interactionDirection.x);
            // Vector3 interactionNormal = new Vector3(-t_ij.x, 0.0f, t_ij.z);
            //
            // // Compute Interaction Force
            // // Formula: force = forceVelocity * interactionDirection + forceTheta * interactionNormal
            // force += forceVelocity * D_ij  + forceTheta * interactionNormal;

            theta += B * 0.005f;
            float d = Vector3.Magnitude(distance_ij);
            Vector3 n_ij = Quaternion.Euler(0f, -90f, 0f) * t_ij;
            
            Vector3 force_ij = -A * Mathf.Exp(-d/B) * 
                               (Mathf.Exp(- Mathf.Pow(nPrime * B * theta, 2)) * t_ij + 
                                Mathf.Exp(- Mathf.Pow(n * B * theta, 2)) * n_ij);
            
            force += force_ij;
        }
        
        return force;
    }

     private Vector3 wallInteractForce() {
         const int repulsionCoefficient = 3;
         const float decayCoefficient = 0.1f;

         Vector3 minWallAgentVector = new(0f, 0f, 0f);
         float distanceSquared, minDistanceSquared = float.PositiveInfinity;

         foreach (Vector3 wallPoint in getCloseWallsPoints()) {
             Vector3 wallAgentVector = this.transform.position - wallPoint;
             distanceSquared = wallAgentVector.sqrMagnitude;
         
             if (distanceSquared < minDistanceSquared) {
                 minDistanceSquared = distanceSquared;
                 minWallAgentVector = wallAgentVector;
             }
         }
         
         if (minWallAgentVector == Vector3.zero) {
             return Vector3.zero;
         }
         
         float distanceToWall = minWallAgentVector.magnitude - radius; // Distance between wall and agent i

         // Compute Interaction Force
         // Formula: interactionForce = RepulsionCoefficient * exp(-distanceToWall / DecayCoefficient)
         float interactionForce = repulsionCoefficient * Mathf.Exp(-distanceToWall / decayCoefficient);
         minWallAgentVector.Normalize();
         return interactionForce * minWallAgentVector;
     }


}