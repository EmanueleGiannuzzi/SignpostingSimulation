using System.Collections.Generic;
using UnityEngine;

public class InputArea : SpawnAreaBase, IRouteMarker {
    Vector3 IRouteMarker.Position => transform.position;

    private AutomaticMarkerGenerator routeGenerator;

    void Start() {
        base.Start();
        routeGenerator = FindObjectOfType<AutomaticMarkerGenerator>();
    }

    private GameObject SpawnRoutedAgent(GameObject agentPrefab) {
        GameObject agent = SpawnAgent(agentPrefab);
        
        Queue<IRouteMarker> route = routeGenerator.GetNewAgentRoute(this);
        agent.GetComponent<RoutedAgent>().SetRoute(route);
        
        return agent;
    }
    
    public override GameObject SpawnAgentEvent(GameObject agentPrefab) {
        return SpawnRoutedAgent(agentPrefab);
    }
}
