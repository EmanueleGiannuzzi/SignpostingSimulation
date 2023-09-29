using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PedestrianSpeedMeasure))]
public class PedestrianSpeedMeasureEditor : Editor {
    private PedestrianSpeedMeasure handler;
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Start")) {
            handler.PerformSelectedAction();
        }
    }

    private void OnEnable() {
        handler = (PedestrianSpeedMeasure)target;
    }
}
