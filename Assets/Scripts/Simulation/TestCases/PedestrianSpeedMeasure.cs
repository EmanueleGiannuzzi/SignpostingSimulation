using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianSpeedMeasure : MonoBehaviour {
    public EventAgentTriggerCollider start;
    public EventAgentTriggerCollider finish;

    public SpawnArea AreaStart;
    public SpawnArea AreaFinish;

    private readonly Dictionary<NavMeshAgent, float> crossingsLog = new (); //Agent - start crossing time

    private void Start() {
        start.collisionEvent.AddListener(onStartCrossed);
        finish.collisionEvent.AddListener(onFinishCrossed);
    }

    private void onStartCrossed(NavMeshAgent agent, Collider collider) {
        float now = Time.time;
        crossingsLog.Add(agent, now);
        
        Debug.Log("Agent entered");
    }

    private void onFinishCrossed(NavMeshAgent agent, Collider collider) {
        if (crossingsLog.ContainsKey(agent)) {
            float startTime = crossingsLog[agent];
            float now = Time.time;
            float elapsed = now - startTime;
            
            Debug.Log("Agent exited in " + elapsed);
        }
    }
}
