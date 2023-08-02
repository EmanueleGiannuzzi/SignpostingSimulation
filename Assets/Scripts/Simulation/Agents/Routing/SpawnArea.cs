using MyBox;
using UnityEngine;
using UnityEngine.AI;

public class SpawnArea : SpawnAreaBase {

    public SpawnAreaDestination[] goals;

    //public Gradient AgentColorGradient;


    

    private GameObject SpawnAgentMoveTo(GameObject agentPrefab, SpawnAreaDestination goal) {
        Transform destination = goal.Destination;
        Collider destroyer = goal.Destroyer;
        return SpawnAgentMoveTo(agentPrefab, destination.position, destroyer);
    }


    protected GameObject SpawnAgentMoveTo(GameObject agentPrefab, Vector3 destination, Collider destroyer) {
        if(!this.Enabled || goals is not { Length: > 0 }) {
            return null;
        }

        GameObject agent = base.SpawnAgent(agentPrefab);
        agent.GetComponent<AgentCollisionDetection>().destroyer = destroyer;

        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(destination);

        return agent;
    }

    private new GameObject SpawnAgent(GameObject agentPrefab) {
        if(goals.Length == 0) {
            Debug.Log("SpawnArea [" + this.gameObject.name + "]: No destination set");
        }
        return SpawnAgentMoveTo(agentPrefab, goals[Random.Range(0, goals.Length)]);
    }

    public override GameObject SpawnAgentEvent(GameObject agentPrefab) {
        return SpawnAgent(agentPrefab);
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
