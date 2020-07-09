using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SignageBoard))]
public class SignageBoardEditor : Editor
{
    SignageBoard signageBoard;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        //if(DrawDefaultInspector()) {
        //    if(signageBoard.autoUpdate) {
        //        FindObjectOfType<VisibilityHandler>().GenerateVisibilityData();
        //    }
        //}
    }

    void OnEnable() {
        signageBoard = (SignageBoard)target;
    }
}
