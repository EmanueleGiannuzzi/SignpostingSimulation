using UnityEngine;

public abstract class SpawnAreaBase : MonoBehaviour {
    public bool Enabled = true;
    private Environment environment;

    protected void Awake() {
        environment = FindObjectOfType<Environment>();
    }

    private bool isSpawnPointCloseToAgents(Vector3 point, float maxDistance) {
        if (environment.GetAgents() == null) {
            return false;
        }
        
        foreach (Transform agent in environment.GetAgents()) {
            Vector3 distanceVector = point - agent.position;
            if (distanceVector.sqrMagnitude > maxDistance * maxDistance) {
                return true;
            }
        }
        return false;
    }
    
    protected GameObject SpawnAgent(GameObject agentPrefab) {
        Transform agentTransform = this.transform;
        Vector3 position = agentTransform.position;
        Vector3 localScale = agentTransform.localScale / 2;
        float randX = Random.Range(-localScale.x, localScale.x) * 10;
        float randZ = Random.Range(-localScale.z, localScale.z) * 10;

        Vector3 spawnPoint;
        const int MAX_TENTATIVES = 1000;
        int tentatives = 0;
        do {
            if (tentatives >= MAX_TENTATIVES) {
                Debug.LogError("Unable to Spawn Agent: Spawn Area too full.");
                return null;
            }
            spawnPoint = new Vector3(position.x + randX, position.y, position.z + randZ);
            tentatives++;
        } while (isSpawnPointCloseToAgents(spawnPoint, 0.25f));

        GameObject agent = Instantiate(agentPrefab, spawnPoint, Quaternion.identity);

        if(environment != null) {
            environment.OnAgentSpawned();
        }

        return agent;
    }

    public abstract GameObject SpawnAgentEvent(GameObject agentPrefab);

    public virtual bool ShouldSpawnAgents() {
        return Enabled;
    }
}
