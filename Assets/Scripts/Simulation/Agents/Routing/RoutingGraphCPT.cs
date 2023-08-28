
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Vertx.Debugging;

// #if UNITY_EDITOR
// using Physics = Vertx.Debugging.DrawPhysics;
// #endif

public class RoutingGraphCPT : OpenCPT {
    private IRouteMarker[] VertLabels { get; }
    
    public RoutingGraphCPT(IRouteMarker[] vertexLabels) : base(vertexLabels.Length) {
        VertLabels = (IRouteMarker[])vertexLabels.Clone();
        foreach (var vertex in vertexLabels) {
            generateEdgesFrom(vertex);
        }
    }
    
    private void generateEdgesFrom(IRouteMarker vertex1) {
        foreach (IRouteMarker vertex2 in VertLabels) {
            if (vertex1 == vertex2) {
                continue;
            }
            if (doesDirectPathExistsBetweenPoints(vertex1, vertex2, out float cost)) {
                addEdge(vertex1, vertex2, cost);
            }
        }
    }
    
    private void addEdge(IRouteMarker vertex1, IRouteMarker vertex2, float cost) {
        if (cost < 0) {
            throw new ArgumentOutOfRangeException($"Edge can't have negative cost [{cost}]");
        }
        addArc("", vertex1, vertex2, cost);
    }
    
    private void addArc(string label, IRouteMarker u, IRouteMarker v, float cost) {
        int uPos = findVertex(u);
        int vPos = findVertex(v);
        base.addArc(label, uPos, vPos, cost);
        // base.addArc(label, vPos, uPos, cost);
    }
    
    private int findVertex(IRouteMarker vertex) {
        for (int i = 0; i < nVertices; i++) {
            if (VertLabels[i] == vertex) {
                return i;
            }
        }
        throw new Exception($"Unable to find vertex label: {vertex}");
    }

    private static float GetPathLengthSquared(NavMeshPath path) {
        Vector3[] corners = path.corners;

        float length = 0f;
        for (int i = 1; i < corners.Length; i++) {
            length += (corners[i] -  corners[i - 1]).sqrMagnitude;
        }

        return length;
    }

    private static bool doesDirectPathExistsBetweenPoints(IRouteMarker startMarker, IRouteMarker destinationMarker, out float cost) {
        Vector3 start = startMarker.Position;
        Vector3 destination = destinationMarker.Position;
        
        NavMeshPath path = new ();
        NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, path);
        cost = GetPathLengthSquared(path);
        bool pathExists = path.status == NavMeshPathStatus.PathComplete;
        bool isDirect = !doesPathIntersectOtherMarkers(path, startMarker, destinationMarker);

        return pathExists && isDirect;
    }
    
    private static bool doesPathIntersectOtherMarkers(NavMeshPath path, IRouteMarker startMarker, IRouteMarker destinationMarker) {
        const int MARKER_LAYER_MASK = 1 << 10;
        const float SPHERE_RADIUS = 0.5f;

        for (int i = 1; i < path.corners.Length; i++) {
            Vector3 directionTowardsNextCorner = (path.corners[i - 1] - path.corners[i]).normalized;
            float distanceToNextCorner = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            DrawPhysicsSettings.SetDuration(60f);
            if (Physics.SphereCast(path.corners[i], SPHERE_RADIUS, directionTowardsNextCorner, out RaycastHit hit, distanceToNextCorner + 0.3f, MARKER_LAYER_MASK)) {
                IRouteMarker markerHit = hit.collider.GetComponent<IRouteMarker>();
                if (markerHit != null && markerHit != startMarker && markerHit != destinationMarker) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public IEnumerable<Tuple<IRouteMarker, IRouteMarker>> GetArcs() {
        List<Tuple<IRouteMarker, IRouteMarker>> arcsObjs = new();

        foreach (Arc arc in this.arcs) {
            Tuple<IRouteMarker, IRouteMarker> arcObj = new(VertLabels[arc.u], VertLabels[arc.v]);
            arcsObjs.Add(arcObj);
        }
        
        return arcsObjs;
    }
    
    public Queue<IRouteMarker> GetRoute(IRouteMarker startVertex) {
        int startVertexPos = findVertex(startVertex);
        
        Queue<IRouteMarker> openCPT = new ();
        string debug = $"route[{nVertices}]: ";
        Queue<int> openCPTVertPos = getOpenCPT(startVertexPos);
        
        foreach (int vertexPos in openCPTVertPos) {
            
            debug += vertexPos + " ";
            if (vertexPos < nVertices) {
                openCPT.Enqueue(VertLabels[vertexPos]);
            }
        }
        Debug.Log(debug);
        
        openCPT.Dequeue();//remove start area
        return openCPT;
    }
}