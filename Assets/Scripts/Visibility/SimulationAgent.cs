using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationAgent : MonoBehaviour {
    private Environment environment;

    private struct Banana {
        int agentTypeID;//TODO: ArrayID?
        Dictionary<int, float> visibleBoards;//Board-First seen time(Time.time)
    }

    void Start() {
        environment = FindObjectOfType<Environment>();
    }

    public void StartSimulation(float repeatRate) {
        InvokeRepeating(nameof(SimulationUpdate), 1f, repeatRate);
    }

    public void StopSimulation() {
        CancelInvoke(nameof(SimulationUpdate));
    }

    private bool IsSimulationEnabled() {
        return environment.IsSimulationEnabled();
    }

    private void SimulationUpdate() {
        if(IsSimulationEnabled()) {
            for(int agentTypeID = 0; agentTypeID < environment.GetVisibilityHandler().agentTypes.Length; agentTypeID++) {
                List<int> visibleBoards = environment.GetSignageBoardsVisible(this.transform.position, agentTypeID);
                if(visibleBoards != null && visibleBoards.Count > 0) {
                    OnAgentInVisibilityArea(visibleBoards, agentTypeID);
                }

            }
        }
    }

    private void OnAgentInVisibilityArea(List<int> visibleBoards, int agentTypeID) {
        throw new NotImplementedException();
    }
}
