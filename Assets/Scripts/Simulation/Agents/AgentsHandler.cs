using UnityEngine;
using UnityEngine.AI;
using static AgentsPrefabGenerator;

public class AgentsHandler : MonoBehaviour
{
    [Header("Agent Properties")]
    public GameObject PrefabBase;
    public AgentPrefabProperty PrefabProperties;

    [Header("Agent Spawn Settings")]
    public bool SpawnEnable = true;

    public int SpawRate = 30;

    private GameObject agentPrefab;
    public GameObject GetAgentPrefab() {
        return agentPrefab;
    }

    public void Start() {
        agentPrefab = AgentsPrefabGenerator.GeneratePrefab(PrefabBase, PrefabProperties);
    }

    void FixedUpdate() {
        if(!this.SpawnEnable) {
            return;
        }

        int now = Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
        foreach(SpawnArea spawnArea in this.GetComponentsInChildren<SpawnArea>()) {
            if(spawnArea.Enabled) {
                float actualSpawnRate = spawnArea.OverrideSpawnRate ? spawnArea.SpawnRate : this.SpawRate;
                if(now % actualSpawnRate == 0) {
                    spawnArea.SpawnAgent();
                    //GameObject agent = spawnArea.SpawnAgent();
                    //agent.GetComponent<AgentCollisionDetection>().collisionEvent.AddListener(OnAgentCollidedWith);
                }
            }
        }

    }

    //void OnAgentCollidedWith(NavMeshAgent agent, Collider collider) {

    //}
}
