
using System;
using System.Collections.Generic;

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
            g.addArc("'virtual start'", nVertices, startVertex, cost);
            g.addArc("'virtual end'", 
                g.umbalancedVerticesNeg.Length == 0 ? startVertex : g.umbalancedVerticesNeg[i], nVertices, cost); // graph is Eulerian if neg.length=0
            g.solve();
            if( bestGraph == null || bestCost > g.cost() ) {
                bestCost = g.cost();
                bestGraph = g;
            }
        } while(++i < g.umbalancedVerticesNeg.Length);
        return bestGraph.getCPT(nVertices);
    }
}