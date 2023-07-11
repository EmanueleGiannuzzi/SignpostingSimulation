using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RoutedAgent : MonoBehaviour {
    private NavMeshAgent agent;

    private Queue<RouteMarker> route;
    
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        route = new Queue<RouteMarker>();
    }

    private void OnTriggerEnter(Collider other) {
        RouteMarker marker = other.GetComponent<RouteMarker>();
        if (marker == null) {
            return;
        }

        if(route.Count > 0 && marker == route.Peek()) {
            RouteMarker reachedMarker = route.Dequeue();
            if (route.Count > 0) {
                OnExitReached(reachedMarker);
            }
            else {
                OnIntermediateMarkerReached(reachedMarker);
            }
        }
    }

    private void OnIntermediateMarkerReached([NotNull] RouteMarker marker) {
        agent.SetDestination(route.Peek().transform.position);
    }

    private void OnExitReached([NotNull] RouteMarker exit) {
        Destroy(this.gameObject);
    }
    
    

    public void SetRoute(IEnumerable<RouteMarker> newRoute) {
        route = new Queue<RouteMarker>(newRoute);
    }
}
