using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PedestrianSpeedMeasure))]
public class PedestrianSpeedMeasureEditor : Editor {
    private PedestrianSpeedMeasure handler;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        GUI.enabled = Application.isPlaying && handler.SelectedAction != PedestrianSpeedMeasure.UseCase.NONE;
        if(GUILayout.Button("Start")) {
            handler.PerformSelectedAction();
        }
        GUI.enabled = true;
        
        handler.pathToCSV = EditorGUILayout.TextField("Path to CSV: ", handler.pathToCSV);
        GUI.enabled = handler.testFinished;
        if(GUILayout.Button("Export to CSV")) {
            switch (handler.SelectedAction) {
                case PedestrianSpeedMeasure.UseCase.NONE:
                    break;
                case PedestrianSpeedMeasure.UseCase.ACCELERATION_TEST:
                    handler.ExportSpeedLogCSV(handler.pathToCSV);
                    break;
                case PedestrianSpeedMeasure.UseCase.BACK_AND_FORTH:
                    handler.ExportTrajectoriesCSV(handler.pathToCSV);
                    handler.ExportLeftRightCount(handler.pathToCSV);
                    break;
                case PedestrianSpeedMeasure.UseCase.COUNTERFLOW:
                    handler.ExportTrajectoriesCSV(handler.pathToCSV);
                    handler.ExportLeftRightCount(handler.pathToCSV);
                    break;
            }
        }
        GUI.enabled = true;
    }

    private void OnEnable() {
        handler = (PedestrianSpeedMeasure)target;
    }
}
