using UnityEngine;

public abstract class SpawnAreaBase : MonoBehaviour {
    public bool Enabled = true;
    private Environment environment;

    protected void Start() {
        environment = FindObjectOfType<Environment>();
    }
    
    
    protected GameObject SpawnAgent(GameObject agentPrefab) {
        Vector3 position = this.transform.position;
        Vector3 localScale = this.transform.localScale / 2;
        float randX = Random.Range(-localScale.x, localScale.x) * 10;
        float randZ = Random.Range(-localScale.z, localScale.z) * 10;
        Vector3 spawnPoint = new Vector3(position.x + randX, position.y, position.z + randZ);

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
