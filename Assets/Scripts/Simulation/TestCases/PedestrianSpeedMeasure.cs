using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using MyBox;
using UnityEngine;
using UnityEngine.AI;


public class PedestrianSpeedMeasure : MonoBehaviour {
    public GameObject AgentPrefab; 
    public EventAgentTriggerCollider StartCollider;
    public EventAgentTriggerCollider FinishCollider;
    public InputArea AreaStart;
    public InputArea AreaFinish;

    private readonly Dictionary<NavMeshAgent, AgentInfo> crossingsLog = new ();
    private readonly Dictionary<NavMeshAgent, AgentInfo> crossingsLog = new ();
    private readonly Dictionary<NavMeshAgent, List<float>> agentSpeedLog = new ();
    

    [ReadOnly]
    public bool testStarted = false;
    private const float POSITION_SAVE_FREQUENCY_HZ = 5f;
    private const float TEST_DURATION_SECONDS = 60f * 2;
    private const float SPAWN_RATE_PED_PER_SEC = 0.65f;
    
    private const float ACCEL_TEST_DURATION_SECONDS = 5f;
    private int ACCEL_TEST_MAX_READS;

    private class AgentInfo {
        public string agentName;
        public readonly float startingTime;
        public List<Vector2> agentPos;
        public float time;
        
        public AgentInfo() {}

        public AgentInfo(int agentID, float startingTime) {
            this.agentName = $"Agent{agentID}";
            this.startingTime = startingTime;
            this.agentPos = new List<Vector2>();
            time = 0f;
        }
    }
    
    
    public UseCase SelectedAction;

    public enum UseCase {
        NONE,
        ACCELERATION_TEST,
        COUNTERFLOW_TEST
    }


    private void Start() { 
        ACCEL_TEST_MAX_READS = Mathf.CeilToInt(1f / Time.fixedDeltaTime * ACCEL_TEST_DURATION_SECONDS);
        StartCollider.collisionEvent.AddListener(onStartCrossed);
        FinishCollider.collisionEvent.AddListener(onFinishCrossed);
    }

    private IEnumerator SaveAgentPositions() {
        if (!testStarted) {
            yield return null;
        }
        
        foreach (NavMeshAgent agent in crossingsLog.Keys) {
            crossingsLog[agent].agentPos.Add(agent.transform.position);
        }

        yield return new WaitForSeconds(1 / POSITION_SAVE_FREQUENCY_HZ);
    }

    private void stopTest() {
        StopCoroutine(SaveAgentPositions());
    }

    private void onTestStarted() {
        testStarted = true;
        StartCoroutine(SaveAgentPositions());
        Invoke(nameof(stopTest), TEST_DURATION_SECONDS);
    }

    private void onStartCrossed(NavMeshAgent agent, Collider triggerCollider) {
        if (crossingsLog.ContainsKey(agent)) {
            crossingsLog.Remove(agent);
        }

        if (!testStarted && crossingsLog.Count <= 0) {
            onTestStarted();
        }

        float now = Time.time;
        AgentInfo agentInfo = new AgentInfo(agent.GetHashCode(), now);
        crossingsLog.Add(agent, agentInfo);
        
        // Debug.Log("Agent entered");
    }

    private void onFinishCrossed(NavMeshAgent agent, Collider triggerCollider) {
        if (crossingsLog.ContainsKey(agent)) {
            float startTime = crossingsLog[agent].startingTime;
            float now = Time.time;
            float elapsed = now - startTime;

            crossingsLog[agent].time = elapsed;
            
            // Debug.Log("Agent exited in " + elapsed);
        }
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

    private IEnumerator counterflowTest() {
        Queue<IRouteMarker> routeForward = new();
        Queue<IRouteMarker> routeBackwards = new();
        routeForward.Enqueue(AreaFinish);
        routeBackwards.Enqueue(AreaStart);
        
        testStarted = true;
        while (testStarted) {
            AreaStart.SpawnRoutedAgent(AgentPrefab, routeForward);
            AreaFinish.SpawnRoutedAgent(AgentPrefab, routeBackwards);
            // StartCoroutine(logAgentSpeed(agent));
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

    public void ExportSpeedLogCSV(string pathToFile) {
        using StreamWriter writer = new StreamWriter(pathToFile);
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.Configuration.HasHeaderRecord = true;
        csv.Configuration.Delimiter = ";";

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

    public void ExportTrajectoriesCSV(string pathToFile) {
        using StreamWriter writer = new StreamWriter(pathToFile);
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        // csv.Configuration.RegisterClassMap(new PedestrianTestDataMap());
        // csv.Configuration.HasHeaderRecord = true;
        
        csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
        csv.NextRecord();

        foreach (AgentInfo agentInfo in crossingsLog.Values) {
            string row = "";
            foreach (Vector2 position in agentInfo.agentPos) {
                
            }
            csv.WriteRecord(agentInfo);
            csv.NextRecord();
        }
        // csv.WriteRecords(crossingsLog);
    }
    
    // private sealed class PedestrianTestDataMap : ClassMap<AgentInfo> {
    //     public PedestrianTestDataMap() {
    //         Map(agentInfo => agentInfo.agentName).Name("Name");
    //         Map(m => m.direction).Name("Direction");
    //         Map(m => m.agentPos).Name("Positions");
    //         Map(m => m.time).Name("Time");
    //     }
    // }
}
