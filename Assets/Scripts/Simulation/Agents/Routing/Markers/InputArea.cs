using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InputArea : SpawnAreaBase, IRouteMarker {
    Vector3 IRouteMarker.Position => transform.position;

    private RoutingGraphCPT routingGraph;
    private Queue<IRouteMarker> route;
    

    private void Awake() {
        //base.Start();
        MarkerGenerator markerGen = FindObjectOfType<MarkerGenerator>();
        if (markerGen == null) {
            Debug.LogError($"Unable to find {nameof(MarkerGenerator)}");
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

    private GameObject SpawnRoutedAgent(GameObject agentPrefab) {
        if (routingGraph == null) {
            Debug.LogError("Unable to find a Routing Graph to spawn the agent");
            return null;
        }
        GameObject agent = SpawnAgent(agentPrefab);
        
        agent.GetComponent<RoutedAgent>().SetRoute(new Queue<IRouteMarker>(route));
        
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
