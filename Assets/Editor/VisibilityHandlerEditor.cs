using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VisibilityHandler))]
public class VisibilityHandlerEditor : Editor {

    VisibilityHandler handler;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        //if(DrawDefaultInspector()) {//If any value was changed
        //    if(handler.autoUpdate) {
        //        handler.Generate();
        //    }
        //}


        if(GUILayout.Button("Init")) {
            handler.Init();
        }

        if(GUILayout.Button("Calculate Visibility Data")) {
            handler.GenerateVisibilityData();
        }

        if(GUILayout.Button("Show Plane 0")) {
            handler.GenerateVisibilityData();
            handler.ShowVisibilityPlane(0);
        }

    }

    void OnEnable() {
        handler = (VisibilityHandler)target;
    }
}
