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

    private const string CSV_DELIMITER = ";";
    
    [ReadOnly]
    public bool testStarted = false;
    [ReadOnly]
    public bool testFinished = false;

    public int numberOfTests = 1;
    public float testDurationSeconds = 60f * 2;
    private const float POSITION_SAVE_FREQUENCY_HZ = 5f;
    // private const float SPAWN_RATE_PED_PER_SEC = 0.65f;
    private const float SPAWN_RATE_PED_PER_SEC = 0.20f;
    // private const float DESTINATION_ERROR = 0.5f;
    private const float DESTINATION_ERROR = 0f;
    private int ACCEL_TEST_MAX_READS;

    private class AgentInfo {
        public readonly List<Vector2> agentPos = new ();
        public readonly EventAgentTriggerCollider startCheckpoint;
        public bool shouldLogTrajectory = false;
        public int crossingNumber = 0;
        
        public AgentInfo(EventAgentTriggerCollider startCheckpoint) {
            this.startCheckpoint = startCheckpoint;
        }
    }
    
    
    public UseCase SelectedAction;
    public LoggingType SelectedLoggingType;

    public enum UseCase {
        NONE,
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

        // foreach (NavMeshAgent agent in positionLog.Keys) {
        //     StopCoroutine(logAgentPosition(agent, positionLog[agent].startCheckpoint));
        //     Destroy(agent.gameObject);
        // }
        Debug.Log("Job's done");
    }

    private void onTestStarted() {
        Debug.Log("Test Started");
        testStarted = true;
        testFinished = false;
    }

    private void onCheckpointCrossed(NavMeshAgent agent, EventAgentTriggerCollider checkpointCrossed) {
        if (positionLog.ContainsKey(agent) ) {
            if (positionLog[agent].startCheckpoint != checkpointCrossed) {
                onFinishCrossed(agent, checkpointCrossed);
            }
        }
        else {
            onStartCrossed(agent, checkpointCrossed);
        }

        positionLog[agent].crossingNumber++;
        switch (SelectedLoggingType) {
            case LoggingType.ALL:
                positionLog[agent].shouldLogTrajectory = !positionLog[agent].shouldLogTrajectory;
                break;
            case LoggingType.LEFT_RIGHT_ONLY:
                bool shouldLogTrajectory = (positionLog[agent].crossingNumber - 1) % 4 == 0;
                positionLog[agent].shouldLogTrajectory = shouldLogTrajectory;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Debug.Log(positionLog[agent].shouldLogTrajectory);
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
            if (positionLog[agent].shouldLogTrajectory) {
                Vector3 pos = agent.transform.position;
                Vector3 offset = this.transform.position;
                
                positionLog[agent].agentPos.Add(new Vector2(pos.x - offset.x, pos.z - offset.z));
                Debug.Log("BANANA");
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

    private IEnumerator counterflowTest() {
        Queue<IRouteMarker> routeForward = new();
        Queue<IRouteMarker> routeBackwards = new();
        routeForward.Enqueue(AreaFinish);
        routeBackwards.Enqueue(AreaStart);
        
        while (!testFinished) {
            GameObject agent1 = AreaStart.SpawnAgent(AgentPrefab);
            GameObject agent2 = AreaFinish.SpawnAgent(AgentPrefab);
            if (agent1 != null)
                agent1.GetComponent<RoutedAgent>().SetRoute(routeForward);
            if (agent2 != null)
                agent2.GetComponent<RoutedAgent>().SetRoute(routeBackwards);
            
            yield return new WaitForSeconds(1 / SPAWN_RATE_PED_PER_SEC);
        }
    }

    private IEnumerator backAndForthTest() {
        onTestStarted();
        for (int i = 0; i < numberOfTests; i++) {
            Queue<IRouteMarker> route = new();
            for (int j = 0; j < 100; j++) {
                route.Enqueue(AreaFinish);
                route.Enqueue(AreaStart);
            }
        
            GameObject agentObject = AreaStart.SpawnAgent(AgentPrefab);
            if (agentObject != null) {
                RoutedAgent routedAgent = agentObject.GetComponent<RoutedAgent>();
                routedAgent.Error = DESTINATION_ERROR;
                routedAgent.SetRoute(route);
            }
            yield return new WaitForSeconds(testDurationSeconds);
            NavMeshAgent agent = agentObject.GetComponent<NavMeshAgent>();
            StopCoroutine(logAgentPosition(agent, positionLog[agent].startCheckpoint));
            Destroy(agentObject);
        }
        stopTest();
    }


    private void PerformAction(UseCase action) {
        switch (action) {
            case UseCase.NONE:
                break;
                // StartCoroutine(startAccelTest());
                // Invoke(nameof(stopAccelTest), testDurationSeconds);
                // break;
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

    public void ExportTrajectoriesCSV(string pathToFolder) {
        Debug.Log("Export Started " + Checkpoints.Length);
        
        Dictionary<NavMeshAgent, AgentInfo>[] directionalPosLogs = new Dictionary<NavMeshAgent, AgentInfo>[Checkpoints.Length];
        for (int i = 0; i < Checkpoints.Length; i++) {
            directionalPosLogs[i] = new Dictionary<NavMeshAgent, AgentInfo>();
            var checkpoint = Checkpoints[i];
            foreach (var pair in positionLog) {
                if (pair.Value.startCheckpoint.Equals(checkpoint)) {
                    directionalPosLogs[i].Add(pair.Key, pair.Value);
                }
            }
        }

        int directionIndex = 0;
        foreach (var posLog in directionalPosLogs) {
            if (posLog.IsNullOrEmpty())
                continue;
            
            using StreamWriter writer = new StreamWriter(Path.Combine(pathToFolder, $"Trajectories_{directionIndex}.csv"));
            using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = CSV_DELIMITER;
            csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
            csv.NextRecord();

            foreach (var agentInfo in posLog.Values) {
                // if (agentInfo.HasFinished()) {
                string rowX = "";
                string rowY = "";
                foreach (var agentPos in agentInfo.agentPos) {
                    rowX += agentPos.x + csv.Configuration.Delimiter;
                    rowY += agentPos.y + csv.Configuration.Delimiter;
                }
                csv.WriteField(rowX, false);
                csv.NextRecord();
                csv.WriteField(rowY, false);
                csv.NextRecord();
                // }
            }
            directionIndex++;
        }
        Debug.Log("Export Done");
    }
}
