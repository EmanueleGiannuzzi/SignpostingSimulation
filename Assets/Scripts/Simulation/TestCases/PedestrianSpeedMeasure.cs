using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using MyBox;
using UnityEngine;
using UnityEngine.AI;


public class PedestrianSpeedMeasure : MonoBehaviour {
    public GameObject AgentPrefab; 
    public EventAgentTriggerCollider[] Checkpoints;
    public InputArea[] InputAreas;
    private  InputArea AreaStart => InputAreas[0];
    private InputArea AreaFinish => InputAreas[1];

    private readonly Dictionary<NavMeshAgent, AgentInfo> positionLog = new ();
    private readonly Dictionary<NavMeshAgent, List<float>> agentSpeedLog = new ();

    private const string CSV_DELIMITER = ",";
    
    [ReadOnly]
    public bool testStarted = false;
    [ReadOnly]
    public bool testFinished = false;
    [ReadOnly]
    public int TestDone = 0;

    public int numberOfTests = 1;
    public float testDurationSeconds = 60f * 2;
    private const float POSITION_SAVE_FREQUENCY_HZ = 5f;
    // private const float SPAWN_RATE_PED_PER_SEC = 0.65f;
    private const float SPAWN_RATE_PED_PER_SEC = 0.20f;
    private const float DESTINATION_ERROR = 0.2f;
    private int ACCEL_TEST_MAX_READS;

    [HideInInspector]
    public string pathToCSV;
    
    private int chooseLeft = 0; 
    private int chooseRight = 0; 
    
    private class AgentInfo {
        public List<List<Vector2>> AgentPos { get; } = new();
        public EventAgentTriggerCollider StartCheckpoint { get; }
        public bool ShouldLogTrajectory = false;
        public int CrossingNumber = 0;
        private List<Vector2> currentTrajectory;

        public AgentInfo(EventAgentTriggerCollider startCheckpoint) {
            StartCheckpoint = startCheckpoint;
            NextTrajectory();
        }

        public void NextTrajectory() {
            List<Vector2> trajectory = new();
            AgentPos.Add(trajectory);
            currentTrajectory = trajectory;
        }

        public void AddPosition(Vector2 pos) {
            currentTrajectory.Add(pos);
        }
    }
    
    public UseCase SelectedAction;
    public LoggingType SelectedLoggingType;

    public enum UseCase {
        NONE,
        ACCELERATION_TEST,
        BACK_AND_FORTH,
        COUNTERFLOW
    }

    public enum LoggingType {
        ALL,
        LEFT_RIGHT_ONLY
    }


    private void Start() { 
        ACCEL_TEST_MAX_READS = Mathf.CeilToInt(1f / Time.fixedDeltaTime * testDurationSeconds);
        foreach (var checkpoint in Checkpoints) {
            checkpoint.collisionEvent.AddListener(onCheckpointCrossed);
        }
    }
    
    private void stopTest() {
        testStarted = false;
        testFinished = true;
        TestDone = 0;
        Debug.Log("Job's done");
    }

    private void onTestStarted() {
        Debug.Log("Test Started");
        testStarted = true;
        testFinished = false;
        chooseLeft = 0;
        chooseRight = 0;
        positionLog.Clear();
        agentSpeedLog.Clear();
    }

    private void onCheckpointCrossed(NavMeshAgent agent, EventAgentTriggerCollider checkpointCrossed) {
        if (positionLog.TryGetValue(agent, out var agentInfo) ) {
            if (agentInfo.StartCheckpoint != checkpointCrossed) {
                onFinishCrossed(agent, checkpointCrossed);
            }
        }
        else {
            onStartCrossed(agent, checkpointCrossed);
        }

        positionLog[agent].CrossingNumber++;
        switch (SelectedLoggingType) {
            case LoggingType.ALL:
                positionLog[agent].ShouldLogTrajectory = !positionLog[agent].ShouldLogTrajectory;
                break;
            case LoggingType.LEFT_RIGHT_ONLY:
                bool shouldLogTrajectory = (positionLog[agent].CrossingNumber - 1) % 4 == 0;
                positionLog[agent].ShouldLogTrajectory = shouldLogTrajectory;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (positionLog[agent].ShouldLogTrajectory) {
            positionLog[agent].NextTrajectory();
        }
    }
    
    private void onStartCrossed(NavMeshAgent agent, EventAgentTriggerCollider checkpoint) {
        if (!positionLog.ContainsKey(agent)) {
            StartCoroutine(logAgentPosition(agent, checkpoint));
        }
    }

    private void onFinishCrossed(NavMeshAgent agent, EventAgentTriggerCollider checkpoint) {
        
    }
    
    private IEnumerator startAccelTest() {
        Debug.Log("Accel Test Started");
        Queue<IRouteMarker> route = new();
        route.Enqueue(AreaFinish);
        testStarted = true;
        while (testStarted) {
            NavMeshAgent agent = AreaStart.SpawnRoutedAgent(AgentPrefab, route).GetComponent<NavMeshAgent>();
            StartCoroutine(logAgentSpeed(agent));
            yield return new WaitForSeconds(1 / SPAWN_RATE_PED_PER_SEC);
        }
    }

    private void stopAccelTest() {
        StopAllCoroutines();
        testStarted = false;
        stopTest();
        Debug.Log("Job's done");
    }

    private IEnumerator logAgentSpeed(NavMeshAgent agent) {
        agentSpeedLog.Add(agent, new List<float>());
        int i = 0;
        while (i < ACCEL_TEST_MAX_READS && agent != null) {
            float speed = agent.velocity.magnitude;
            agentSpeedLog[agent].Add(speed);
            i++;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator logAgentPosition(NavMeshAgent agent, EventAgentTriggerCollider checkpoint) {
        positionLog.Add(agent, new AgentInfo(checkpoint));
        while (testStarted && agent != null) {
            if (positionLog[agent].ShouldLogTrajectory) {
                Vector3 pos = agent.transform.position;
                Vector3 offset = transform.position;
                
                positionLog[agent].AddPosition(new Vector2(pos.z - offset.z, pos.x - offset.x));
            }
            yield return new WaitForSeconds(1f / POSITION_SAVE_FREQUENCY_HZ);
        }
    }

    private void setAgentDestination(GameObject agent, IRouteMarker routeDestination) {
        if (agent == null) {
            return;
        }

        Vector3 agentDestination = new Vector3(agent.transform.position.x, routeDestination.Position.y, routeDestination.Position.z);
        agent.GetComponent<RoutedAgent>().SetDestination(agentDestination);
    }

    private void SaveAgentLeftRightChoice(GameObject agentObject) {
        SocialForceAgent socialForceAgent = agentObject.GetComponent<SocialForceAgent>();
        chooseLeft += socialForceAgent.ChooseLeft;
        chooseRight += socialForceAgent.ChooseRight;
    }

    private GameObject setupAgent(Queue<IRouteMarker> route, InputArea startingArea) {
        GameObject agentObject = startingArea.SpawnAgent(AgentPrefab);
        if (agentObject != null) {
            RoutedAgent routedAgent = agentObject.GetComponent<RoutedAgent>();
            routedAgent.Error = DESTINATION_ERROR;
            routedAgent.SetRoute(route);
        }
        return agentObject;
    }

    private void removeAgent(GameObject agentObject) {
        NavMeshAgent agent = agentObject.GetComponent<NavMeshAgent>();
        if (positionLog.ContainsKey(agent)) {
            StopCoroutine(logAgentPosition(agent, positionLog[agent].StartCheckpoint));
        }
        SaveAgentLeftRightChoice(agentObject);
        Destroy(agentObject);
    }

    private IEnumerator counterflowTest() {
        onTestStarted();
        for (int i = 0; i < numberOfTests; i++) {
            Queue<IRouteMarker> routeForward = new();
            Queue<IRouteMarker> routeBackwards = new();
            for (int j = 0; j < 500; j++) {
                routeForward.Enqueue(AreaFinish);
                routeForward.Enqueue(AreaStart);
                routeBackwards.Enqueue(AreaStart);
                routeBackwards.Enqueue(AreaFinish);
            }

            GameObject agentObj1, agentObj2;
            do {
                agentObj1 = setupAgent(routeForward, AreaStart);
                if (agentObj1 == null) {
                    yield return new WaitForSeconds(1f);
                }
            } while (agentObj1 == null);
            do {
                agentObj2 = setupAgent(routeBackwards, AreaFinish);
                if (agentObj2 == null) {
                    yield return new WaitForSeconds(1f);
                }
            } while (agentObj2 == null);

            yield return new WaitForSeconds(testDurationSeconds);
            
            removeAgent(agentObj1);
            removeAgent(agentObj2);
            TestDone++;
        }
        stopTest();
    }

    private IEnumerator backAndForthTest() {
        onTestStarted();
        for (int i = 0; i < numberOfTests; i++) {
            Queue<IRouteMarker> route = new();
            for (int j = 0; j < 500; j++) {
                route.Enqueue(AreaFinish);
                route.Enqueue(AreaStart);
            }
            GameObject agentObject;
            do {
                agentObject = setupAgent(route, AreaStart);
                if (agentObject == null) {
                    yield return new WaitForSeconds(1f);
                }
            } while (agentObject == null);
            yield return new WaitForSeconds(testDurationSeconds);
            
            removeAgent(agentObject);
            TestDone++;
        }
        stopTest();
    }


    private void PerformAction(UseCase action) {
        switch (action) {
            case UseCase.NONE:
                break;
            case UseCase.ACCELERATION_TEST:
                StartCoroutine(startAccelTest());
                Invoke(nameof(stopAccelTest), testDurationSeconds);
                break;
            case UseCase.BACK_AND_FORTH:
                StartCoroutine(backAndForthTest());
                break;
            case UseCase.COUNTERFLOW:
                StartCoroutine(counterflowTest());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public void PerformSelectedAction() {
        PerformAction(SelectedAction);
    }

    public void ExportSpeedLogCSV(string pathToFolder) {
        using StreamWriter writer = new StreamWriter(Path.Combine(pathToFolder, "AccelerationTest.csv"));
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.Configuration.HasHeaderRecord = true;
        csv.Configuration.Delimiter = CSV_DELIMITER;

        csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
        csv.NextRecord();
        
        string header = "Average" + csv.Configuration.Delimiter + "MIN" + csv.Configuration.Delimiter + "MAX";
        csv.WriteField(header, false);
        csv.NextRecord();

        foreach (var keyValuePair in agentSpeedLog.Where(kv => kv.Value.Count  < ACCEL_TEST_MAX_READS).ToList()) {
            Debug.Log(keyValuePair.Value.Count);
            agentSpeedLog.Remove(keyValuePair.Key);
        }

        List<float> minAll = new();
        List<float> maxAll = new();
        List<float> avgAll = new();
        
        for (int i = 0; i < ACCEL_TEST_MAX_READS; i++) {
            float min = float.PositiveInfinity;
            float max = -float.PositiveInfinity;
            float avg = 0f;
            foreach (NavMeshAgent agent in agentSpeedLog.Keys) {
                float reading = agentSpeedLog[agent][i];
                if (reading < min) min = reading;
                if (reading > max) max = reading;
                avg += reading;
            }
            avg /= agentSpeedLog.Keys.Count;

            minAll.Add(min);
            maxAll.Add(max);
            avgAll.Add(avg);
        }

        for (int i = 0; i < avgAll.Count; i++) {
            csv.WriteField(avgAll[i] + csv.Configuration.Delimiter + minAll[i] + csv.Configuration.Delimiter + maxAll[i], false);
            csv.NextRecord();
        }
    }

    private static bool isPositiveTrajectory(IReadOnlyList<Vector2> trajectory) {
        float[] yAxis = new float[trajectory.Count];
        for (int i = 0; i < yAxis.Length; i++) {
            yAxis[i] = trajectory[i].y;
        }
        return Mathf.Abs(yAxis.Max()) > Mathf.Abs(yAxis.Min());
    }

    public void ExportLeftRightCount(string pathToFolder) {
        using StreamWriter writer = new StreamWriter(Path.Combine(pathToFolder, "LeftRight.csv"));
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.Configuration.HasHeaderRecord = true;
        csv.Configuration.Delimiter = CSV_DELIMITER;

        csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
        csv.NextRecord();
        
        string header = "LEFT" + csv.Configuration.Delimiter + "RIGHT";
        csv.WriteField(header, false);
        csv.NextRecord();
        
        csv.WriteField(chooseLeft + csv.Configuration.Delimiter + chooseRight, false);
        csv.NextRecord();
        
        Debug.Log("LeftRight Export Done");
    }

    public void ExportTrajectoriesCSV(string pathToFolder) {
        Debug.Log("Export Started");

        Dictionary<NavMeshAgent, AgentInfo>[] directionalPosLogs = new Dictionary<NavMeshAgent, AgentInfo>[Checkpoints.Length];
        for (int i = 0; i < Checkpoints.Length; i++) {
            directionalPosLogs[i] = new Dictionary<NavMeshAgent, AgentInfo>();
            var checkpoint = Checkpoints[i];
            foreach (var pair in positionLog) {
                if (pair.Value.StartCheckpoint.Equals(checkpoint)) {
                    directionalPosLogs[i].Add(pair.Key, pair.Value);
                }
            }
        }

        CultureInfo cultureInfo = CultureInfo.InvariantCulture;
        int directionIndex = 0;
        foreach (var posLog in directionalPosLogs) {
            if (posLog.IsNullOrEmpty())
                continue;

            using StreamWriter writer = new StreamWriter(Path.Combine(pathToFolder, $"Trajectories_{directionIndex}.csv"));
            using CsvWriter csv = new CsvWriter(writer, cultureInfo);
            csv.Configuration.Delimiter = CSV_DELIMITER;
            csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
            csv.NextRecord();

            foreach (var agentInfo in posLog.Values) {
                foreach (List<Vector2> trajectory in agentInfo.AgentPos) {
                    if (trajectory.IsNullOrEmpty() || !isPositiveTrajectory(trajectory)) {
                        continue;
                    }
                    string rowX = "";
                    string rowY = "";
                    foreach (var agentPos in trajectory) {
                        rowX += agentPos.x.ToString(cultureInfo) + csv.Configuration.Delimiter;
                        rowY += agentPos.y.ToString(cultureInfo) + csv.Configuration.Delimiter;
                    }

                    csv.WriteField(rowX, false);
                    csv.NextRecord();
                    csv.WriteField(rowY, false);
                    csv.NextRecord();
                }
            }
            directionIndex++;
        }
        Debug.Log("Trajectory Export Done");
    }
}
