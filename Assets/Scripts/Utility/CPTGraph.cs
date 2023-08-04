
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CPTGraph<T> where T : class {
    protected T[] VertLabels { get; }

    protected int nVertices; // number of vertices
    private int[] vertDeltas; // deltas of vertices
    internal int[] umbalancedVerticesNeg; // unbalanced vertices
    private int[] umbalancedVerticesPos; // unbalanced vertices
    private int[,] adjMat; // adjacency matrix, counts arcs between vertices
    private List<string>[,] arcLabels; // vectors of labels of arcs (for each vertex pair)
    private int[,] repeatedArcs; // repeated arcs in CPT
    private float[,] arcCosts; // costs of cheapest arcs or paths
    private string[,] cheapestLabel; // labels of cheapest arcs
    private bool[,] pathDefined; // whether path cost is defined between vertices
    private int[,] spanningTree; // spanning tree of the graph
    internal float basicCost; // total cost of traversing each arc once

    private const int NONE = -1; // anything < 0
    
    internal CPTGraph(int nVertices) {
        if ((this.nVertices = nVertices) <= 0) {
            throw new Exception("Graph is empty");
        }

        vertDeltas = new int[nVertices];
        pathDefined = new bool[nVertices, nVertices];
        arcLabels = new List<string>[nVertices, nVertices];
        arcCosts = new float[nVertices, nVertices];
        repeatedArcs = new int[nVertices, nVertices];
        adjMat = new int[nVertices, nVertices];
        cheapestLabel = new string[nVertices, nVertices];
        spanningTree = new int[nVertices, nVertices];
        basicCost = 0;
    }

    protected CPTGraph(T[] vertexLabels) : this(vertexLabels.Length) {
        this.VertLabels = new T[vertexLabels.Length];
        Array.Copy(vertexLabels, this.VertLabels, vertexLabels.Length);
        
        // if(vertLabels == null)Debug.LogError("BANANA2");
        // else Debug.LogError("BANANA3");
        // foreach (var vert in vertLabels) {
        //     if (vert == null) {
        //         Debug.LogError("BANANA");
        //     }
        // }
    }

    public Tuple<T, T>[] GetArcs() {
        List<Tuple<T, T>> arcs = new();
        
        for (int i = 0; i < nVertices; i++) {
            for (int j = 0; j < nVertices; j++) {
                if (this.adjMat[i, j] > 0) {
                    Tuple<T, T> arc = new(VertLabels[i], VertLabels[j]);
                    arcs.Add(arc);
                }
            }
        }

        return arcs.ToArray();
    }

    protected internal void solve() {
        do {
            leastCostPaths();
            checkValid();
            findUnbalanced();
            findFeasible();
        } while (improvements());
    }
    

    protected int findVertex(T vertex) {
        for (int i = 0; i < nVertices; i++) {
            if (VertLabels[i] == vertex) {
                return i;
            }
        }

        throw new Exception($"Unable to find vertex label: {vertex}");
    }

    protected void addArc(string label, T u, T v, float cost) {
        int uPos = findVertex(u);
        int vPos = findVertex(v);
        addArc(label, uPos, vPos, cost);
    }

    protected internal void addArc(string lab, int u, int v, float cost) {
        if (!pathDefined[u,v]) {
            arcLabels[u,v] = new List<string>();
        }
        arcLabels[u,v].Add(lab);
        basicCost += cost;
        if( !pathDefined[u,v] || arcCosts[u,v] > cost )
        { arcCosts[u,v] = cost;
            cheapestLabel[u,v] = lab;
            pathDefined[u,v] = true;
            spanningTree[u,v] = v;
        }
        adjMat[u,v]++;
        vertDeltas[u]++;
        vertDeltas[v]--;
    }

    private void leastCostPaths() {
        for (int k = 0; k < nVertices; k++) {
            for (int i = 0; i < nVertices; i++) {
                if (pathDefined[i, k])
                    for (int j = 0; j < nVertices; j++) {
                        if (pathDefined[k, j] && (!pathDefined[i, j] || arcCosts[i, j] > arcCosts[i, k] + arcCosts[k, j])) {
                            spanningTree[i, j] = spanningTree[i, k];
                            arcCosts[i, j] = arcCosts[i, k] + arcCosts[k, j];
                            pathDefined[i, j] = true;
                            if (i == j && arcCosts[i, j] < 0) return; // stop on negative cycle
                        }
                    }
            }
        }
    }
    
    private void checkValid() {

        for(int i = 0; i < nVertices; i++) { 
            for(int j = 0; j < nVertices; j++){
                if (VertLabels != null && !pathDefined[i, j]) {
                    if (VertLabels[i] is IRouteMarker marker1 && VertLabels[j] is IRouteMarker marker2) {//TODO: REMOVE
                        Debug.DrawLine(marker1.Position, marker2.Position, Color.red, 30f, false);
                    }
                    throw new Exception("Graph is not strongly connected");
                }
                if (arcCosts[i, i] < 0) {
                    throw new Exception("Graph has a negative cycle");
                }
            }
        }
    }

    internal void findUnbalanced() { 
        int nn = 0 , np = 0 ; // number of vertices of negative/positive delta
        for (int i = 0; i < nVertices; i++) {
            if (vertDeltas[i] < 0) 
                nn++;
            else if (vertDeltas[i] > 0) 
                np++;
        }

        umbalancedVerticesNeg = new int[nn];
        umbalancedVerticesPos = new int[np];
        nn = np = 0 ;
        for (int i = 0; i < nVertices; i++) {
            // initialise sets
            if (vertDeltas[i] < 0) 
                umbalancedVerticesNeg[nn++] = i;
            else if 
                (vertDeltas[i] > 0) umbalancedVerticesPos[np++] = i;
        }
    }
    
    private void findFeasible() { 
        // delete next 3 lines to be faster, but non-reentrant
        int[] delta = new int[nVertices];
        for (int i = 0; i < nVertices; i++) {
            delta[i] = this.vertDeltas[i];
        }

        for(int u = 0; u < umbalancedVerticesNeg.Length; u++) {
            int i = umbalancedVerticesNeg[u];
            for(int v = 0; v < umbalancedVerticesPos.Length; v++) { 
                int j = umbalancedVerticesPos[v];
                repeatedArcs[i, j] = -delta[i] < delta[j]? -delta[i]: delta[j];
                delta[i] += repeatedArcs[i, j];
                delta[j] -= repeatedArcs[i, j];
            }
        }
    }

    private bool improvements() {
        CPTGraph<T> residual = new CPTGraph<T>(nVertices);
        for (int u = 0; u < umbalancedVerticesNeg.Length; u++) {
            int i = umbalancedVerticesNeg[u];
            for (int v = 0; v < umbalancedVerticesPos.Length; v++) {
                int j = umbalancedVerticesPos[v];
                residual.addArc(null, i, j, arcCosts[i, j]);
                if (repeatedArcs[i, j] != 0) {
                    residual.addArc(null, j, i, -arcCosts[i, j]);
                }
            }
        }

        residual.leastCostPaths(); // find a negative cycle
        for (int i = 0; i < nVertices; i++) {
            if (residual.arcCosts[i, i] < 0){ // cancel the cycle (if any)
                int k = 0, u, v;
                bool kunset = true;
                u = i;
                do{ // find k to cancel
                    v = residual.spanningTree[u, i];
                    if (residual.arcCosts[u, v] < 0 && (kunset || k > repeatedArcs[v, u])) {
                        k = repeatedArcs[v, u];
                        kunset = false;
                    }
                }while((u = v) != i);

                u = i;
                do{ // cancel k along the cycle
                    v = residual.spanningTree[u, i];
                    if (residual.arcCosts[u, v] < 0)
                        repeatedArcs[v, u] -= k;
                    else 
                        repeatedArcs[u, v] += k;
                }while((u = v) != i);

                return true; // have another go
            }
        }
        return false; // no improvements found
    }

    internal float cost() { 
        return basicCost+phi();
    }

    internal float phi(){
        float phi = 0;
        for (int i = 0; i < nVertices; i++) {
            for (int j = 0; j < nVertices; j++) {
                phi += arcCosts[i, j] * repeatedArcs[i, j];
            }
        }
        return phi;
    }
    
    private int findPath(int from, int[,] f) { // find a path between unbalanced vertices
        for (int i = 0; i < nVertices; i++) {
            if (f[from, i] > 0) {
                return i;
            }
        }
        return NONE;
    }
    
    protected internal Queue<int> getCPT(int startVertex) {
        Queue<int> pathCPT = new ();
        int v = startVertex;
        // delete next 7 lines to be faster, but non-reentrant
        int[,] arcs = new int[nVertices, nVertices];
        int[,] f = new int[nVertices, nVertices];
        for (int i = 0; i < nVertices; i++) {
            for (int j = 0; j < nVertices; j++) {
                arcs[i, j] = this.adjMat[i, j];
                f[i, j] = this.repeatedArcs[i, j];
            }
        }

        while(true) {
            int u = v;
            if((v = findPath(u, f)) != NONE) {
                f[u, v]--; // remove path
                for(int p; u != v; u = p){ // break down path into its arcs
                    p = spanningTree[u, v];
                    if (pathCPT.Count <= 0 || pathCPT.Peek() != u) {//TODO: Ignore virtual arcs
                        pathCPT.Enqueue(u);
                    }
                    if (pathCPT.Count <= 0 || pathCPT.Peek() != v) {
                        pathCPT.Enqueue(v);
                    }
                }
            }
            else{ 
                int bridgeVertex = spanningTree[u, startVertex];
                if (arcs[u, bridgeVertex] == 0) {
                    break; // finished if bridge already used
                }

                v = bridgeVertex;
                for( int i = 0; i < nVertices; i++ ) // find an unused arc, using bridge last
                    if( i != bridgeVertex && arcs[u, i] > 0) { 
                        v = i;
                        break;
                    }
                arcs[u, v]--; // decrement count of parallel arcs
                
                if (pathCPT.Count <= 0 || pathCPT.Peek() != u) {//TODO: Ignore virtual arcs
                    pathCPT.Enqueue(u);
                }
                if (pathCPT.Count <= 0 || pathCPT.Peek() != v) {
                    pathCPT.Enqueue(v);
                }
            }
        }

        return pathCPT;
    }
    
    protected internal void printCPT(int startVertex) { 
        int v = startVertex;
        // delete next 7 lines to be faster, but non-reentrant
        int[,] arcs = new int[nVertices, nVertices];
        int[,] f = new int[nVertices, nVertices];
        for (int i = 0; i < nVertices; i++) {
            for (int j = 0; j < nVertices; j++) {
                arcs[i, j] = this.adjMat[i, j];
                f[i, j] = this.repeatedArcs[i, j];
            }
        }

        while(true) { 
            int u = v;
            if((v = findPath(u, f)) != NONE) { 
                f[u, v]--; // remove path
                for(int p; u != v; u = p){ // break down path into its arcs
                    p = spanningTree[u, v];
                    Debug.Log("Take arc " + cheapestLabel[u, p] + " from " + u + " to " + p);
                }
            }
            else{ 
                int bridgeVertex = spanningTree[u, startVertex];
                if (arcs[u, bridgeVertex] == 0) {
                    break; // finished if bridge already used
                }

                v = bridgeVertex;
                for( int i = 0; i < nVertices; i++ ) // find an unused arc, using bridge last
                    if( i != bridgeVertex && arcs[u, i] > 0) { 
                        v = i;
                        break;
                    }
                arcs[u, v]--; // decrement count of parallel arcs
                Debug.Log("Take arc " + arcLabels[u, v][arcs[u, v]] + " from " + u + " to " + v); // use each arc label in turn
            }
        }
    }
}