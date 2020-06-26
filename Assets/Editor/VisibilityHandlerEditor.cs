using UnityEngine;
using UnityEditor;
using UnityEngine.XR;

[CustomEditor(typeof(VisibilityHandler))]
public class VisibilityHandlerEditor : Editor {

    VisibilityHandler handler;
    float showPlaneSliderValue = 0;

    public override void OnInspectorGUI() {
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

        GUILayout.BeginHorizontal("box");
        if(GUILayout.Button("Show Plane: " + showPlaneSliderValue)) {
            handler.Init();
            handler.GenerateVisibilityData();
            handler.ShowVisibilityPlane(0);
        }
        if(handler.agentTypes.Length > 1) {
            showPlaneSliderValue = GUILayout.HorizontalSlider(showPlaneSliderValue, 0, handler.GetVisibilityInfo().Length);
            showPlaneSliderValue = Mathf.Round(showPlaneSliderValue);
        }
        GUILayout.EndHorizontal();

        if(handler.progressAnalysis > -1f) {
            if(handler.progressAnalysis > 0f) {
                EditorUtility.DisplayCancelableProgressBar("Simple Progress Bar", "Shows a progress bar for the given seconds", 1f - handler.progressAnalysis);
            }
            else {
                EditorUtility.ClearProgressBar();
                handler.progressAnalysis = -1f;
            }
        }

    }

    void OnEnable() {
        handler = (VisibilityHandler)target;
    }
}
