using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class RoutedAgent : MonoBehaviour {
    private NavMeshAgent agent;

    private Queue<IRouteMarker> route;
    public float Error { get; set; } = 0f;
    
    // private const float ARRIVED_DISTANCE_SQR = 0.09f;

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
        SetDestination(route.Peek().Position);
    }

    private void OnExitReached([NotNull] IRouteMarker exit) {
        Destroy(this.gameObject);
    }

    public void SetRoute(Queue<IRouteMarker> newRoute) {
        route = newRoute;
        if (newRoute.TryPeek(out IRouteMarker destination)) {
            SetDestination(destination.Position);
        }
    }

    public void SetDestination(Vector3 destination) {
        if (Error != 0f) {
            float randXOffset = Random.Range(-Error, Error);
            float randZOffset = Random.Range(-Error, Error);
            destination = new Vector3(destination.x + randXOffset, destination.y, destination.z + randZOffset);
        }
        agent.SetDestination(destination);
    }

    public void AddDestination(IRouteMarker newDestination) {
        route.Enqueue(newDestination);
    }

    private void OnDrawGizmos() {
        if (route?.Count > 0) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}
