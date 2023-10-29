using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SocialForceAgent : MonoBehaviour {
    private static readonly List<SocialForceAgent> agents = new ();
    
    private NavMeshAgent navMeshAgent;
    private Vector3 target => navMeshAgent.steeringTarget;

    private Vector3 velocity => navMeshAgent != null ? navMeshAgent.velocity : Vector3.zero;
    private float radius => navMeshAgent.radius;
    private float maxSpeed => navMeshAgent.speed;

    // private AgentsSpawnHandler agentsSpawnHandler;
    
    private const float maxInteractionDistance = 2f;
    private const float relaxationTime = 0.54f; 
    private const float speedStdDeviation = 0.19f; 
    private NormalDistribution desiredSpeed;
    // Constant Values Based on (Moussaid et al., 2009)
    private static readonly NormalDistribution lambdaDistribution = new(2.0f, 0.2f);    // 2.0 +- 0.2 Weight reflecting relative importance of velocity vector against position vector
    private static readonly NormalDistribution gammaDistribution =  new(0.35f, 0.01f);    // 0.35 +- 0.01 Speed interaction
    private static readonly NormalDistribution nPrimeDistribution =  new(3.0f, 0.7f);    // 3.0 +- 0.7 Angular interaction
    private static readonly NormalDistribution nDistribution =  new(2.0f, 0.1f);         // 2.0 +- 0.1 Angular interaction
    private static readonly NormalDistribution ADistribution =  new(4.5f, 0.3f);         // 4.5 +- 0.3 Modal parameter A
    
    private readonly float lambda = lambdaDistribution;
    private readonly float gamma =  gammaDistribution;
    private readonly float nPrime =  nPrimeDistribution;
    private readonly float n =  nDistribution;
    private readonly float A =  ADistribution;
    
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

    private void Update() {
        if (navMeshAgent == null) {
            return;
        }
        moveAgent(Time.deltaTime);
    }

    private void moveAgent(float stepTime) {
        Vector3 acceleration = calculateSocialForce();
        Vector3 newVelocity = velocity + acceleration * stepTime;

        Vector3.ClampMagnitude(newVelocity, desiredSpeed);
        navMeshAgent.velocity = newVelocity;

        navMeshAgent.transform.position = this.transform.position + velocity * stepTime;
    }
    
    private Vector3 driving;
    private Vector3 agentInteract;
    private Vector3 wallInteract;

    private void OnDrawGizmos() {
        var agentPosition = this.transform.position;
        // DebugExtension.DrawArrow(agentPosition + Vector3.up, driving, Color.green);
        DebugExtension.DrawArrow(agentPosition + Vector3.up, agentInteract, Color.blue);
        DebugExtension.DrawArrow(agentPosition + Vector3.up, wallInteract, Color.magenta);
        DebugExtension.DrawArrow(agentPosition + Vector3.up, velocity, Color.red);
    }

    private Vector3 calculateSocialForce() {
        driving = this.drivingForce();
        agentInteract = this.agentInteractForce();
        wallInteract = this.wallInteractForce();

        return driving + agentInteract + wallInteract;
        // return agentInteract + wallInteract;
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
        Vector3 agentPosition = this.transform.position;
        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);

        foreach (SocialForceAgent otherAgent in getCloseAgents()) {
            if (otherAgent == this) continue;
            // Compute Distance Between Agent j and i
            Vector3 distance_ij = otherAgent.transform.position - agentPosition;

            // Skip Computation if Agents i and j are Too Far Away
            if (Vector3.SqrMagnitude(distance_ij) > maxInteractionDistance * maxInteractionDistance) {
                continue;
            }

            // Compute Direction of Agent j from i
            // Formula: e_ij = (position_j - position_i) / ||position_j - position_i||
            Vector3 e_ij = distance_ij;
            e_ij.Normalize();

            // Compute Interaction Vector Between Agent i and j
            // Formula: D = lambda * (velocity_i - velocity_j) + e_ij
            Vector3 D_ij = lambda * (velocity - otherAgent.velocity) + e_ij;

            // Compute Modal Parameter B
            // Formula: B = gamma * ||D_ij||
            float B = gamma * D_ij.magnitude;

            // Compute Interaction Direction
            // Formula: t_ij = D_ij / ||D_ij||
            Vector3 t_ij = D_ij;
            t_ij.Normalize();

            // Compute Angle Between Interaction Direction (t_ij) and Vector Pointing from Agent i to j (e_ij)
            float theta = Vector3.SignedAngle(e_ij, t_ij, Vector3.up) * Mathf.Deg2Rad;
            theta += B * 0.005f;

            // Compute Sign of Angle 'theta'
            // Formula: K = theta / |theta|
            int K = theta == 0 ? 0 : (int)(theta / Mathf.Abs(theta));
            
            // Compute Amount of Deceleration
            // Formula: f_v = -A * exp(-distance_ij / B - ((n_prime * B * theta) * (n_prime * B * theta)))
            float f_v = -A * Mathf.Exp(-distance_ij.magnitude / B - ((nPrime * B * theta) * (nPrime * B * theta)));

            // Compute Amount of Directional Changes
            // Formula: f_theta = -A * K * exp(-distance_ij / B - ((n * B * theta) * (n * B * theta)))
            float f_theta = -A * K * Mathf.Exp(-distance_ij.magnitude / B - ((n * B * theta) * (n * B * theta)));

            // Compute Normal Vector of Interaction Direction Oriented to the Left
            Vector3 n_ij = new Vector3(-t_ij.z, 0f, t_ij.x);

            // Compute Interaction Force
            // Formula: f_ij = f_v * t_ij + f_theta * n_ij
            force += f_v * t_ij + f_theta * n_ij;
        }
        
        return force;
    }

     private Vector3 wallInteractForce() {
         const int repulsionCoefficient = 3;
         const float decayCoefficient = 0.1f;

         Vector3 minWallAgentVector = new(0f, 0f, 0f);
         float minDistanceSquared = float.PositiveInfinity;

         foreach (Vector3 wallPoint in getCloseWallsPoints()) {
             Vector3 wallAgentVector = this.transform.position - wallPoint;
             float distanceSquared = wallAgentVector.sqrMagnitude;
         
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