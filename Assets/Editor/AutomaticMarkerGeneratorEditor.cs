using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutomaticMarkerGenerator))]
public class AutomaticMarkerGeneratorEditor : Editor {
    AutomaticMarkerGenerator handler;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Markers")) {
            handler.AddMarkersToTraversables();
        }
    }

    void OnEnable() {
        handler = (AutomaticMarkerGenerator)target;
    }
}
