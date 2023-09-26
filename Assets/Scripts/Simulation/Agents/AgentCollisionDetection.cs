using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[System.Serializable]
public class CollisionEvent : UnityEvent<NavMeshAgent, Collider> { }

public class AgentCollisionDetection : MonoBehaviour {
    public CollisionEvent collisionEvent;
    public Collider destroyer;

    private void Start() {
        collisionEvent ??= new CollisionEvent();
    }

    private void OnTriggerEnter(Collider other) {
        NavMeshAgent agent = this.gameObject.GetComponent<NavMeshAgent>();
        collisionEvent.Invoke(agent, other);

        if(destroyer != null && destroyer.Equals(other)) {
            Destroy(this.gameObject);
        }

        EventAgentTriggerCollider agentTriggerCollider = other.GetComponent<EventAgentTriggerCollider>();
        if (agentTriggerCollider != null) {
            agentTriggerCollider.OnAgentCrossed(agent);
        }
    }
}
