using UnityEngine;
using UnityEngine.AI;
using static AgentsPrefabGenerator;

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
    private GameObject AgentsParent;
    private GameObject agentPrefab;

    public GameObject GetAgentPrefab() {
        return agentPrefab;
    }

    void Start() {
        AgentsParent = new GameObject("Agents");
        agentPrefab = AgentsPrefabGenerator.GeneratePrefab(PrefabBase, PrefabProperties);
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
                agent.transform.parent = AgentsParent.transform;
                //agent.GetComponent<AgentCollisionDetection>().collisionEvent.AddListener(OnAgentCollidedWith);
            }
        }
    }
}
