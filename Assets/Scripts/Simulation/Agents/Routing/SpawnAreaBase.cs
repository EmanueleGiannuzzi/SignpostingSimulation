using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SpawnAreaBase : MonoBehaviour {
    public bool Enabled = true;
    protected Environment environment;
    
    private static List<GameObject> agentsSpawned = new();

    private const float SPAWNED_AGENTS_INITIAL_MIN_DISTANCE = 5f;
    private const float SPAWNED_AGENTS_MIN_DISTANCE = 1f;

    public bool IsSpawnRandom = true;

    protected void Start() {
        environment = FindObjectOfType<Environment>();
    }

    protected static List<GameObject> GetAgents() {
        return agentsSpawned;
    }

    private static bool isSpawnPointCloseToAgents(IEnumerable<GameObject> agents, Vector3 point, float minDistance) {
        if (agents == null || agents.Count() <= 0) {
            return false;
        }

        float min = float.MaxValue;
        foreach (GameObject agent in agents) {
            if (agent == null) {
                continue;
            }
            Vector3 distanceVector = point - agent.transform.position;
            if (distanceVector.sqrMagnitude < minDistance * minDistance) {
                return true;
            }
        }
        return false;
    }
    

    public GameObject SpawnAgent(GameObject agentPrefab) {
        return SpawnAgent(GetAgents(), agentPrefab);
    }

    protected GameObject SpawnAgent(IEnumerable<GameObject> agents, GameObject agentPrefab) {
        Transform spawnAreaTransform = this.transform;
        Vector3 position = spawnAreaTransform.position;
        Vector3 localScale = spawnAreaTransform.localScale / 2;

        Vector3 spawnPoint;
        const int MAX_TENTATIVES = 50;
        float agentsMinDistance = SPAWNED_AGENTS_INITIAL_MIN_DISTANCE;
        bool spawnPointFound;
        do {
            var tentatives = 0;
            do {
                float randXOffset = 0;
                float randZOffset = 0;
                if (IsSpawnRandom) {
                    randXOffset = Random.Range(-localScale.x, localScale.x) * 9.5f;
                    randZOffset = Random.Range(-localScale.z, localScale.z) * 9.5f;
                }
                spawnPoint = new Vector3(position.x + randXOffset, position.y, position.z + randZOffset);
                tentatives++;
                spawnPointFound = !isSpawnPointCloseToAgents(agents, spawnPoint, agentsMinDistance);
            } while (tentatives < MAX_TENTATIVES && !spawnPointFound);
            agentsMinDistance -= agentsMinDistance * 0.2f;
        } while (!spawnPointFound && agentsMinDistance > SPAWNED_AGENTS_MIN_DISTANCE);

        if (!spawnPointFound) {
            Debug.LogWarning("Unable to spawn agent. Skipping. " + agentsMinDistance);
            return null;
        }

        GameObject agent = Instantiate(agentPrefab, spawnPoint, Quaternion.identity);

        if(environment != null) {
            environment.OnAgentSpawned();
        }

        agentsSpawned.Add(agent);
        return agent;
    }

    public abstract GameObject SpawnAgentEvent(GameObject agentPrefab);

    public virtual bool ShouldSpawnAgents() {
        return Enabled;
    }
}
