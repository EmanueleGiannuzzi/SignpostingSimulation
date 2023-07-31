
using System.Collections.Generic;
using UnityEngine;

public class OpenCPT<T>: CPTGraph<T> {
    List<Arc> arcs = new ();
    
    class Arc {
        public string lab; 
        public int u, v; 
        public float cost;
        Arc(string lab, int u, int v, float cost)
        { this.lab = lab;
            this.u = u;
            this.v = v;
            this.cost = cost;
        }
    }
    
    public OpenCPT(int vertices) : base(vertices) {}
    
    float printCPT(int startVertex) { 
        CPTGraph<T> bestGraph = null, g;
        float bestCost = 0, cost;
        int i = 0;
        do { 
            g = new (nVertices+1);
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