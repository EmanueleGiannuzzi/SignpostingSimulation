using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

public class RoutingGraph {
    public Dictionary<int, HashSet<IRouteMarker>> Vertices { get; } = new();
    public Dictionary<IRouteMarker, List<IRouteMarker>> AdjacencyList { get; } = new();
    
    private readonly Random random = new ();

    public void AddVertex(IRouteMarker marker, string storeyName) {
        int storeyHash = hashFunction(storeyName);
        
        if (!Vertices.ContainsKey(storeyHash)) {
            Vertices.Add(storeyHash, new HashSet<IRouteMarker>());
        }
        
        Vertices[storeyHash].Add(marker);
        generateEdges(marker);
    }

    private void generateEdges(IRouteMarker vertex1) {
        HashSet<IRouteMarker> allVertices = new ();
        foreach (HashSet<IRouteMarker> storeyVertices in Vertices.Values) {
            allVertices.UnionWith(storeyVertices);
        }
        
        AdjacencyList.Add(vertex1, new List<IRouteMarker>());
        foreach (IRouteMarker vertex2 in allVertices) {
            if (vertex1 == vertex2) {
                continue;
            }
            if (doesDirectPathExistsBetweenPoints(vertex1, vertex2)) {
                addEdge(vertex1, vertex2);
            }
        }
    }

    private void addEdge(IRouteMarker vertex1, IRouteMarker vertex2) {

        if (!AdjacencyList[vertex1].Contains(vertex2)) {
            AdjacencyList[vertex1].Add(vertex2);
        }
        if (!AdjacencyList[vertex2].Contains(vertex1)) {
            AdjacencyList[vertex2].Add(vertex1);
        }
    }

    private static bool doesDirectPathExistsBetweenPoints(IRouteMarker startmarker, IRouteMarker destinationMarker) {
        Vector3 start = startmarker.Position;
        Vector3 destination = destinationMarker.Position;
        
        NavMeshPath path = new ();
        NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, path);
        bool doesPathExists = path.status == NavMeshPathStatus.PathComplete;
        bool isDirect = !doesPathIntersectOtherMarkers(path, startmarker, destinationMarker);

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
            
            List<IRouteMarker> neighbors = AdjacencyList[currentNode];
            neighbors.Shuffle();

            foreach (var neighbor in neighbors.Where(neighbor => !visited[neighbor])) {
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
