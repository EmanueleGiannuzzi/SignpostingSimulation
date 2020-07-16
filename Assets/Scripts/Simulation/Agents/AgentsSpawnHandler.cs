using UnityEngine;
using UnityEngine.AI;
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
    [Range(0.0f, 5.0f)]
    public float SpawRate;

    [HideInInspector]
    private GameObject AgentsGameObjectParent;
    private GameObject AgentPrefab;

    public int GetAgentsCount() {
        if(AgentsGameObjectParent == null) {
            return 0;
        }
        return AgentsGameObjectParent.transform.childCount;
    }

    public Transform GetAgentsTranform(int agentID) {
        return AgentsGameObjectParent.transform.GetChild(agentID);
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
        if(SpawRate > 0) {
            InvokeRepeating(nameof(SpawnAgents), 1f, 1 / SpawRate);
        }
    }

    public void StopSpawn() {
        CancelInvoke(nameof(SpawnAgents));
    }

    private void SpawnAgents() {
        foreach(SpawnArea spawnArea in this.GetComponentsInChildren<SpawnArea>()) {
            if(spawnArea.Enabled) {
                GameObject agent = spawnArea.SpawnAgent(GetAgentPrefab());
                agent.transform.parent = AgentsGameObjectParent.transform;
                //agent.GetComponent<AgentCollisionDetection>().collisionEvent.AddListener(OnAgentCollidedWith);
            }
        }
    }
}
