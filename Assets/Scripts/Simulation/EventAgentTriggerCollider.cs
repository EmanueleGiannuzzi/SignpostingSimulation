using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class EventAgentTriggerCollider : MonoBehaviour {
    [HideInInspector]
    public CollisionEvent collisionEvent;
    private new Collider collider;

    private void Start() {
        collisionEvent ??= new CollisionEvent();
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            collider.isTrigger = true;
            Debug.LogWarning($"Collier with {nameof(EventAgentTriggerCollider)} component is not a trigger. Changing to trigger.");
        }
    }

    public void OnAgentCrossed(NavMeshAgent agent) {
        collisionEvent.Invoke(agent, collider);
    }
}