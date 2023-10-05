using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PedestrianSpeedMeasure))]
public class PedestrianSpeedMeasureEditor : Editor {
    private PedestrianSpeedMeasure handler;

    private string pathToCSV = "C:/Users/emagi/Desktop/pedestrianTests.csv";
    
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
                case PedestrianSpeedMeasure.UseCase.ACELERATION_TEST:
                    handler.ExportSpeedLogCSV(pathToCSV);
                    break;
                case PedestrianSpeedMeasure.UseCase.GO_TO_AND_BACK:
                    break;
                case PedestrianSpeedMeasure.UseCase.DOUBLE_GO_TO_AND_BACK:
                    break;
                default:
                    break;
            }
            // handler.ExportTrajectoriesCSV(pathToCSV);
        }
        GUI.enabled = true;
    }

    private void OnEnable() {
        handler = (PedestrianSpeedMeasure)target;
    }
}
