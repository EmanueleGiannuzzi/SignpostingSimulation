using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisibilityPlaneGenerator))]
public class VisibilityPlaneGeneratorEditor : Editor
{
    VisibilityPlaneGenerator handler;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Generate Visibility Plane")) {
            handler.GenerateVisibilityPlanes();
        }
    }

    void OnEnable() {
        handler = (VisibilityPlaneGenerator)target;
    }
}
