using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoutingGraph {
    private readonly Dictionary<int, HashSet<Vertex>> vertices = new ();
    private readonly HashSet<Edge> edges = new ();

    public void AddVertex(RouteMarker marker, string storeyName) {
        Vertex vertex= new Vertex(marker);
        int storeyHash = HashFunction(storeyName);
        
        if (!vertices.ContainsKey(storeyHash)) {
            vertices.Add(storeyHash, new HashSet<Vertex>());
        }
        
        vertices[storeyHash].Add(vertex);
        GenerateEdges(vertex);
    }

    private void GenerateEdges(Vertex vertex1) {
        HashSet<Vertex> allVertices = new ();
        foreach (HashSet<Vertex> storeyVertices in vertices.Values) {
            allVertices.UnionWith(storeyVertices);
        }

        foreach (Vertex vertex2 in allVertices) {
            if (vertex1 == vertex2) {
                continue;
            }
            if (DoesPathExistsBetweenPoints(vertex1.Marker.transform.position, vertex2.Marker.transform.position)) {
                AddEdge(vertex1, vertex2);
            }
        }
    }

    private void AddEdge(Vertex vertex1, Vertex vertex2) {
        edges.Add(new Edge(vertex1, vertex2));
    }

    private bool DoesPathExistsBetweenPoints(Vector3 start, Vector3 destination) {
        return NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, new NavMeshPath());
    }

    private int HashFunction(string str) {
        return str.GetHashCode();
    }


    private class Vertex {
        public RouteMarker Marker { get; }

        public Vertex(RouteMarker marker) {
            this.Marker = marker;
        }
    }

    private class Edge {
        private Tuple<Vertex, Vertex> vertices;

        public Edge(Vertex vertex1, Vertex vertex2) {
            vertices = new Tuple<Vertex, Vertex>(vertex1, vertex2);
        }
        //private float weight;
    }
    
}
