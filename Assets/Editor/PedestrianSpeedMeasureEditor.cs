using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PedestrianSpeedMeasure))]
public class PedestrianSpeedMeasureEditor : Editor {
    private PedestrianSpeedMeasure handler;

    private string pathToCSV = "C:/Users/emagi/Desktop/PedestrianTest";
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(handler.SelectedAction != PedestrianSpeedMeasure.UseCase.NONE 
           && GUILayout.Button("Start")) {
            handler.PerformSelectedAction();
        }
        
        pathToCSV = EditorGUILayout.TextField("Path to CSV: ", pathToCSV);
        GUI.enabled = !handler.testStarted;
        if(GUILayout.Button("Export to CSV")) {
            switch (handler.SelectedAction) {
                case PedestrianSpeedMeasure.UseCase.NONE:
                    break;
                case PedestrianSpeedMeasure.UseCase.ACCELERATION_TEST:
                    handler.ExportSpeedLogCSV(pathToCSV);
                    break;
                case PedestrianSpeedMeasure.UseCase.COUNTERFLOW_TEST:
                    handler.ExportTrajectoriesCSV(pathToCSV);
                    break;
                default:
                    break;
            }
        }
        GUI.enabled = true;
    }

    private void OnEnable() {
        handler = (PedestrianSpeedMeasure)target;
    }
}
