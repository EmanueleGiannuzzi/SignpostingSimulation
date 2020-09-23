using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Environment))]
public class EnvironmentEditor : Editor {
    Environment handler;
    float showPlaneSliderValue = 0;
    float showHeatmapSliderValue = 0;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Generate Visibility Planes")) {
            handler.GenerateVisibilityPlanes();
        }


        if(GUILayout.Button("Calculate Visibility Data")) {
            handler.InitVisibilityHandlerData();

            handler.visibilityHandler.ShowVisibilityPlane(0);
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



        if(GUILayout.Button("Generate Signboard Grid")) {
            handler.GetBestSignboardPosition().StartEvalutation();
            //handler.GetBestSignboardPosition().ShowVisibilityPlane(0); Must be at the end of the corutine
        }
        if(handler.GetBestSignboardPosition() == null || handler.GetSignboardGridGenerator().GetSignboardGridGroup() == null) {
            GUI.enabled = false;
            GUILayout.Button("Show Heatmap: 0");
            GUI.enabled = true;
        }
        else {
            GUILayout.BeginHorizontal("box");
            if(GUILayout.Button("Show Heatmap: " + showHeatmapSliderValue)) {
                handler.GetBestSignboardPosition().ShowVisibilityPlane((int)showHeatmapSliderValue);
            }
            if(handler.GetVisibilityHandler().agentTypes.Length > 1) {
                showHeatmapSliderValue = GUILayout.HorizontalSlider(showHeatmapSliderValue, 0, handler.GetVisibilityHandler().agentTypes.Length - 1);
                showHeatmapSliderValue = Mathf.Round(showHeatmapSliderValue);
            }
            GUILayout.EndHorizontal();
        }
    }

    void OnEnable() {
        handler = (Environment)target;
    }
}
