using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RoutedAgent : MonoBehaviour {
    private NavMeshAgent agent;

    private Queue<IRouteMarker> route;
    
    public Vector3 Target {
        get {
            route.TryPeek(out IRouteMarker destination);
            return destination.Position;
        }
    }
    
    void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnTriggerEnter(Collider other) {
        IRouteMarker marker = other.GetComponent<IRouteMarker>();
        if (marker == null || route is not { Count: > 0 } || marker != route.Peek()) {
            return;
        }
        
        IRouteMarker reachedMarker = route.Dequeue();
        if (route.Count > 0) {
            OnIntermediateMarkerReached(reachedMarker);
        }
        else {
            OnExitReached(reachedMarker);
        }
    }

    private void OnIntermediateMarkerReached([NotNull] IRouteMarker marker) {
        agent.SetDestination(route.Peek().Position);
    }

    private void OnExitReached([NotNull] IRouteMarker exit) {
        Destroy(this.gameObject);
    }
    
    public void SetRoute(Queue<IRouteMarker> newRoute) {
        route = newRoute;
        if (newRoute.TryPeek(out IRouteMarker destination)) {
            agent.SetDestination(destination.Position);
        }
    }

    // private void OnDrawGizmos() {
    //     if (route?.Count > 0) {
    //         Gizmos.DrawLine(transform.position, route.Peek().Position);
    //     }
    // }
}
