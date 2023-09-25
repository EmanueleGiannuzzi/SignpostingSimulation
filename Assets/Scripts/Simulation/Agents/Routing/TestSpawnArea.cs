using System;
using UnityEngine;

internal enum UseCase {
    NONE,
    GO_TO,
    GO_TO_AND_BACK,
    DOUBLE_GO_TO_AND_BACK
}
static class useCaseExtension {
    public static void PerformAction(this UseCase action) {
        switch (action) {
            case UseCase.NONE:
                break;
            case UseCase.GO_TO:
                break;
            case UseCase.GO_TO_AND_BACK:
                break;
            case UseCase.DOUBLE_GO_TO_AND_BACK:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}

public class TestSpawnArea : SpawnArea {
    private Camera PlayerCamera;
    public GameObject AgentPrefab;

    [SerializeField]
    private UseCase SelectedAction;


    public enum MouseButton{LEFT = 0, RIGHT = 1, MIDDLE = 2} 
    public MouseButton mouseButton;

    private void Start() {
        PlayerCamera = FindObjectOfType<Camera>();
    }

    void Update() {
        if(Input.GetMouseButtonDown((int)mouseButton)) {
            Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit)) {
                Debug.Log("BANANA");
                SpawnAgentMoveTo(AgentPrefab, hit.point, null);
            }
        }
    }

    public override bool ShouldSpawnAgents() {
        return false;
    }
}
