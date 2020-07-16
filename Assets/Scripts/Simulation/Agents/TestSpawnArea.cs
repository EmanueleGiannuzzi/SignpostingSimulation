using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnArea : SpawnArea
{
    public Camera PlayerCamera;
    public GameObject AgentPrefab;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit)) {
                SpawnAgentMoveTo(AgentPrefab, hit.point);
            }
        }
    }
}
