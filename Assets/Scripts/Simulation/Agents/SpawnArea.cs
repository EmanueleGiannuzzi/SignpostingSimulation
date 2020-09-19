using MyBox;
using UnityEngine;
using UnityEngine.AI;

public class SpawnArea : MonoBehaviour {
    public bool Enabled = true;

    [Header("Agent Spawn Settings")]
    public bool OverrideSpawnRate = true;
    [ConditionalField(nameof(OverrideSpawnRate))]
    public int SpawnRate;

    public SpawnAreaDestination[] goals;

    public Gradient Gradient;

    private Environment environment;

    private void Start() {
        environment = FindObjectOfType<Environment>();
    }
    public GameObject SpawnAgentMoveTo(GameObject agentPrefab, SpawnAreaDestination goal) {
        Transform destination = goal.Destination;
        Collider destroyer = goal.Destroyer;
        return SpawnAgentMoveTo(agentPrefab, destination.position, destroyer);
    }

    public GameObject SpawnAgentMoveTo(GameObject agentPrefab, Vector3 destination, Collider destroyer) {
        if(!this.Enabled || goals == null || goals.Length <= 0) {
            return null;
        }

        Vector3 position = this.transform.position;
        Vector3 localScale = this.transform.localScale / 2;
        float randX = Random.Range(-localScale.x, localScale.x) * 10;
        float randZ = Random.Range(-localScale.z, localScale.z) * 10;

        Vector3 spawnPoint = new Vector3(position.x + randX, position.y, position.z + randZ);
        float gradientTime = (Mathf.Sin(Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime)) + 1) / 2;//[0,1]
        Color agentColor = Gradient.Evaluate(gradientTime);

        GameObject agent = Object.Instantiate(agentPrefab, spawnPoint, Quaternion.identity);
        agent.GetComponent<MeshRenderer>().material.color = agentColor;
        agent.GetComponent<AgentCollisionDetection>().destroyer = destroyer;

        if(environment != null) {
            environment.OnAgentSpawned();
        }

        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(destination);

        return agent;
    }

    public GameObject SpawnAgent(GameObject agentPrefab) {
        if(goals.Length == 0) {
            Debug.Log("SpawnArea [" + this.gameObject.name + "]: No destination set");
        }
        return SpawnAgentMoveTo(agentPrefab, goals[Random.Range(0, goals.Length)]);
    }

}


[System.Serializable]
public class SpawnAreaDestination {
    [SerializeField]
    private Transform destination;

    [SerializeField]
    private Collider destroyer;

    public Transform Destination => destination;
    public Collider Destroyer => destroyer;

    public SpawnAreaDestination(Transform destination, Collider destroyer) {
        this.destination = destination;
        this.destroyer = destroyer;
    }
}
