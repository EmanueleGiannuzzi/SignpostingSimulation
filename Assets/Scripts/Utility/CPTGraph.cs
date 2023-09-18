
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CPTGraph {
    private int nVertices; // number of vertices
    private int[] vertDeltas; // deltas of vertices
    internal int[] unbalancedVerticesNeg; // unbalanced vertices
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

    protected internal void solve() {
        int step = 0;
        const int STEP_LIMIT = 1000;
        do {
            leastCostPaths();
            checkValid();
            findUnbalanced();
            findFeasible();
            step++;
            if(EditorUtility.DisplayCancelableProgressBar("Path Generator", $"Making Improvements ({step}/{STEP_LIMIT})", (float)step/STEP_LIMIT)) {
                EditorUtility.ClearProgressBar();
                return;
            }
        } while (step < STEP_LIMIT && improvements());
        EditorUtility.ClearProgressBar();
    }
    
    protected internal void addArc(string lab, int u, int v, float cost) {
        if (!pathDefined[u,v]) {
            arcLabels[u,v] = new List<string>();
        }
        arcLabels[u,v].Add(lab);
        basicCost += cost;
        if( !pathDefined[u,v] || arcCosts[u,v] > cost ) { 
            arcCosts[u,v] = cost;
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
                if (pathDefined[i, k]) {
                    for (int j = 0; j < nVertices; j++) {
                        if (pathDefined[k, j] &&
                            (!pathDefined[i, j] || arcCosts[i, j] > arcCosts[i, k] + arcCosts[k, j])) {
                            spanningTree[i, j] = spanningTree[i, k];
                            arcCosts[i, j] = arcCosts[i, k] + arcCosts[k, j];
                            pathDefined[i, j] = true;
                            if (i == j && arcCosts[i, j] < 0) {
                                return; // stop on negative cycle
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void checkValid() {
        for(int i = 0; i < nVertices; i++) { 
            for(int j = 0; j < nVertices; j++){
                if (!pathDefined[i, j]) {
                    throw new Exception($"Graph is not strongly connected. Unable to find path between {i} and {j}");
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

        unbalancedVerticesNeg = new int[nn];
        umbalancedVerticesPos = new int[np];
        nn = np = 0 ;
        for (int i = 0; i < nVertices; i++) {
            // initialise sets
            if (vertDeltas[i] < 0) 
                unbalancedVerticesNeg[nn++] = i;
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

        for(int u = 0; u < unbalancedVerticesNeg.Length; u++) {
            int i = unbalancedVerticesNeg[u];
            for(int v = 0; v < umbalancedVerticesPos.Length; v++) { 
                int j = umbalancedVerticesPos[v];
                repeatedArcs[i, j] = -delta[i] < delta[j]? -delta[i]: delta[j];
                delta[i] += repeatedArcs[i, j];
                delta[j] -= repeatedArcs[i, j];
            }
        }
    }

    private bool improvements() {
        CPTGraph residual = new CPTGraph(nVertices);
        foreach (var i in unbalancedVerticesNeg) {
            foreach (var j in umbalancedVerticesPos) {
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

    private float phi(){
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
        solve();
        
        Queue<int> pathCPT = new ();
        int v = startVertex;
        // delete next 7 lines to be faster, but non-reentrant
        int[,] adjMat = new int[nVertices, nVertices];
        int[,] repeatedArcs = new int[nVertices, nVertices];
        for (int i = 0; i < nVertices; i++) {
            for (int j = 0; j < nVertices; j++) {
                adjMat[i, j] = this.adjMat[i, j];
                repeatedArcs[i, j] = this.repeatedArcs[i, j];
            }
        }

        int last = -1;
        while(true) {
            int u = v;
            
            if((v = findPath(u, repeatedArcs)) != NONE) {
                repeatedArcs[u, v]--; // remove path
                if (last != u) {
                    pathCPT.Enqueue(u);
                    last = u;
                }
                for(int p; u != v; u = p){ // break down path into its arcs
                    p = spanningTree[u, v];
                    if (last != p) {
                        pathCPT.Enqueue(p);
                        last = p;
                    }
                }

                if (last != v) {
                    pathCPT.Enqueue(v);
                    last = v;
                }
            }
            else { 
                int bridgeVertex = spanningTree[u, startVertex];
                if (adjMat[u, bridgeVertex] == 0) {
                    break; // finished if bridge already used
                }

                v = bridgeVertex;
                for(int i = 0; i < nVertices; i++) // find an unused arc, using bridge last
                    if( i != bridgeVertex && adjMat[u, i] > 0) { 
                        v = i;
                        break;
                    }
                adjMat[u, v]--; // decrement count of parallel arcs
                
                if (last != u) {
                    pathCPT.Enqueue(u);
                    last = u;
                }
                if (last != v) {
                    pathCPT.Enqueue(v);
                    last = v;
                }
            }
        }

        return pathCPT;
    }
    
}