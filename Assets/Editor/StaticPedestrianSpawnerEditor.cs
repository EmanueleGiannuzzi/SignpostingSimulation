using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StaticPedestrianSpawner))]
public class StaticPedestrianSpawnerEditor : Editor {
    private StaticPedestrianSpawner handler;
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Spawn Pedestrians")) {
            handler.SpawnPedestrians();
        }
        
    }

    private void OnEnable() {
        handler = (StaticPedestrianSpawner)target;
    }
}
