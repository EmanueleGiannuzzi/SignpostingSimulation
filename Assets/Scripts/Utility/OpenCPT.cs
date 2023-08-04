
using System.Collections.Generic;
using UnityEngine;

public class OpenCPT<T>: CPTGraph<T> where T : class {
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

    protected OpenCPT(T[] vertexLabels) : base(vertexLabels) {}

    public Queue<T> GetOpenCPT(T startVertex) {
        int startVertexPos = findVertex(startVertex);
        
        Queue<T> openCPT = new Queue<T>();
        string debug = "route: ";
        foreach (int vertexPos in getOpenCPT(startVertexPos)) {
            debug += vertexPos + " ";
            if (vertexPos < nVertices) {
                openCPT.Enqueue(VertLabels[vertexPos]);
            }
        }
        Debug.Log(debug);

        openCPT.Dequeue();//Remove starting area
        return openCPT;
    }
    
    private Queue<int> getOpenCPT(int startVertex) {
        CPTGraph<T> bestGraph = null, g;
        float bestCost = 0, cost;
        int i = 0;
        do {
            g = new CPTGraph<T>(nVertices+1);
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
        CPTGraph<T> bestGraph = null, g;
        float bestCost = 0, cost;
        int i = 0;
        do { 
            g = new CPTGraph<T>(nVertices+1);
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