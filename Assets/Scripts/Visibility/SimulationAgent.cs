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
        //Debug.Log("BANANA START " + repeatRate + "s");
        InvokeRepeating(nameof(SimulationUpdate), 0f, repeatRate);
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

                for(int i = 0; i < visibleBoards.Count; i++) {
                    int signageBoardID = visibleBoards[i];

                    if(!IsSignboardInFOV(signageBoardID)) {
                        visibleBoards.Remove(signageBoardID);
                        Debug.Log("BANANA REMOVED");
                    }
                }

                if(visibleBoards != null) {
                    OnAgentInVisibilityArea(visibleBoards, agentTypeID);
                }
            }
        }
    }

    private void OnAgentInVisibilityArea(List<int> visibleBoards, int agentTypeID) {
        BoardsEcounter boardsEcounters = boardsEcountersPerAgentType[agentTypeID];
        if(boardsEcounters == null) {
            boardsEcounters = new BoardsEcounter();
        }
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
        List<int> signBoardsToRemove = new List<int>();
        foreach(KeyValuePair<int, float> boardEncounter in boardsEcounters.visibleBoards) {
            int signageBoardID = boardEncounter.Key;
            if(!visibleBoards.Contains(signageBoardID)) {
                signBoardsToRemove.Add(signageBoardID);

                float enterTime = boardEncounter.Value;
                environment.OnAgentExitVisibilityArea(this.gameObject, agentTypeID, signageBoardID, now - enterTime);
            }
        }

        foreach(int signgboardToRemoveID in signBoardsToRemove) {
            boardsEcounters.visibleBoards.Remove(signgboardToRemoveID);
        }

        boardsEcountersPerAgentType[agentTypeID] = boardsEcounters;
    }

    private bool IsSignboardInFOV(int signboardID) {
        SignageBoard signageBoard = environment.signageBoards[signboardID];
        Vector2 signboardPos = Utility.Vector3ToVerctor2NoY(signageBoard.transform.position);

        Vector2 agentPos = Utility.Vector3ToVerctor2NoY(this.transform.position);

        Vector2 directionLooking = Utility.Vector3ToVerctor2NoY(this.transform.forward);
        Vector2 directionSignboard = (signboardPos - agentPos).normalized;

        float angleToPoint = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(directionLooking, directionSignboard));
        float angleFOV = environment.AgentFOVDegrees;

        Debug.Log("ANGLE: " + angleToPoint + " " + angleFOV);
        return angleToPoint <= (angleFOV / 2);
    }

    private void OnDrawGizmos() {
        float angleFOV = environment.AgentFOVDegrees / 2;
        Color color = Color.cyan;
        Debug.DrawRay(this.transform.position, Quaternion.Euler(0, angleFOV, 0) * this.transform.forward, color);
        Debug.DrawRay(this.transform.position, Quaternion.Euler(0, -angleFOV, 0) * this.transform.forward, color);
    }
}
