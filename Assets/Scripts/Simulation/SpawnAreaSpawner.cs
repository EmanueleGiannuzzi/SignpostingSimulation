using UnityEngine;
using UnityEngine.AI;
using MyBox;

public class SpawnAreaSpawner : MonoBehaviour {
    [Header("SpawnRate")]

    public bool OverrideSpawnRate = true;
    [ConditionalField(nameof(OverrideSpawnRate))] 
    public int spawRate = 30;

    [Header("References")]
    public GameObject _agentPrefab;



    void FixedUpdate() {
        int now = Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
        foreach(SpawnArea spawnArea in this.GetComponentsInChildren<SpawnArea>()) {
            float actualSpawnRate = this.OverrideSpawnRate ? this.spawRate : spawnArea.spawnRate;
            if(now % actualSpawnRate == 0) {
                GameObject agent = spawnArea.spawnAgent();
                agent.GetComponent<AgentCollisionDetection>().collisionEvent.AddListener(OnAgentCollided);
            }
        }

    }


    void OnAgentCollided(NavMeshAgent agent, Collider collider) {

    }

}
