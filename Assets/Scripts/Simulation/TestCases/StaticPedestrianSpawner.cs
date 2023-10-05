using System.Collections.Generic;
using UnityEngine;

public class StaticPedestrianSpawner : SpawnAreaBase {
    public GameObject AgentPrefab;
    public float PedestrianDensity = 0.2f;

    private readonly List<GameObject> agents = new();

    private new void Start() {
        base.Start();
        SpawnPedestrians();
    }

    public void SpawnPedestrians() {
        Vector3 localScale = this.transform.localScale;
        float area = localScale.x * localScale.z * 100f;
        int numberOfPedestrians = Mathf.CeilToInt(area * PedestrianDensity);

        for (int i = 0; i < numberOfPedestrians; i++) {
            agents.Add(SpawnAgent(agents, AgentPrefab));
        }
    }

    public override GameObject SpawnAgentEvent(GameObject agentPrefab) {
        return null;
    }
}
