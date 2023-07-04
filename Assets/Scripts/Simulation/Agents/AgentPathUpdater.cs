using UnityEngine;
using UnityEngine.AI;

public class AgentPathUpdater : MonoBehaviour {
    public NavMeshAgent navMeshAgent;
    private NavMeshPath path;
    private float elapsed = 0.0f;
    void Start() {
        path = new NavMeshPath();
        elapsed = 0.0f;
    }

    void Update() {
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f) {
            elapsed = 0.0f;
            NavMesh.CalculatePath(transform.position, navMeshAgent.destination, NavMesh.AllAreas, path);
            navMeshAgent.SetPath(path);
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[0], path.corners[i + 1], Color.red);
    }
}
