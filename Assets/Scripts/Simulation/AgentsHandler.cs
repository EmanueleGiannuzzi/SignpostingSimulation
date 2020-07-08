using UnityEngine;

public class AgentsHandler : MonoBehaviour
{
    [Header("Agent Movement")]
    public float Speed;
    public float AngularSpeed;
    public float Acceleration;

    public float AgentFOV = 120;//degrees

    public int SimulationQuality; //AgentPrefab -> Obstacle Avoidance -> Quality

}
