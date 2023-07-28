using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RoutingGraph {

    public class EdgeTo {
        public IRouteMarker Marker { get; }
        public float cost { get; }
        public EdgeTo(IRouteMarker marker, float cost) {
            Marker = marker;
            this.cost = cost;
        }
    }
    
    public Dictionary<IRouteMarker, List<EdgeTo>> AdjacencyList { get; } = new();
    
    public void AddVertex(IRouteMarker marker, string storeyName) {
        generateEdges(marker);
    }

    private void generateEdges(IRouteMarker vertex1) {
        
        var allVertices = AdjacencyList.Keys;
        AdjacencyList.Add(vertex1, new List<EdgeTo>());
        foreach (IRouteMarker vertex2 in allVertices) {
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
        AdjacencyList[vertex1].Add(new EdgeTo(vertex2, cost));
        AdjacencyList[vertex2].Add(new EdgeTo(vertex1, cost));
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
        bool doesPathExists = path.status == NavMeshPathStatus.PathComplete;
        bool isDirect = !doesPathIntersectOtherMarkers(path, startMarker, destinationMarker);

        // if (doesPathExists) {
        //     DrawPathGizmos(path);
        // }
        return doesPathExists && isDirect;
    }


    public List<IRouteMarker> GeneratePath(IRouteMarker start) {
        List<IRouteMarker> path = new();
        Queue<IRouteMarker> queue = new ();
        int visitedNodes = 0;
        Dictionary<IRouteMarker, bool> visited = AdjacencyList.Keys.ToDictionary(vertex => vertex, _ => false);

        queue.Enqueue(start);
        visited[start] = true;

        while (queue.Count > 0) {
            IRouteMarker currentNode = queue.Dequeue();
            if (currentNode != start) {
                path.Add(currentNode);
            }
            visitedNodes++;
            
            List<EdgeTo> edges = AdjacencyList[currentNode];
            edges.Shuffle();

            foreach (var edge in edges.Where(edge => !visited[edge.Marker])) {
                var neighbor = edge.Marker;
                queue.Enqueue(neighbor);
                visited[neighbor] = true;
            }

            if (visitedNodes >= AdjacencyList.Count) {
                break;
            }
        }
        
        return path;
    }

    private static void drawPathGizmos(NavMeshPath path) {
        for (int i = 1; i < path.corners.Length; i++) {
            Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.red, 30f, false);
        }
    }

    private  static bool doesPathIntersectOtherMarkers(NavMeshPath path, IRouteMarker startmarker, IRouteMarker destinationMarker) {
        const int MARKER_LAYER_MASK = 1 << 10;
        const float SPHERE_RADIUS = 0.5f;
        const bool DEBUG = false;

        for (int i = 1; i < path.corners.Length; i++) {
            Vector3 directionTowardsNextCorner = (path.corners[i - 1] - path.corners[i]).normalized;
            float distanceToNextCorner = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (Physics.SphereCast(path.corners[i], SPHERE_RADIUS, directionTowardsNextCorner, out RaycastHit hit, distanceToNextCorner + 0.3f, MARKER_LAYER_MASK)) {
                IRouteMarker markerHit = hit.collider.GetComponent<IRouteMarker>();
                if (markerHit != null && markerHit != startmarker && markerHit != destinationMarker) {
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

    private static int hashFunction(string str) {
        return str.GetHashCode();
    }
}
