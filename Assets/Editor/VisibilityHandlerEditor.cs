using UnityEngine;
using UnityEditor;

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
        DrawDefaultInspector();

        if(GUILayout.Button("Init")) {
            handler.Init();
        }

        if(GUILayout.Button("Calculate Visibility Data")) {
            handler.GenerateVisibilityData();
        }

        GUILayout.BeginHorizontal(/*"box"*/);
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


    }

    void OnEnable() {
        handler = (VisibilityHandler)target;
    }
}
