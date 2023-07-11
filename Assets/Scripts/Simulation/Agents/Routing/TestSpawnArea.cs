using UnityEngine;

public class TestSpawnArea : SpawnArea
{
    public Camera PlayerCamera;
    public GameObject AgentPrefab;

    [Range(0, 2)]
    public int mouseButton = 0;


    void Update()
    {
        if(Input.GetMouseButtonDown(mouseButton)) {
            Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit)) {
                SpawnAgentMoveTo(AgentPrefab, hit.point, null);
            }
        }
    }
}
