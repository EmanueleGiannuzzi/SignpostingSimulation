using System.Collections.Generic;
using UnityEngine;

public class SimulationAgent : MonoBehaviour {
    private Environment environment;

    private List<int>[] signboardEncounteredPerAgentType;
    private BoardsEncounter[] boardsEncountersPerAgentType;

    public bool DebugFOV = false;

    private class BoardsEncounter {
        public readonly Dictionary<int, float> visibleBoards;//Board-First seen time(Time.time)

        public BoardsEncounter() {
            visibleBoards = new Dictionary<int, float>();
        }
    }

    void Start() {
        environment = FindObjectOfType<Environment>();

        if(environment == null) {
            throw new MissingReferenceException("No Environment found!");
        }

        boardsEncountersPerAgentType = new BoardsEncounter[environment.GetVisibilityHandler().agentTypes.Length];

        if(environment.IsSimulationEnabled()) {
            OnStartSimulation(environment.repeatRate);
        }
    }

    public void OnStartSimulation(float repeatRate) {
        int agentTypeSize = environment.GetVisibilityHandler().agentTypes.Length;
        signboardEncounteredPerAgentType = new List<int>[agentTypeSize];
        for(int i = 0; i < agentTypeSize; i++) {
            signboardEncounteredPerAgentType[i] = new List<int>();
        }

        InvokeRepeating(nameof(simulationUpdate), 0f, repeatRate);
    }

    public void OnSimulationStopped() {
        CancelInvoke(nameof(simulationUpdate));
        signboardEncounteredPerAgentType = null;
    }

    private bool isSimulationEnabled() {
        return environment.IsSimulationEnabled();
    }

    private void simulationUpdate() {
        if(isSimulationEnabled()) {
            for(int agentTypeID = 0; agentTypeID < environment.GetVisibilityHandler().agentTypes.Length; agentTypeID++) {
                List<int> visibleBoards = environment.GetSignageBoardsVisible(this.transform.position, agentTypeID);
                if(visibleBoards == null) {
                    return;
                }

                for(int i = 0; i < visibleBoards.Count; i++) {
                    int signageBoardID = visibleBoards[i];

                    if(!isSignboardInFOV(signageBoardID)) {
                        visibleBoards.Remove(signageBoardID);
                    }
                }

                if(visibleBoards != null) {
                    OnAgentInVisibilityArea(visibleBoards, agentTypeID);
                }
            }
        }
    }

    private void OnAgentInVisibilityArea(List<int> visibleBoards, int agentTypeID) {
        BoardsEncounter boardsEncounters = boardsEncountersPerAgentType[agentTypeID];
        if(boardsEncounters == null) {
            boardsEncounters = new BoardsEncounter();
        }
        float now = Time.time;

        foreach(int signageBoardID in visibleBoards) {
            //1) Se non c'è in boardsEncounters viene aggiunta e l'agente entra
            if(!boardsEncounters.visibleBoards.ContainsKey(signageBoardID)) {
                boardsEncounters.visibleBoards.Add(signageBoardID, now);
                environment.OnAgentEnterVisibilityArea(this.gameObject, agentTypeID, signageBoardID);
            }
            //else { } //2) Se c'è già non fa niente
        }
        //3) Se c'è in boardsEncounters, ma non in visibleBoards viene tolta e l'agente esce 
        List<int> signBoardsToRemove = new List<int>();
        foreach(KeyValuePair<int, float> boardEncounter in boardsEncounters.visibleBoards) {
            int signageBoardID = boardEncounter.Key;
            if(!visibleBoards.Contains(signageBoardID)) {
                signBoardsToRemove.Add(signageBoardID);

                List<int> signboardEncountered = signboardEncounteredPerAgentType[agentTypeID];
                if(!signboardEncountered.Contains(signageBoardID)) {
                    float enterTime = boardEncounter.Value;
                    signboardEncountered.Add(signageBoardID);
                    environment.OnAgentExitVisibilityArea(this.gameObject, agentTypeID, signageBoardID, now - enterTime);
                }
            }
        }

        foreach(int signboardToRemoveID in signBoardsToRemove) {
            boardsEncounters.visibleBoards.Remove(signboardToRemoveID);
        }

        boardsEncountersPerAgentType[agentTypeID] = boardsEncounters;
    }

    private bool isSignboardInFOV(int signboardID) {
        SignBoard signBoard = environment.signageBoards[signboardID];
        Vector2 signboardPos = Utility.Vector3ToVerctor2NoY(signBoard.transform.position);

        Vector2 agentPos = Utility.Vector3ToVerctor2NoY(this.transform.position);

        Vector2 directionLooking = Utility.Vector3ToVerctor2NoY(this.transform.forward);
        Vector2 directionSignboard = (signboardPos - agentPos).normalized;

        float angleToPoint = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(directionLooking, directionSignboard));
        float angleFOV = environment.AgentFOVDegrees;

        return angleToPoint <= (angleFOV / 2);
    }

    private void OnDrawGizmos() {
        if(DebugFOV) {
            float angleFOV = environment.AgentFOVDegrees / 2;
            Color color = Color.cyan;
            Debug.DrawRay(this.transform.position, Quaternion.Euler(0, angleFOV, 0) * this.transform.forward, color);
            Debug.DrawRay(this.transform.position, Quaternion.Euler(0, -angleFOV, 0) * this.transform.forward, color);
        }
    }
}
