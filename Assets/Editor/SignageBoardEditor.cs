using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SignageBoard))]
public class SignageBoardEditor : Editor
{
    SignageBoard signageBoard;
    public override void OnInspectorGUI() {

        if(DrawDefaultInspector()) {//If any value was changed
            //if(signageBoard.autoUpdate) {
            //    FindObjectOfType<VisibilityHandler>().GenerateVisibilityData();
            //}
        }
    }

    void OnEnable() {
        signageBoard = (SignageBoard)target;
    }
}
