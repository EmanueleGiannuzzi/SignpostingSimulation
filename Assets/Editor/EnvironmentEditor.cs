using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Environment))]
public class EnvironmentEditor : Editor {
    Environment handler;
    float showPlaneSliderValue = 0;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Generate Visibility Planes")) {
            handler.GenerateVisibilityPlanes();
        }

        if(GUILayout.Button("Generate Signboard Grid")) {
            handler.GetSignboardGridGenerator().GenerateGrid();
        }

        if(GUILayout.Button("Calculate Visibility Data")) {
            handler.InitVisibilityHandlerData();
        }

        if(handler.GetVisibilityHandler() == null || handler.GetVisibilityHandler().agentTypes == null || handler.GetVisibilityHandler().agentTypes.Length <= 0) {
            GUI.enabled = false;
            GUILayout.Button("Show Plane: 0");
            GUI.enabled = true;
        }
        else { 
            GUILayout.BeginHorizontal("box");
            if(GUILayout.Button("Show Plane: " + showPlaneSliderValue)) {
                handler.GetVisibilityHandler().ShowVisibilityPlane((int)showPlaneSliderValue);
            }
            if(handler.GetVisibilityHandler().agentTypes.Length > 1) {
                showPlaneSliderValue = GUILayout.HorizontalSlider(showPlaneSliderValue, 0, handler.GetVisibilityHandler().agentTypes.Length - 1);
                showPlaneSliderValue = Mathf.Round(showPlaneSliderValue);
            }
            GUILayout.EndHorizontal();
        }
        if(GUILayout.Button("Clear Data")) {
            handler.ClearAllData();
        }
    }

    void OnEnable() {
        handler = (Environment)target;
    }
}
