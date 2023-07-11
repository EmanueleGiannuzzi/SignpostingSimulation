using System;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class RouteMarker : MonoBehaviour {
    private Collider collider;

    private void Start() {
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            throw new ArgumentException($"RouteMarker: {gameObject.name} has no trigger collider");
        }
    }
}