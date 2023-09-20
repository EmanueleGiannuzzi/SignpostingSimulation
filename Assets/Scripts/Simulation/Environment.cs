using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    public bool isSimulationEnabled = false;

    public KeyCode Keybind;
    public int SimulationUpdateFrequencyHz;
    public float AgentFOVDegrees;
    public float WarmupDurationSeconds;
    public float SimulationDurationSeconds;
    [HideInInspector]
    public float repeatRate;// cached value: (1 / SimulationUpdateFrequencyHz)
    [HideInInspector]
    public float positionSensitivity;// Cahed value: (1 / visibilityHandler.resolution)

    public VisibilityPlaneGenerator visibilityPlaneGenerator;
    public VisibilityHandler visibilityHandler;
    private AgentsSpawnHandler agentsSpawnHandler;
    public SignboardGridGenerator signboardGridGenerator;
    public BestSignboardPosition bestSignboardPosition;
    public TextureExporter textureExporter;

    private int agentSpawnedCount;
    private int[,] SignboardAgentViews; //[agentTypeID, signageBoardID]

    public SignBoard[] signageBoards;


    public Environment() {
        visibilityHandler = new VisibilityHandler(this);
        visibilityPlaneGenerator = new VisibilityPlaneGenerator();
        signboardGridGenerator = new SignboardGridGenerator(this);
        bestSignboardPosition = new BestSignboardPosition(this);
        textureExporter = new TextureExporter(this);
    }

    void Start() {
        agentsSpawnHandler = FindObjectOfType<AgentsSpawnHandler>();

        repeatRate = 1f / (float)SimulationUpdateFrequencyHz;

        positionSensitivity = 1f / (float)visibilityHandler.resolution;

        SignboardAgentViews = new int[GetVisibilityHandler().agentTypes.Length, signageBoards.Length];

        //GenerateVisibilityPlanes(); TODO: Enable
        
        //InitVisibilityHandlerData();

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

    public BestSignboardPosition GetBestSignboardPosition() {
        return bestSignboardPosition;
    }

    public void StartSpawnAgents() {
        agentsSpawnHandler.StartSpawn();
    }

    public void StopSpawnAgents() {
        agentsSpawnHandler.StopSpawn();
    }

    public void GenerateVisibilityPlanes() {
        visibilityPlaneGenerator.GenerateVisibilityPlanes(visibilityHandler.resolution);
        if(visibilityPlaneGenerator.GetVisibilityPlanesGroup() != null && visibilityPlaneGenerator.GetVisibilityPlanesGroup().transform.childCount > 0) {
            Debug.Log("Visibility Planes Generated");
        }
        else {
            Debug.LogError("Unable to generate Visibility Planes");
            return;
        }
    }

    public void InitVisibilityHandlerData() {
        if(visibilityHandler.agentTypes.Length <= 0) {
            Debug.LogError("No Agent Types found");
            return;
        }

        if(GetVisibilityPlaneGenerator().GetVisibilityPlanesGroup() == null) {
            Debug.LogError("Generate Visibility Planes first");
            return;
        }

        signageBoards = FindObjectsOfType<SignBoard>();
        Debug.Log(signageBoards.Length + " Signage Boards found.");

        visibilityHandler.GenerateVisibilityData();
    }

    public void ClearAllData() {
        if(visibilityPlaneGenerator != null && visibilityPlaneGenerator.GetVisibilityPlanesGroup() != null) {
            DestroyImmediate(visibilityPlaneGenerator.GetVisibilityPlanesGroup());
        }
        visibilityHandler.ClearAllData();
        if(signboardGridGenerator != null && signboardGridGenerator.GetSignboardGridGroup() != null) {
            signboardGridGenerator.DeleteObjects();
        }
    }

    void Update() {
        if(Input.GetKeyDown(Keybind)) {
            isSimulationEnabled = !isSimulationEnabled;
            if(isSimulationEnabled) {
                StartSimulation();
            }
            else {
                StopSimulation();
                StopAllCoroutines();
            }
        }
    }

    public bool IsSimulationEnabled() {
        return isSimulationEnabled;
    }

    public void RunSimulationForInspectorDuration() {
        RunSimulationForSeconds(this.SimulationDurationSeconds);
    }

    public void RunSimulationForSeconds(float dT) {
        StartCoroutine(CoroutineRunSimulationForSeconds(dT));
    }

    public void SimulationWarmup() {
        SimulationWarmup(this.WarmupDurationSeconds);
    }

    public void SimulationWarmup(float dT) {
        StartCoroutine(CoroutineSimulationWarmup(dT));
    }

    public IEnumerator CoroutineSimulationWarmup(float dT) {
        Debug.Log("Warmup Started");
        StartSpawnAgents();
        yield return new WaitForSeconds(dT);
        Debug.Log("Warmup Done");
    }

    public IEnumerator CoroutineRunSimulationForSeconds(float dT) {
        isSimulationEnabled = true;
        StartSimulation();
        yield return new WaitForSeconds(dT);
        StopSimulation();
        isSimulationEnabled = false;
    }

    public void StartSimulation() {
        this.agentSpawnedCount = 0;
        for(int agentID = 0; agentID < agentsSpawnHandler.GetAgentsCount(); agentID++) {
            SimulationAgent agent = agentsSpawnHandler.GetAgentsTransform(agentID).gameObject.GetComponent<SimulationAgent>();
            agent.OnStartSimulation(SimulationUpdateFrequencyHz);
        }
        Debug.Log("Simulation Started");
    }

    public void StopSimulation() {
        for(int agentID = 0; agentID < agentsSpawnHandler.GetAgentsCount(); agentID++) {
            SimulationAgent agent = agentsSpawnHandler.GetAgentsTransform(agentID).gameObject.GetComponent<SimulationAgent>();
            agent.OnSimulationStopped();
        }
        Debug.Log("Simulation Stopped");

        int agentTypeSize = GetVisibilityHandler().agentTypes.Length;

        for(int signageBoardID = 0; signageBoardID < signageBoards.Length; signageBoardID++) {
            SignBoard signboard = signageBoards[signageBoardID];
            signboard.visibilityPerAgentType = new float[agentTypeSize];
            for(int agentTypeID = 0; agentTypeID < agentTypeSize; agentTypeID++) {
            //  print("Agent Type: " + GetVisibilityHandler().agentTypes[agentTypeID].Key + "(" + GetVisibilityHandler().agentTypes[agentTypeID].Value + ")");
                //print("SignageBoard: " + signboard.gameObject.name + ": " + (((float)SignboardAgentViews[agentTypeID, signageBoardID] / (float)agentSpawnedCount) * 100) + "%");
                signboard.visibilityPerAgentType[agentTypeID] = (float)SignboardAgentViews[agentTypeID, signageBoardID] / (float)agentSpawnedCount;
                signboard.views = SignboardAgentViews[agentTypeID, signageBoardID];
                signboard.agentsSpawned = agentSpawnedCount;
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
        if(visibilityHandler.visibilityInfos == null) {
            return null;
        }
        for(int visPlaneId = 0; visPlaneId < visibilityPlaneGenerator.GetVisibilityPlanesGroup().transform.childCount; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = visibilityHandler.visibilityInfos[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfos[agentTypeID];
            foreach(VisibilityInfo value in visInfoDictionary.Values) {

                Vector2 heading;
                float distanceSquared;  

                heading.x = value.cachedWorldPos.x - agentPosition.x;
                heading.y = value.cachedWorldPos.z - agentPosition.z;

                distanceSquared = heading.x * heading.x + heading.y * heading.y;

                if(distanceSquared < (positionSensitivity * positionSensitivity)) {
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
        SignBoard signBoard = signageBoards[signboardID];
        //Debug.Log("Agent " + agent.name + " enter in range of " + signageBoard.name);
    }

    public void OnAgentExitVisibilityArea(GameObject agent, int agentTypeID, int signboardID, float residenceTime) {
        SignBoard signBoard = signageBoards[signboardID];

        if(residenceTime >= signBoard.MinimumReadingTime) {
            SignboardAgentViews[agentTypeID, signboardID]++;
        }

        //Debug.Log("Agent " + agent.name + " stayed in range of " + signageBoard.name + " for " + residenceTime + " seconds");
    }
}
