using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = Unity.Mathematics.Random;

public class InputArea : SpawnAreaBase, IRouteMarker {
    Vector3 IRouteMarker.Position => transform.position;

    private RoutingGraphCPT routingGraph;
    private Queue<IRouteMarker> route;
    
    private Random rand;
    

    private void Awake() {
        rand = new Random((uint)DateTime.Now.Millisecond);
        //base.Start();
        MarkerGenerator markerGen = FindObjectOfType<MarkerGenerator>();
        if (markerGen == null) {
            // Debug.LogError($"Unable to find {nameof(MarkerGenerator)}");
            return;
        }
        markerGen.OnMarkersGeneration += OnMarkersGenerated;
    }

    private void OnMarkersGenerated(List<IRouteMarker> markers) {
        List<IRouteMarker> markersConnected = new (markers);
        markersConnected.RemoveAll(marker => {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(((IRouteMarker)this).Position, marker.Position, NavMesh.AllAreas, path);
            bool pathExists = path.status == NavMeshPathStatus.PathComplete;
            if (!pathExists) {
                Debug.DrawLine(marker.Position, ((IRouteMarker)this).Position, Color.red, 120f, false);
            }
            return !pathExists;
        });
        markersConnected.Insert(0,this);
        routingGraph = new RoutingGraphCPT(markersConnected.ToArray());
        route = routingGraph.GetRoute(this);
    }

    public GameObject SpawnRoutedAgent(GameObject agentPrefab, IEnumerable<IRouteMarker> agentRoute) {
        GameObject agent = SpawnAgent(agentPrefab);
        if(agent != null)
            agent.GetComponent<RoutedAgent>().SetRoute(new Queue<IRouteMarker>(agentRoute));

        return agent;
    }

    public GameObject SpawnAgentWithDestination(GameObject agentPrefab, Vector3 destination) {
        GameObject agent = SpawnAgent(agentPrefab);
        if(agent != null)
            agent.GetComponent<RoutedAgent>().SetDestination(destination);

        return agent;
    }

    private int RandomExponential(int max, float rateParameter) {
        float randomUniform = rand.NextFloat(0f, 1f);
        float expValue = -Mathf.Log(1 - randomUniform) / rateParameter;
        return Mathf.Min(Mathf.FloorToInt(expValue) + 1, max);
    }
    
    private Queue<IRouteMarker> thinPath(Queue<IRouteMarker> path) {
        Queue<IRouteMarker> newPath = new Queue<IRouteMarker>(path);
        int maxItemsToRemove = (int)Mathf.Floor(newPath.Count * 0.8f);
        float rateParameter = 1 / (((float)maxItemsToRemove + 1) / 10);
        // int itemsToRemove = maxItemsToRemove - RandomExponential(maxItemsToRemove, rateParameter);
        int itemsToRemove = rand.NextInt(0, maxItemsToRemove);

        for (int i = 0; i < itemsToRemove; i++) {
            newPath.Dequeue();
        }
        return newPath;
    }

    [CanBeNull]
    private GameObject SpawnRoutedAgent(GameObject agentPrefab) {
        if (routingGraph == null) {
            Debug.LogError("Unable to find a Routing Graph to spawn the agent");
            return null;
        }
        GameObject agent = SpawnAgent(agentPrefab);
        if (agent == null) {
            return null;
        }
        
        RoutedAgent routedAgent = agent.GetComponent<RoutedAgent>();
        if (routedAgent != null && route != null) {
            routedAgent.SetRoute(thinPath(route));
        }
        else {
            Destroy(routedAgent.gameObject);
        }
        
        return agent;
    }
    
    public override GameObject SpawnAgentEvent(GameObject agentPrefab) {
        return SpawnRoutedAgent(agentPrefab);
    }
    
    private static void DrawLineBetweenMarkers(IRouteMarker marker1, IRouteMarker marker2) {
        Vector3 dir = marker2.Position - marker1.Position;
        Gizmos.DrawRay(marker1.Position, dir);
        Gizmos.color = Color.blue;
    }
    
    private void OnDrawGizmos() {
        if (routingGraph == null) {
            return;
        }
        foreach (var arc in routingGraph.GetArcs()) {
            DrawLineBetweenMarkers(arc.Item1, arc.Item2);
        }
    }

    public override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && routingGraph != null;
    }
}
