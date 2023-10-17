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
    public InputArea AreaStart;
    public InputArea AreaFinish;

    private readonly Dictionary<NavMeshAgent, AgentInfo> positionLog = new ();
    private readonly Dictionary<NavMeshAgent, List<float>> agentSpeedLog = new ();

    private const string CSV_DELIMITER = ",";
    

    [ReadOnly]
    public bool testStarted = false;
    private const float POSITION_SAVE_FREQUENCY_HZ = 5f;
    private const float TEST_DURATION_SECONDS = 60f * 2;
    private const float SPAWN_RATE_PED_PER_SEC = 0.65f;
    private int ACCEL_TEST_MAX_READS;

    private class AgentInfo {
        public string agentName;
        public readonly float startingTime;
        public List<Vector2> agentPos;
        public float time;
        public EventAgentTriggerCollider startCheckpoint;
        
        public AgentInfo(int agentID, float startingTime) {
            this.agentName = $"Agent{agentID}";
            this.startingTime = startingTime;
            this.agentPos = new List<Vector2>();
            time = -1f;
        }

        public AgentInfo(int agentID, float startingTime, EventAgentTriggerCollider startCheckpoint) 
            : this(agentID, startingTime) {
            this.startCheckpoint = startCheckpoint;
        }

        public bool HasFinished() {
            return time > 0f;
        }
    }
    
    
    public UseCase SelectedAction;

    public enum UseCase {
        NONE,
        ACCELERATION_TEST,
        COUNTERFLOW_TEST
    }


    private void Start() { 
        ACCEL_TEST_MAX_READS = Mathf.CeilToInt(1f / Time.fixedDeltaTime * TEST_DURATION_SECONDS);
        foreach (var checkpoint in Checkpoints) {
            checkpoint.collisionEvent.AddListener(onCheckpointCrossed);
        }
    }
    
    private void stopTest() {
        // StopCoroutine(logAgentPositions());
    }

    private void onTestStarted() {
        testStarted = true;
        Invoke(nameof(stopTest), TEST_DURATION_SECONDS);
        Debug.Log("Test Started");
    }


    private void onCheckpointCrossed(NavMeshAgent agent, Collider checkpointCrossed) {
        if (positionLog.ContainsKey(agent) ) {
            if (positionLog[agent].startCheckpoint != checkpointCrossed) {
                onFinishCrossed(agent, checkpointCrossed);
            }
        }
        else {
            onStartCrossed(agent, checkpointCrossed);
        }
    }
    
    private void onStartCrossed(NavMeshAgent agent, Collider checkpoint) {
        if (testStarted) {
            StartCoroutine(logAgentPosition(agent, checkpoint.GetComponent<EventAgentTriggerCollider>()));
        }
        // Debug.Log("Agent entered");
    }
    
    private void onFinishCrossed(NavMeshAgent agent, Collider checkpoint) {
        if (!testStarted && positionLog.Count <= 0) {
            onTestStarted();
        }
        
        if (testStarted && positionLog.ContainsKey(agent)) {
            float startTime = positionLog[agent].startingTime;
            float now = Time.time;
            float elapsed = now - startTime;

            positionLog[agent].time = elapsed;
            StopCoroutine(logAgentPosition(agent, positionLog[agent].startCheckpoint));
            // Debug.Log("Agent exited in " + elapsed);
        }
        StartCoroutine(destroyAgentDelayed(agent.gameObject, 2f));
    }

    private static IEnumerator destroyAgentDelayed(GameObject agent, float time) {
        yield return new WaitForSeconds(time);
        Destroy(agent.gameObject);
    }
    

    private IEnumerator startAccelTest() {
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
        positionLog.Add(agent, new AgentInfo(agent.GetHashCode(), Time.time, checkpoint));
        
        while (testStarted && !positionLog[agent].HasFinished() && agent != null) {
            Vector3 pos = agent.transform.position;
            Vector3 offset = this.transform.position;
            
            positionLog[agent].agentPos.Add(new(pos.x - offset.x, pos.z - offset.z));
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
        // Queue<IRouteMarker> routeForward = new();
        // Queue<IRouteMarker> routeBackwards = new();
        // routeForward.Enqueue(AreaFinish);
        // routeBackwards.Enqueue(AreaStart);
        
        testStarted = true;
        while (testStarted) {
            GameObject agent1 = AreaStart.SpawnAgent(AgentPrefab);
            GameObject agent2 = AreaFinish.SpawnAgent(AgentPrefab);
            setAgentDestination(agent1, AreaFinish);
            setAgentDestination(agent2, AreaStart);
            
            yield return new WaitForSeconds(1 / SPAWN_RATE_PED_PER_SEC);
        }
    }

    private void stopCounterflowTest() {
        StopCoroutine(counterflowTest());
        testStarted = false;
        Debug.Log("Job's done");
    }

    private void PerformAction(UseCase action) {
        switch (action) {
            case UseCase.NONE:
                break;
            case UseCase.ACCELERATION_TEST:
                StartCoroutine(startAccelTest());
                Invoke(nameof(stopAccelTest), TEST_DURATION_SECONDS);
                break;
            case UseCase.COUNTERFLOW_TEST:
                StartCoroutine(counterflowTest());
                Invoke(nameof(stopCounterflowTest), TEST_DURATION_SECONDS);
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
            using StreamWriter writer = new StreamWriter(Path.Combine(pathToFolder, $"Trajectories_{directionIndex}.csv"));
            using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = CSV_DELIMITER;
            csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
            csv.NextRecord();

            foreach (var agentInfo in posLog.Values) {
                if (agentInfo.HasFinished()) {
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
                }
            }
            directionIndex++;
        }
        Debug.Log("Export Done");
    }
}
