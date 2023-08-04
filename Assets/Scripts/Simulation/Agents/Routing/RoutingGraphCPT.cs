
using System;
using UnityEngine;
using UnityEngine.AI;

public class RoutingGraphCPT : OpenCPT<IRouteMarker> {
    public RoutingGraphCPT(IRouteMarker[] vertexLabels) : base(vertexLabels) {
        foreach (var vertex in vertexLabels) {
            generateEdges(vertex);
        }
    }

    public void banana() {
        if (VertLabels == null) {
            Debug.Log("BANANA");
        }
        
        for (int i = 0; i < nVertices; i++) {
            if (VertLabels[i] == null) {
                Debug.Log($"vertLabels[{i}] null");
            }
            else {
                Debug.Log("OK");
            }
        }
    }

    private void generateEdges(IRouteMarker vertex1) {
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
        addArc("", vertex2, vertex1, cost);
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
        const bool DEBUG = false;

        for (int i = 1; i < path.corners.Length; i++) {
            Vector3 directionTowardsNextCorner = (path.corners[i - 1] - path.corners[i]).normalized;
            float distanceToNextCorner = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (Physics.SphereCast(path.corners[i], SPHERE_RADIUS, directionTowardsNextCorner, out RaycastHit hit, distanceToNextCorner + 0.3f, MARKER_LAYER_MASK)) {
                IRouteMarker markerHit = hit.collider.GetComponent<IRouteMarker>();
                if (markerHit != null && markerHit != startMarker && markerHit != destinationMarker) {
                    if (DEBUG) {
                        Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.red, 60f, false);
                        DebugExtension.DebugWireSphere(path.corners[i], Color.red, SPHERE_RADIUS, 60f, false);
                        DebugExtension.DebugWireSphere(path.corners[i-1], Color.green, SPHERE_RADIUS, 60f, false);
                        Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.magenta, 60f, false);
                        DebugExtension.DebugWireSphere(markerHit.Position, Color.blue, SPHERE_RADIUS, 60f, false);
                    }
                    return true;
                }
            }

            if (DEBUG) {
                Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.green, 60f, false);
            }
        }

        return false;
    }
}