using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    public KeyCode Keybind;
    public int SimulationUpdateFrequencyHz;
    [HideInInspector]
    public float repeatRate;// cached value: (1 / SimulationUpdateFrequencyHz)
    [HideInInspector]
    public float positionSensitivity;// Cahed value: (1 / visibilityHandler.resolution)

    private VisibilityHandler visibilityHandler;
    private VisibilityPlaneGenerator visibilityPlaneGenerator;
    private AgentsSpawnHandler agentsSpawnHandler;

    private bool isSimulationRunning = false;

    private int[,] SignboardAgentViews; //[agentTypeID, signageBoardID]

    void Start() {
        visibilityHandler = GetComponentInChildren<VisibilityHandler>();
        visibilityPlaneGenerator = GetComponentInChildren<VisibilityPlaneGenerator>();
        agentsSpawnHandler = GetComponentInChildren<AgentsSpawnHandler>();

        repeatRate = 1 / SimulationUpdateFrequencyHz;

        positionSensitivity = 1 / visibilityHandler.resolution;

        SignboardAgentViews = new int[GetVisibilityHandler().agentTypes.Length, GetVisibilityHandler().GetSignageBoardCount()];

        if(IsSimulationEnabled()) {
            StartSimulation();
        }
    }

    public VisibilityPlaneGenerator GetVisibilityPlaneGenerator() {
        return visibilityPlaneGenerator;
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
        Debug.Log("Simulation Started");
    }

    public void StopSimulation() {
        for(int agentID = 0; agentID < agentsSpawnHandler.GetAgentsCount(); agentID++) {
            SimulationAgent agent = agentsSpawnHandler.GetAgentsTranform(agentID).gameObject.GetComponent<SimulationAgent>();
            agent.StopSimulation();
        }
        Debug.Log("Simulation Stopped");
    }

    public List<int> GetSignageBoardsVisible(Vector3 agentPosition, int agentTypeID) {
        if(visibilityPlaneGenerator == null) {
            Debug.Log("visibilityPlaneGenerator NULL");
        }
        if(visibilityPlaneGenerator.GetVisibilityPlanesGroup() == null) {
            Debug.Log("GetVisibilityPlanesGroup NULL");
        }
        for(int visPlaneId = 0; visPlaneId < visibilityPlaneGenerator.GetVisibilityPlanesGroup().transform.childCount; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = visibilityHandler.visibilityInfos[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfos[agentTypeID];
            foreach(VisibilityInfo value in visInfoDictionary.Values) {
                Vector2 worldPos2D = new Vector2(value.cachedWorldPos.x, value.cachedWorldPos.z);
                Vector2 agentPos2D = new Vector2(agentPosition.x, agentPosition.z);
                if(Vector3.Distance(worldPos2D, agentPos2D) < positionSensitivity) {
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

        SignboardAgentViews[agentTypeID, signboardID]++;

        Debug.Log("Agent " + agent.name + " stayed in range of " + signageBoard.name + " for " + residenceTime + " seconds");
    }
}
