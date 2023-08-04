
using System.Collections.Generic;
using UnityEngine;

public class OpenCPT : CPTGraph {
    private readonly List<Arc> arcs = new();

    private class Arc {
        public string lab; 
        public int u, v; 
        public float cost;

        private Arc(string lab, int u, int v, float cost) {
            this.lab = lab;
            this.u = u;
            this.v = v;
            this.cost = cost;
        }
    }

    protected OpenCPT(int nVertices) : base(nVertices) {}

    protected Queue<int> getOpenCPT(int startVertex) {
        CPTGraph bestGraph = null, g;
        float bestCost = 0, cost;
        int i = 0;
        do {
            g = new CPTGraph(nVertices+1);
            for(int j = 0; j < arcs.Count; j++){ 
                Arc it = arcs[j];
                g.addArc(it.lab, it.u, it.v, it.cost);
            }
            cost = g.basicCost;
            g.findUnbalanced(); // initialise g.neg on original graph
            g.addArc("'virtual start'", nVertices, startVertex, cost);
            g.addArc("'virtual end'", 
                g.umbalancedVerticesNeg.Length == 0 ? startVertex : g.umbalancedVerticesNeg[i], nVertices, cost); // graph is Eulerian if neg.length=0
            g.solve();
            if( bestGraph == null || bestCost > g.cost() ) {
                bestCost = g.cost();
                bestGraph = g;
            }
        } while(++i < g.umbalancedVerticesNeg.Length);
        Debug.Log("Open CPT from "+startVertex+" (ignore virtual arcs)");
        return bestGraph.getCPT(nVertices);
    }
    
    private new float printCPT(int startVertex) { 
        CPTGraph bestGraph = null, g;
        float bestCost = 0, cost;
        int i = 0;
        do { 
            g = new CPTGraph(nVertices+1);
            for(int j = 0; j < arcs.Count; j++){ 
                Arc it = arcs[j];
                g.addArc(it.lab, it.u, it.v, it.cost);
            }
            cost = g.basicCost;
            g.findUnbalanced(); // initialise g.neg on original graph
            g.addArc("'virtual start'", nVertices, startVertex, cost);
            g.addArc("'virtual end'", g.umbalancedVerticesNeg.Length == 0 ? startVertex : g.umbalancedVerticesNeg[i], nVertices, cost); // graph is Eulerian if neg.length=0
            g.solve();
            if( bestGraph == null || bestCost > g.cost() )
            { bestCost = g.cost();
                bestGraph = g;
            }
        } while( ++i < g.umbalancedVerticesNeg.Length );
        Debug.Log("Open CPT from "+startVertex+" (ignore virtual arcs)");
        bestGraph.printCPT(nVertices);
        return cost+bestGraph.phi();
    }
}