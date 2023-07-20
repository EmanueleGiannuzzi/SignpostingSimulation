using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class RoutingGraph {
    public Dictionary<int, HashSet<Vertex>> Vertices { get; } = new();
    public HashSet<Edge> Edges { get; } = new();

    public void AddVertex(RouteMarker marker, string storeyName) {
        Vertex vertex= new Vertex(marker);
        int storeyHash = HashFunction(storeyName);
        
        if (!Vertices.ContainsKey(storeyHash)) {
            Vertices.Add(storeyHash, new HashSet<Vertex>());
        }
        
        Vertices[storeyHash].Add(vertex);
        GenerateEdges(vertex);
    }

    private void GenerateEdges(Vertex vertex1) {
        HashSet<Vertex> allVertices = new ();
        foreach (HashSet<Vertex> storeyVertices in Vertices.Values) {
            allVertices.UnionWith(storeyVertices);
        }

        foreach (Vertex vertex2 in allVertices) {
            if (vertex1 == vertex2) {
                continue;
            }
            if (DoesDirectPathExistsBetweenPoints(vertex1.Marker, vertex2.Marker)) {
                AddEdge(vertex1, vertex2);
            }
        }
    }

    private void AddEdge(Vertex vertex1, Vertex vertex2) {
        Edges.Add(new Edge(vertex1, vertex2));
    }

    private bool DoesDirectPathExistsBetweenPoints(RouteMarker startmarker, RouteMarker destinationMarker) {
        Vector3 start = startmarker.transform.position;
        Vector3 destination = destinationMarker.transform.position;
        
        NavMeshPath path = new ();
        NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, path);
        bool doesPathExists = path.status == NavMeshPathStatus.PathComplete;
        bool isDirect = !DoesPathIntersectOtherMarkers(path, startmarker, destinationMarker);

        // if (doesPathExists) {
        //     DrawPathGizmos(path);
        // }
        return doesPathExists && isDirect;
    }

    private void DrawPathGizmos(NavMeshPath path) {
        for (int i = 1; i < path.corners.Length; i++) {
            Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.red, 30f, false);
        }
    }

    private bool DoesPathIntersectOtherMarkers(NavMeshPath path, RouteMarker startmarker, RouteMarker destinationMarker) {
        const int MARKER_LAYER_MASK = 1 << 10;
        const float SPHERE_RADIUS = 0.5f;
        const bool DEBUG = false;

        for (int i = 1; i < path.corners.Length; i++) {
            Vector3 directionTowardsNextCorner = (path.corners[i - 1] - path.corners[i]).normalized;
            float distanceToNextCorner = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (Physics.SphereCast(path.corners[i], SPHERE_RADIUS, directionTowardsNextCorner, out RaycastHit hit, distanceToNextCorner + 0.3f, MARKER_LAYER_MASK)) {
                RouteMarker markerHit = hit.collider.GetComponent<RouteMarker>();
                if (markerHit != null && markerHit != startmarker && markerHit != destinationMarker) {
                    if (DEBUG) {
                        Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.red, 60f, false);
                        DebugExtension.DebugWireSphere(path.corners[i], Color.red, SPHERE_RADIUS, 60f, false);
                        DebugExtension.DebugWireSphere(path.corners[i-1], Color.green, SPHERE_RADIUS, 60f, false);
                        Debug.DrawLine(path.corners[i - 1], path.corners[i], Color.magenta, 60f, false);
                        DebugExtension.DebugWireSphere(markerHit.transform.position, Color.blue, SPHERE_RADIUS, 60f, false);
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

    private int HashFunction(string str) {
        return str.GetHashCode();
    }


    public class Vertex {
        public RouteMarker Marker { get; }

        public Vertex(RouteMarker marker) {
            this.Marker = marker;
        }
    }

    public class Edge {
        private Tuple<Vertex, Vertex> vertices;
        //private float weight;
        
        public RouteMarker Vertex1 => vertices.Item1.Marker;
        public RouteMarker Vertex2 => vertices.Item2.Marker;

        public Edge(Vertex vertex1, Vertex vertex2) {
            vertices = new Tuple<Vertex, Vertex>(vertex1, vertex2);
        }

    }
    
}
