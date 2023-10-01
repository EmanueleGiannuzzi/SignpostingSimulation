using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PedestrianSpeedMeasure : MonoBehaviour {
    public GameObject AgentPrefab; 
    public EventAgentTriggerCollider StartCollider;
    public EventAgentTriggerCollider FinishCollider;
    public InputArea AreaStart;
    public InputArea AreaFinish;

    private readonly Dictionary<NavMeshAgent, AgentInfo> crossingsLog = new (); //Agent - start crossing time

    private struct AgentInfo {
        public readonly float startingTime;
        public List<Vector2> agentPos;

        public AgentInfo(float startingTime) {
            this.startingTime = startingTime;
            agentPos = new List<Vector2>();
        }
    }
    
    
    [SerializeField]
    private UseCase SelectedAction;
    
    private enum UseCase {
        NONE,
        GO_TO,
        GO_TO_AND_BACK,
        DOUBLE_GO_TO_AND_BACK
    }


    private void Start() {
        StartCollider.collisionEvent.AddListener(onStartCrossed);
        FinishCollider.collisionEvent.AddListener(onFinishCrossed);
    }

    private void FixedUpdate() {
        foreach (NavMeshAgent agent in crossingsLog.Keys) {
            crossingsLog[agent].agentPos.Add(agent.transform.position);
        }
    }

    private void onStartCrossed(NavMeshAgent agent, Collider triggerCollider) {
        float now = Time.time;
        if (crossingsLog.ContainsKey(agent)) {
            crossingsLog.Remove(agent);
        }

        AgentInfo agentInfo = new AgentInfo(now);
        crossingsLog.Add(agent, agentInfo);
        
        Debug.Log("Agent entered");
    }

    private void onFinishCrossed(NavMeshAgent agent, Collider triggerCollider) {
        if (crossingsLog.ContainsKey(agent)) {
            float startTime = crossingsLog[agent].startingTime;
            float now = Time.time;
            float elapsed = now - startTime;
            
            Debug.Log("Agent exited in " + elapsed);
        }
    }
    
    private void PerformAction(UseCase action) {
        switch (action) {
            case UseCase.NONE:
                break;
            case UseCase.GO_TO:
                Queue<IRouteMarker> route = new Queue<IRouteMarker>();
                route.Enqueue(AreaFinish);
                AreaStart.SpawnRoutedAgent(AgentPrefab, route);
                break;
            case UseCase.GO_TO_AND_BACK:
                break;
            case UseCase.DOUBLE_GO_TO_AND_BACK:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public void PerformSelectedAction() {
        PerformAction(SelectedAction);
    }
}
