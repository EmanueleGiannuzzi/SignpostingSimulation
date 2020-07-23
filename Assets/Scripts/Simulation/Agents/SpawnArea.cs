﻿using MyBox;
using UnityEngine;
using UnityEngine.AI;

public class SpawnArea : MonoBehaviour {
    public bool Enabled = true;

    [Header("Agent Spawn Settings")]
    public bool OverrideSpawnRate = true;
    [ConditionalField(nameof(OverrideSpawnRate))]
    public int SpawnRate;

    public Transform Destination;
    public Collider Destroyer;
    public Gradient Gradient;

    private Environment environment;

    private void Start() {
        environment = FindObjectOfType<Environment>();
    }

    public GameObject SpawnAgentMoveTo(GameObject agentPrefab, Vector3 destination) {
        if(!this.Enabled) {
            return null;
        }

        Vector3 position = this.transform.position;
        Vector3 localScale = this.transform.localScale / 2;
        float randX = Random.Range(-localScale.x, localScale.x) * 10;
        float randZ = Random.Range(-localScale.z, localScale.z) * 10;

        Vector3 spawnPoint = new Vector3(position.x + randX, position.y, position.z + randZ);
        float gradientTime = (Mathf.Sin(Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime)) + 1) / 2;//[0,1]
        Color agentColor = Gradient.Evaluate(gradientTime);

        GameObject agent = Object.Instantiate(agentPrefab, spawnPoint, Quaternion.identity);
        agent.GetComponent<MeshRenderer>().material.color = agentColor;
        agent.GetComponent<AgentCollisionDetection>().destination = Destroyer;

        if(environment != null) {
            environment.OnAgentSpawned();
        }

        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(destination);

        return agent;
    }

    public GameObject SpawnAgent(GameObject agentPrefab) {
        return SpawnAgentMoveTo(agentPrefab, Destination.position);
    }

}
