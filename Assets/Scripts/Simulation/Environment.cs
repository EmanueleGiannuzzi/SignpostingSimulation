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

    public void OnAgentEnterVisibilityArea(Transform agent, int signboardID) {
        //Save agent pos
    }

    public void OnAgentExitVisibilityArea(Transform agent, int signboardID, float elapsedTime) {
        //remove saved agent pos and if elapsedTime > signageboard.minimumReadingTime set signageboard as seen
    }

    //private void OnAgentInCoord(Transform agent, int agentTypeID, Vector2Int coord) {
    //    List<int> visibleSignboards = visibilityHandler.GetSignboardIDsInCoord(coord, agentTypeID);
    //    //handle agent enterd/exited visible
    //}

    //public void AnalyzeCurrentAgentsStatus() {
    //    for(int agentID = 0; agentID < agentsSpawnHandler.GetAgentsCount(); agentID++) {
    //        Transform agent = agentsSpawnHandler.GetAgentsTranform(agentID);
    //        Vector2Int agentPos = new Vector2Int();//TODO: worldPos TO texturePos
    //        for(int agentTypeID = 0; agentTypeID < visibilityHandler.agentTypes.Length; agentTypeID++) {
    //            OnAgentInCoord(agent, agentTypeID, agentPos);
    //        }
    //    }
    //}
}
