
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OpenCPT {
    protected readonly List<Arc> arcs = new();
    protected int nVertices; // number of vertices

    protected class Arc {
        public string label; 
        public int u, v; 
        public float cost;

        internal Arc(string label, int u, int v, float cost) {
            this.label = label;
            this.u = u;
            this.v = v;
            this.cost = cost;
        }
    }

    protected OpenCPT(int nVertices) {
        this.nVertices = nVertices;
    }
    
    protected void addArc(string lab, int u, int v, float cost) {
        if (cost < 0) {
            throw new ArgumentException("Graph has negative costs");
        }
        arcs.Add(new Arc(lab, u, v, cost));
    }

    protected Queue<int> getOpenCPT(int startVertex) {
        CPTGraph bestGraph = null, g;
        float bestCost = 0, cost;
        int i = 0;
        do {
            g = new CPTGraph(nVertices+1);
            foreach (Arc arc in arcs) {
                g.addArc(arc.label, arc.u, arc.v, arc.cost);
            }
            cost = g.basicCost;
            g.findUnbalanced(); // initialise g.neg on original graph
            Debug.Log("1: L="   + g.unbalancedVerticesNeg.Length + " i=" + i);
            g.addArc("'virtual start'", nVertices, startVertex, cost);
            Debug.Log("2: L="   + g.unbalancedVerticesNeg.Length + " i=" + i);
            g.addArc("'virtual end'", 
                g.unbalancedVerticesNeg.Length <= 1 ? startVertex : g.unbalancedVerticesNeg[i], nVertices, cost); // graph is Eulerian if neg.length=0
            g.solve();
            if( bestGraph == null || bestCost > g.cost() ) {
                bestCost = g.cost();
                bestGraph = g;
            }
            
            
            if(EditorUtility.DisplayCancelableProgressBar("Path Generator", $"Generating Path ({i}/{g.unbalancedVerticesNeg.Length-1})", (float)i/g.unbalancedVerticesNeg.Length)) {
                EditorUtility.ClearProgressBar();
                return null;
            }
            Debug.Log("3: L="   + g.unbalancedVerticesNeg.Length + " i=" + i);
        } while(++i < g.unbalancedVerticesNeg.Length);
        
        EditorUtility.ClearProgressBar();
        return bestGraph.getCPT(nVertices);
    }
}