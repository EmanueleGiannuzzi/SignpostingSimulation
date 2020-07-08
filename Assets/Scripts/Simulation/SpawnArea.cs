using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnArea : MonoBehaviour {
    public bool Enabled = true;

    public Transform destination;
    public Collider destroyer;
    public Gradient gradient;
    public int spawnRate;

    private GameObject agentPrefab;

    void Start() {
        agentPrefab = this.GetComponentInParent<SpawnAreaSpawner>()._agentPrefab;//TODO: Agent prefab generator
    }

    public GameObject spawnAgent() {
        if(Enabled) {
            Vector3 position = this.transform.position;
            Vector3 localScale = this.transform.localScale / 2;
            float randX = Random.Range(-localScale.x, localScale.x) * 10;
            float randZ = Random.Range(-localScale.z, localScale.z) * 10;

            Vector3 spawnPoint = new Vector3(position.x + randX, position.y, position.z + randZ);
            float gradientTime = (Mathf.Sin(Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime)) + 1) / 2;//[0,1]
            Color agentColor = gradient.Evaluate(gradientTime);

            GameObject agent = Object.Instantiate(agentPrefab, spawnPoint, Quaternion.identity);
            agent.GetComponent<MeshRenderer>().material.color = agentColor;
            agent.GetComponent<AgentCollisionDetection>().destination = destroyer;

            NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.SetDestination(destination.position);

            return agent;
        }
        return null;
    }
}
