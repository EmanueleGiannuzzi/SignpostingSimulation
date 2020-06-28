using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IfcOpenShellParser))]
public class IfcOpenShellParserEditor : Editor
{
    IfcOpenShellParser handler;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Load IFC")) {
            handler.LoadIFC();
        }

        //if(GUILayout.Button("Load OBJ, MTL, XML")) {
        //    handler.LoadOBJMTLXMLFile();
        //}
    }

    void OnEnable() {
        handler = (IfcOpenShellParser)target;
    }
}
