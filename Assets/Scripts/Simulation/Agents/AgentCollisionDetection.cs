using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[System.Serializable]
public class CollisionEvent : UnityEvent<NavMeshAgent, Collider> { }

public class AgentCollisionDetection : MonoBehaviour {
    public CollisionEvent collisionEvent;
    public Collider destroyer;

    void Start() {
        if(collisionEvent == null) {
            collisionEvent = new CollisionEvent();
        }
    }

    private void OnTriggerEnter(Collider other) {
        collisionEvent.Invoke(this.gameObject.GetComponent<NavMeshAgent>(), other);

        if(destroyer != null && destroyer.Equals(other)) {
            Destroy(this.gameObject);
        }
    }
}
