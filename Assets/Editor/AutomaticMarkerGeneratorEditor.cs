using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MarkerGenerator))]
public class AutomaticMarkerGeneratorEditor : Editor {
    MarkerGenerator handler;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Markers")) {
            handler.AddMarkersToTraversables();
        }
    }

    void OnEnable() {
        handler = (MarkerGenerator)target;
    }
}
