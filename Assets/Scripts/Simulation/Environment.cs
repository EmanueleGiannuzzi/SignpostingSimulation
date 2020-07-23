using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    public bool isSimulationEnabled = false;

    public KeyCode Keybind;
    public int SimulationUpdateFrequencyHz;
    public float AgentFOVDegrees;
    [HideInInspector]
    public float repeatRate;// cached value: (1 / SimulationUpdateFrequencyHz)
    [HideInInspector]
    public float positionSensitivity;// Cahed value: (1 / visibilityHandler.resolution)

    public VisibilityPlaneGenerator visibilityPlaneGenerator;
    public VisibilityHandler visibilityHandler;
    private AgentsSpawnHandler agentsSpawnHandler;
    public SignboardGridGenerator signboardGridGenerator;

    private int agentSpawnedCount;
    private int[,] SignboardAgentViews; //[agentTypeID, signageBoardID]

    public SignageBoard[] signageBoards;

    public Environment() {
        visibilityHandler = new VisibilityHandler(this);
        visibilityPlaneGenerator = new VisibilityPlaneGenerator();
        signboardGridGenerator = new SignboardGridGenerator(this);
    }

    void Start() {
        agentsSpawnHandler = FindObjectOfType<AgentsSpawnHandler>();

        repeatRate = 1f / (float)SimulationUpdateFrequencyHz;

        positionSensitivity = 1f / (float)visibilityHandler.resolution;

        SignboardAgentViews = new int[GetVisibilityHandler().agentTypes.Length, signageBoards.Length];

        InitVisibilityHandlerData();

        if(IsSimulationEnabled()) {
            StartSimulation();
        }
    }
    public VisibilityHandler GetVisibilityHandler() {
        return visibilityHandler;
    }

    public VisibilityPlaneGenerator GetVisibilityPlaneGenerator() {
        return visibilityPlaneGenerator;
    }

    public SignboardGridGenerator GetSignboardGridGenerator() {
        return signboardGridGenerator;
    }

    public void InitVisibilityHandlerData() {
        if(visibilityHandler.agentTypes.Length <= 0) {
            Debug.LogError("No Agent Types found");
            return;
        }

        signageBoards = FindObjectsOfType<SignageBoard>();
        Debug.Log(signageBoards.Length + " Signage Boards found.");

        visibilityPlaneGenerator.GenerateVisibilityPlanes();
        if(visibilityPlaneGenerator.GetVisibilityPlanesGroup() != null && visibilityPlaneGenerator.GetVisibilityPlanesGroup().transform.childCount > 0) {
            Debug.Log("Visibility Planes Generated");
        }
        else {
            Debug.Log("Unable to generate Visibility Planes");
            return;
        }

        visibilityHandler.GenerateVisibilityData();
        visibilityHandler.ShowVisibilityPlane(0);
    }

    public void ClearAllData() {
        if(visibilityPlaneGenerator != null && visibilityPlaneGenerator.GetVisibilityPlanesGroup() != null) {
            DestroyImmediate(visibilityPlaneGenerator.GetVisibilityPlanesGroup());
        }
        visibilityHandler.ClearAllData();
    }

    void Update() {
        if(Input.GetKeyDown(Keybind)) {
            isSimulationEnabled = !isSimulationEnabled;
            if(isSimulationEnabled) {
                StartSimulation();
            }
            else {
                StopSimulation();
            }
        }
    }

    public bool IsSimulationEnabled() {
        return isSimulationEnabled;
    }

    public void StartSimulation() {
        this.agentSpawnedCount = 0;
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
        Debug.Log("Simulation Results:");

        for(int agentTypeID = 0; agentTypeID < GetVisibilityHandler().agentTypes.Length; agentTypeID++) {
            print("Agent Type: " + GetVisibilityHandler().agentTypes[agentTypeID].Key + "(" + GetVisibilityHandler().agentTypes[agentTypeID].Value + ")");
            for(int signageBoardID = 0; signageBoardID < signageBoards.Length; signageBoardID++) {
                print("SignageBoard: " + signageBoards[signageBoardID].gameObject.name + ": " + (((float)SignboardAgentViews[agentTypeID, signageBoardID] / (float)agentSpawnedCount) * 100) + "%");
            }
        }
    }

    public List<int> GetSignageBoardsVisible(Vector3 agentPosition, int agentTypeID) {
        if(visibilityPlaneGenerator == null) {
            Debug.LogError("visibilityPlaneGenerator NULL");
            return null;
        }
        if(visibilityPlaneGenerator.GetVisibilityPlanesGroup() == null) {
            Debug.LogError("GetVisibilityPlanesGroup NULL");
            return null;
        }
        for(int visPlaneId = 0; visPlaneId < visibilityPlaneGenerator.GetVisibilityPlanesGroup().transform.childCount; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = visibilityHandler.visibilityInfos[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfos[agentTypeID];
            foreach(VisibilityInfo value in visInfoDictionary.Values) {
                Vector2 worldPos2D = new Vector2(value.cachedWorldPos.x, value.cachedWorldPos.z);
                Vector2 agentPos2D = new Vector2(agentPosition.x, agentPosition.z);
                float distance = Vector3.Distance(worldPos2D, agentPos2D);
                if(distance < positionSensitivity) {
                    //Debug.Log("DISTANCE: " + distance);
                    return value.GetVisibleBoards();
                }
            }

        }
        return new List<int>();
    }

    public void OnAgentSpawned() {
        this.agentSpawnedCount++;
    }

    public void OnAgentEnterVisibilityArea(GameObject agent, int agentTypeID, int signboardID) {
        SignageBoard signageBoard = signageBoards[signboardID];
        //Debug.Log("Agent " + agent.name + " enter in range of " + signageBoard.name);
    }

    public void OnAgentExitVisibilityArea(GameObject agent, int agentTypeID, int signboardID, float residenceTime) {
        SignageBoard signageBoard = signageBoards[signboardID];

        if(residenceTime >= signageBoard.MinimumReadingTime) {
            SignboardAgentViews[agentTypeID, signboardID]++;
        }

        //Debug.Log("Agent " + agent.name + " stayed in range of " + signageBoard.name + " for " + residenceTime + " seconds");
    }
}
