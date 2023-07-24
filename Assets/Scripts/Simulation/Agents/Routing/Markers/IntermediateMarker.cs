using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IntermediateMarker : MonoBehaviour, IRouteMarker {
    Vector3 IRouteMarker.Position => transform.position;

    private new Collider collider;

    private void Start() {
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            throw new ArgumentException($"IntermediateMarker: {gameObject.name} has no trigger collider");
        }
    }
}
