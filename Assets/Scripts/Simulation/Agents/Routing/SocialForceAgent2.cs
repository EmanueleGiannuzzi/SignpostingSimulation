using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SocialForceAgent2 : MonoBehaviour {
    private Vector3 velocity => characterAgent.velocity;

    private AgentsSpawnHandler agentsSpawnHandler;
    
    private RoutedAgent characterControl;
    private NavMeshAgent characterAgent;

    private const float radius = 0.15f;
    private const float desiredSpeed = 5f;

    public Vector3 destination => characterControl.Target;

    private static readonly string[] ifcWallTags = new[] { "IfcWallStandardCase" };
    private void Start() {
        characterControl = this.GetComponent<RoutedAgent>();
        characterAgent = this.GetComponent<NavMeshAgent>();
        agentsSpawnHandler = FindObjectOfType<AgentsSpawnHandler>();
        
        // if (this.destination == null) {
        //     this.destination = this.transform;
        // }
        // else {
        //     if (characterControl)
        //         characterControl.target.localPosition = velocity;
        // }
            
        // if (characterAgent)
        //     characterAgent.speed = desiredSpeed;
    }

    private void FixedUpdate() {
        // Vector3 acceleration = DrivingForce() + AgentInteractForce() + WallInteractForce();
        Vector3 acceleration = AgentInteractForce() + WallInteractForce();
        Vector3 newVelocity = acceleration * (Time.deltaTime * 3);


        //Limit maximum velocity
        if (Vector3.SqrMagnitude(newVelocity) > desiredSpeed * desiredSpeed) {
            newVelocity.Normalize();
            newVelocity *= desiredSpeed;
        }

        // Update current attributes to AICharacterControl
        // if (characterControl)
        //     characterControl.target.transform.position = this.transform.position + velocity.normalized;
        // else
           // this.transform.position += velocity * (Time.deltaTime * 5);
           
        this.characterAgent.velocity += newVelocity / 40;

        // if (characterAgent)
        //     characterAgent.speed = desiredSpeed;
    }

    private IEnumerable<SocialForceAgent2> GetCloseAgents() {
        List<SocialForceAgent2> agents = new List<SocialForceAgent2>();
        foreach (Transform agentTransform in agentsSpawnHandler.GetAgentsParent().transform) {
            SocialForceAgent2 agent = agentTransform.GetComponent<SocialForceAgent2>();
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

    private Vector3 DrivingForce() {
        const float relaxationT = 0.54f;
        Vector3 desiredDirection = destination - this.transform.position;
        desiredDirection.Normalize();

        Vector3 drivingForce = (desiredSpeed * desiredDirection - velocity) / relaxationT;

        return drivingForce;
    }

    private Vector3 AgentInteractForce() {
        const float lambda = 2.0f;
        const float gamma = 0.35f;
        const float nPrime = 3.0f;
        const float n = 2.0f;
        //const float A = 4.5f;
        const float A = 47f;
        float B, theta;
        int K;
        Vector3 interactionForce = new Vector3(0f, 0f, 0f);
        Vector3 vectorToAgent;

        foreach (SocialForceAgent2 agent in GetCloseAgents()) {
            // Skip if agent is self
            if (agent == this) continue;

            vectorToAgent = agent.transform.position - this.transform.position;

            // Skip if agent is too far
            if (Vector3.SqrMagnitude(vectorToAgent) > 10f * 10f) continue;

            Vector3 directionToAgent = vectorToAgent.normalized;
            Vector3 interactionVector = lambda * (this.velocity - agent.velocity) + directionToAgent;

            B = gamma * Vector3.Magnitude(interactionVector);

            Vector3 interactionDir = interactionVector.normalized;

            theta = Mathf.Deg2Rad * Vector3.Angle(interactionDir, directionToAgent);

            if (theta == 0) {
                K = 0;
            }
            else if (theta > 0) {
                K = 1;
            }
            else {
                K = -1;
            }

            float distanceToAgent = Vector3.Magnitude(vectorToAgent);
            float deceleration = -A * Mathf.Exp(-distanceToAgent / B - (nPrime * B * theta) * (nPrime * B * theta));
            float directionalChange = -A * K * Mathf.Exp(-distanceToAgent / B
                                                         - (n * B * theta) * (n * B * theta));
            Vector3 normalInteractionVector = new Vector3(-interactionDir.z, interactionDir.y, interactionDir.x);
            //Vector3 normalInteractionVector = new Vector3(-interactionDir.y, interactionDir.x, 0);

            interactionForce += deceleration * interactionDir + directionalChange * normalInteractionVector; 
        }
        return interactionForce;
    }

    private Vector3 WallInteractForce() {
        const float A = 3f;
        const float B = 0.8f;

        float squaredDist = Mathf.Infinity;
        float minSquaredDist = Mathf.Infinity;
        Vector3 minDistVector = new Vector3();

        // Find distance to nearest obstacles
        foreach (Vector3 wallNearestPoint in GetCloseWallsPoints()) {
            Vector3 vectorToNearestPoint = this.transform.position - wallNearestPoint;
            squaredDist = Vector3.SqrMagnitude(vectorToNearestPoint);

            if (squaredDist < minSquaredDist) {
                minSquaredDist = squaredDist;
                minDistVector = vectorToNearestPoint;
            }
        }

        float distToNearestObs = Mathf.Sqrt(squaredDist) - radius;
        float interactionForce = A * Mathf.Exp(-distToNearestObs / B);

        minDistVector.Normalize();
        minDistVector.y = 0;
        Vector3 obsInteractForce = interactionForce * minDistVector.normalized;

        return obsInteractForce;
    }

}