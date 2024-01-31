using UnityEngine;
using static AgentsPrefabGenerator;

[System.Serializable]
public class AgentsSpawnHandler : MonoBehaviour
{
    [Header("Agent Properties")]
    public GameObject PrefabBase;
    public AgentPrefabProperty PrefabProperties;

    [Header("Agent Spawn Settings")]
    public bool EnableSpawn = true;
    [Tooltip("Agents per second.")]
    [Range(0.0f, 100.0f)]
    public float SpawnRate;

    private GameObject AgentsGameObjectParent;
    private GameObject AgentPrefab;

    public int GetAgentsCount() {
        if(AgentsGameObjectParent == null) {
            return 0;
        }
        return AgentsGameObjectParent.transform.childCount;
    }

    public Transform GetAgentsTransform(int agentID) {
        return AgentsGameObjectParent.transform.GetChild(agentID);
    }

    public GameObject GetAgentsParent() {
        return AgentsGameObjectParent;
    }

    public GameObject GetAgentPrefab() {
        return AgentPrefab;
    }

    void Start() {
        AgentsGameObjectParent = new GameObject("Agents");
        AgentPrefab = AgentsPrefabGenerator.GeneratePrefab(PrefabBase, PrefabProperties);
        if(this.EnableSpawn) {
            StartSpawn();
        }
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.F)) {
            EnableSpawn = !EnableSpawn;
            if(EnableSpawn) {
                StartSpawn();
            }
            else {
                StopSpawn();
            }
        }
    }

    public void StartSpawn() {
        if(SpawnRate > 0) {
            InvokeRepeating(nameof(SpawnAgents), 1f, 1 / SpawnRate);
        }
    }

    public void StopSpawn() {
        CancelInvoke(nameof(SpawnAgents));
    }

    private void SpawnAgents() {
        foreach(SpawnAreaBase spawnArea in this.GetComponentsInChildren<SpawnAreaBase>()) {
            if(spawnArea.ShouldSpawnAgents()) {
                GameObject agent = spawnArea.SpawnAgentEvent(GetAgentPrefab());
                if (agent != null) {
                    agent.transform.parent = AgentsGameObjectParent.transform;
                }
                //agent.GetComponent<AgentCollisionDetection>().collisionEvent.AddListener(OnAgentCollidedWith);
            }
        }
    }
}
