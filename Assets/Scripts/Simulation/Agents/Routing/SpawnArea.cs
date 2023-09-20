using MyBox;
using UnityEngine;
using UnityEngine.AI;

public class SpawnArea : SpawnAreaBase {

    public SpawnAreaDestination[] goals;
    
    // [Header("Agent Spawn Settings")]
    // public bool OverrideSpawnRate = true;
    // [ConditionalField(nameof(OverrideSpawnRate))]
    // public int SpawnRate;

    private GameObject SpawnAgentMoveTo(GameObject agentPrefab, SpawnAreaDestination goal) {
        Transform destination = goal.Destination;
        Collider destroyer = goal.Destroyer;
        return SpawnAgentMoveTo(agentPrefab, destination.position, destroyer);
    }

    protected GameObject SpawnAgentMoveTo(GameObject agentPrefab, Vector3 destination, Collider destroyer) {
        GameObject agent = SpawnAgent(agentPrefab);
        agent.GetComponent<AgentCollisionDetection>().destroyer = destroyer;

        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(destination);

        return agent;
    }

    public override GameObject SpawnAgentEvent(GameObject agentPrefab) {
        return SpawnAgentMoveTo(agentPrefab, goals[Random.Range(0, goals.Length)]);
    }
    
    public override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && goals.Length != 0;
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
