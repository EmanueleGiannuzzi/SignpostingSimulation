using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SignBoard))]
public class SignageBoardEditor : Editor
{
    SignBoard _signBoard;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        //if(DrawDefaultInspector()) {
        //    if(signageBoard.autoUpdate) {
        //        FindObjectOfType<VisibilityHandler>().GenerateVisibilityData();
        //    }
        //}
    }

    void OnEnable() {
        _signBoard = (SignBoard)target;
    }
}
