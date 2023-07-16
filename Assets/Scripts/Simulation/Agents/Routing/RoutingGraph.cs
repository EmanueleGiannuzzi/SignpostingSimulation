using System;
using System.Collections.Generic;

public class RoutingGraph {
    private readonly Dictionary<int, List<Vertex>> vertices = new ();
    private readonly List<Edge> edges = new ();

    public void AddVertex(RouteMarker marker, string storeyName) {
        Vertex vertex= new Vertex(marker);
        int storeyHash = HashFunction(storeyName);
        
        if (!vertices.ContainsKey(storeyHash)) {
            vertices.Add(storeyHash, new List<Vertex>());
        }
        vertices[storeyHash].Add(vertex);
    }

    private int HashFunction(string str) {
        return str.GetHashCode();
    }


    private class Vertex {
        private RouteMarker marker;

        public Vertex(RouteMarker marker) {
            this.marker = marker;
        }
    }

    private class Edge {
        private Tuple<Vertex, Vertex> vertices;
        //private float weight;
    }
    
}
