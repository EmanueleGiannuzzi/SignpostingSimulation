﻿using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AgentCollisionEvent : UnityEvent<NavMeshAgent, EventAgentTriggerCollider> { }

[RequireComponent(typeof(Collider))]
public class EventAgentTriggerCollider : MonoBehaviour {
    public readonly AgentCollisionEvent collisionEvent = new ();
    private new Collider collider;

    private void Start() {
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            collider.isTrigger = true;
            Debug.LogWarning($"Collier with {nameof(EventAgentTriggerCollider)} component is not a trigger. Changing to trigger.");
        }
    }

    public void OnAgentCrossed(NavMeshAgent agent) {
        collisionEvent.Invoke(agent, this);
    }
}