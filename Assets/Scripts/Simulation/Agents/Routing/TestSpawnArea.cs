using System;
using UnityEngine;


public class TestSpawnArea : SpawnArea {
    private Camera PlayerCamera;
    public GameObject AgentPrefab;


    public enum MouseButton{LEFT = 0, RIGHT = 1, MIDDLE = 2} 
    public MouseButton mouseButton;

    private void Start() {
        PlayerCamera = FindObjectOfType<Camera>();
    }

    void Update() {
        if(Input.GetMouseButtonDown((int)mouseButton)) {
            Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit)) {
                SpawnAgentMoveTo(AgentPrefab, hit.point, null);
            }
        }
    }

    public override bool ShouldSpawnAgents() {
        return false;
    }
}
