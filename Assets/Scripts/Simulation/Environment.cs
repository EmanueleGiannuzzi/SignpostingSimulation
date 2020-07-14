using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    public KeyCode Keybind;
    public int SimulationUpdateFrequencyHz;
    [HideInInspector]
    public float repeatRate;// cached value: (1 / SimulationUpdateFrequencyHz)
    [HideInInspector]
    public float positionSensitivity;// 

    private VisibilityHandler visibilityHandler;
    private VisibilityPlaneGenerator visibilityPlaneGenerator;
    private AgentsSpawnHandler agentsSpawnHandler;

    private bool isSimulationRunning = false;

    private int[,] signboardAgentViews; //[agentTypeID, signageBoardID]

    void Start() {
        repeatRate = 1 / SimulationUpdateFrequencyHz;
        positionSensitivity = 1 / visibilityHandler.resolution;

        visibilityHandler = GetComponentInChildren<VisibilityHandler>();
        visibilityPlaneGenerator = GetComponentInChildren<VisibilityPlaneGenerator>();
        agentsSpawnHandler = GetComponentInChildren<AgentsSpawnHandler>();

        if(IsSimulationEnabled()) {
            StartSimulation();
        }
    }

    void Update() {
        if(Input.GetKeyDown(Keybind)) {
            isSimulationRunning = !isSimulationRunning;
            if(isSimulationRunning) {
                StartSimulation();
            }
            else {
                StopSimulation();
            }
        }
    }

    public VisibilityHandler GetVisibilityHandler() {
        return visibilityHandler;
    }

    public bool IsSimulationEnabled() {
        return isSimulationRunning;
    }

    public void StartSimulation() {
        for(int agentID = 0; agentID < agentsSpawnHandler.GetAgentsCount(); agentID++) {
            SimulationAgent agent = agentsSpawnHandler.GetAgentsTranform(agentID).gameObject.GetComponent<SimulationAgent>();
            agent.StartSimulation(SimulationUpdateFrequencyHz);
        }
    }

    public void StopSimulation() {
        for(int agentID = 0; agentID < agentsSpawnHandler.GetAgentsCount(); agentID++) {
            SimulationAgent agent = agentsSpawnHandler.GetAgentsTranform(agentID).gameObject.GetComponent<SimulationAgent>();
            agent.StopSimulation();
        }
    }

    public List<int> GetSignageBoardsVisible(Vector3 agentPosition, int agentTypeID) {
        for(int visPlaneId = 0; visPlaneId < visibilityPlaneGenerator.GetVisibilityPlanesGroup().transform.childCount; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = visibilityHandler.visibilityInfos[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfos[agentTypeID];
            foreach(VisibilityInfo value in visInfoDictionary.Values) {
                if(Vector3.Distance(value.cachedWorldPos, agentPosition) < positionSensitivity) {
                    return value.GetVisibleBoards();
                }
            }

        }
        return null;
    }

    public void OnAgentEnterVisibilityArea(GameObject agent, int agentTypeID, int signboardID) {
        //Save agent pos
    }

    public void OnAgentExitVisibilityArea(GameObject agent, int agentTypeID, int signboardID, float residenceTime) {
        SignageBoard signageBoard = visibilityHandler.GetSignageBoard(signboardID);

        signboardAgentViews[agentTypeID, signboardID]++;

        Debug.Log("Agent " + agent.name + " stayed in range of " + signageBoard.name + " for " + residenceTime + " seconds");
    }
}
