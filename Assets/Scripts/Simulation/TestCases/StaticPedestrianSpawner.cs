using System.Collections.Generic;
using UnityEngine;

public class StaticPedestrianSpawner : SpawnAreaBase {
    public GameObject AgentPrefab;
    public float PedestrianDensity = 0.2f;

    private List<GameObject> agents = new List<GameObject>();

    private new void Start() {
        base.Start();
        SpawnPedestrians();
    }

    public void SpawnPedestrians() {
        Vector3 localScale = this.transform.localScale;
        float area = localScale.x * localScale.z * 100f;
        int numberOfPedestrians = Mathf.CeilToInt(area * PedestrianDensity);

        for (int i = 0; i < numberOfPedestrians; i++) {
            agents.Add(SpawnAgent());
        }
    }

    private GameObject SpawnAgent() {
        Transform agentTransform = this.transform;
        Vector3 position = agentTransform.position;
        Vector3 localScale = agentTransform.localScale / 2;

        Vector3 spawnPoint;
        const int MAX_TENTATIVES = 1000;
        int tentatives = 0;
        do {
            if (tentatives >= MAX_TENTATIVES) {
                Debug.LogError("Unable to Spawn Agent: Spawn Area too full.");
                return null;
            }
            float randX = Random.Range(-localScale.x, localScale.x) * 10;
            float randZ = Random.Range(-localScale.z, localScale.z) * 10;
            spawnPoint = new Vector3(position.x + randX, position.y, position.z + randZ);
            tentatives++;
        } while (isSpawnPointCloseToAgents(agents, spawnPoint, SPAWNED_AGENTS_MIN_DISTANCE));

        GameObject agent = Instantiate(AgentPrefab, spawnPoint, Quaternion.identity);
        // agent.transform.SetParent(this.transform, true);
        if(environment != null) {
            environment.OnAgentSpawned();
        }

        return agent;
    }

    public override GameObject SpawnAgentEvent(GameObject agentPrefab) {
        return null;
    }
}
