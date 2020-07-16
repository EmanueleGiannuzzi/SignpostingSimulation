using UnityEngine;
using UnityEngine.AI;

public static class AgentsPrefabGenerator
{
    [System.Serializable]
    public struct AgentPrefabProperty {
        //Size
        public float radius;//Meters
        public float height;//Meters
        //Movement
        public float speed;
        public float angularSpeed;
        public float acceleration;
        //Simulation
        public ObstacleAvoidanceType obstacleAvoidanceQuality;
        //FOV
        public float agentFOV;//degrees

        public AgentPrefabProperty(float radius, float height, float speed, float angularSpeed, float acceleration, float agentFOV, ObstacleAvoidanceType obstacleAvoidanceQuality) {
            this.radius = radius;           
            this.height = height;    
            
            this.speed = speed;
            this.angularSpeed = angularSpeed;
            this.acceleration = acceleration;

            this.obstacleAvoidanceQuality = obstacleAvoidanceQuality;

            this.agentFOV = agentFOV;
        }
    }

    public static GameObject GeneratePrefab(GameObject prefabBase, AgentPrefabProperty prefabProperties) {
        Transform transform = prefabBase.transform;
        transform.localScale = Vector3.one;

        Vector3 boundsSize = prefabBase.GetComponent<MeshRenderer>().bounds.extents * 2;

        float radiusTransformValue = MetersToTransformValue(prefabProperties.radius, boundsSize.x);
        float heightTransformValue = MetersToTransformValue(prefabProperties.height, boundsSize.y);
        transform.localScale = new Vector3(radiusTransformValue, heightTransformValue, radiusTransformValue);

        //CapsuleCollider collider = prefabBase.GetComponent<CapsuleCollider>();
        //collider.height = prefabProperties.height;
        //collider.radius = prefabProperties.radius;
        //collider.center = Vector3.zero;

        NavMeshAgent navMeshAgent = prefabBase.GetComponent<NavMeshAgent>();
        navMeshAgent.radius = prefabProperties.radius * 1.2f;//+20% to account for some distancing
        navMeshAgent.height = prefabProperties.height;

        navMeshAgent.speed = prefabProperties.speed;
        navMeshAgent.angularSpeed = prefabProperties.angularSpeed;
        navMeshAgent.acceleration = prefabProperties.acceleration;
        navMeshAgent.obstacleAvoidanceType = prefabProperties.obstacleAvoidanceQuality;

        return prefabBase;
    }

    public static void SetAgentHeight(GameObject agent, float heightMeters) {
        Transform transform = agent.transform;

        Vector3 boundsSize = agent.GetComponent<MeshRenderer>().bounds.extents * 2;
        float heightTransformValue = MetersToTransformValue(heightMeters, boundsSize.y);
        transform.localScale = new Vector3(transform.localScale.x, heightTransformValue, transform.localScale.z);

        agent.GetComponent<CapsuleCollider>().height = heightMeters;
    }

    public static float MetersToTransformValue(float meters, float baseValueMeters) {
        return meters / baseValueMeters;
    }
}
