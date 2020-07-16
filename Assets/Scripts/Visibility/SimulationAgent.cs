using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationAgent : MonoBehaviour {
    private Environment environment;

    private BoardsEcounter[] boardsEcountersPerAgentType;

    private class BoardsEcounter {
        public Dictionary<int, float> visibleBoards;//Board-First seen time(Time.time)

        public BoardsEcounter() {
            visibleBoards = new Dictionary<int, float>();
        }
    }

    void Start() {
        environment = FindObjectOfType<Environment>();

        if(environment == null) {
            throw new MissingReferenceException("No Environment found!");
        }

        boardsEcountersPerAgentType = new BoardsEcounter[environment.GetVisibilityHandler().agentTypes.Length];

        if(environment.IsSimulationEnabled()) {
            StartSimulation(environment.repeatRate);
        }
    }

    public void StartSimulation(float repeatRate) {
        Debug.Log("BANANA START");
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
            Debug.Log("BANANA TICK");
            for(int agentTypeID = 0; agentTypeID < environment.GetVisibilityHandler().agentTypes.Length; agentTypeID++) {
                List<int> visibleBoards = environment.GetSignageBoardsVisible(this.transform.position, agentTypeID);
                if(visibleBoards != null && visibleBoards.Count > 0) {
                    OnAgentInVisibilityArea(visibleBoards, agentTypeID);
                }
            }
        }
    }

    private void OnAgentInVisibilityArea(List<int> visibleBoards, int agentTypeID) {
        BoardsEcounter boardsEcounters = boardsEcountersPerAgentType[agentTypeID];
        float now = Time.time;

        foreach(int signageBoardID in visibleBoards) {
            //1) Se non c'è in boardsEcounters viene aggiunta e l'agente entra
            if(!boardsEcounters.visibleBoards.ContainsKey(signageBoardID)) {
                boardsEcounters.visibleBoards.Add(signageBoardID, now);

                environment.OnAgentEnterVisibilityArea(this.gameObject, agentTypeID, signageBoardID);
            }
            //else { } //2) Se c'è già non fa niente
        }
        //3) Se c'è in boardsEcounters, ma non in visibleBoards viene tolta e l'agente esce 
        foreach(KeyValuePair<int, float> boardEncounter in boardsEcounters.visibleBoards) {
            int signageBoardID = boardEncounter.Key;
            if(!visibleBoards.Contains(signageBoardID)) {
                boardsEcounters.visibleBoards.Remove(signageBoardID);

                float enterTime = boardEncounter.Value;
                environment.OnAgentExitVisibilityArea(this.gameObject, agentTypeID, signageBoardID, now - enterTime);
            }
        }
    }
}
