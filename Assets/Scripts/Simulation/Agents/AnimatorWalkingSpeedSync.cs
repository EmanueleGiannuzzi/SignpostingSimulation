
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class AnimatorWalkingSpeedSync : MonoBehaviour {
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    private void Start() {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        animator.SetFloat("Speed", navMeshAgent.desiredVelocity.magnitude);
    }
}