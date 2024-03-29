﻿using UnityEngine;
using UnityEngine.AI;
using System.Linq;

[System.Serializable]
public class VisibilityPlaneGenerator {
    public string[] areaToAnalyze = { "IfcSlab" };

    public GameObject ifcGameObject;

    private GameObject visibilityPlanesGroup;//Child of this object are the Visibility Planes Generated

    private readonly string VISIBILITY_GROUP_NAME = "VisibilityPlanesGroup";

    public GameObject GetVisibilityPlanesGroup() {
        return visibilityPlanesGroup;
    }

    private bool ShouldAnalyzeArea(string ifcClass) {
        return this.areaToAnalyze.Contains(ifcClass);
    }

    public void GenerateVisibilityPlanes(int analysisResolution) {
        Object.DestroyImmediate(GameObject.Find(VISIBILITY_GROUP_NAME));
        visibilityPlanesGroup = new GameObject(VISIBILITY_GROUP_NAME);

        GeneratePlaneForGameObject(analysisResolution, ifcGameObject);
    }

    private void GeneratePlaneForGameObject(int analysisResolution, GameObject goElement) {
        if(!goElement.activeSelf) {
            return;
        }

        IFCData ifcData = goElement.GetComponent<IFCData>();
        if(ifcData != null) {
            string ifClass = ifcData.IFCClass;
            if(ShouldAnalyzeArea(ifClass)) {
                GameObject plane = new GameObject(ifcData.STEPName);
                plane.layer = 8;//VisibilityLayer - To be ignored from NavMesh
                NavMeshModifier navmeshModifier = plane.AddComponent<NavMeshModifier>();
                navmeshModifier.ignoreFromBuild = true;
                MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();

                meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));

                Mesh topMesh = Utility.GetTopMeshFromGameObject(goElement, out float floorHeight);
                floorHeight += 0.0001f;

                Vector3 position = goElement.transform.position;
                position[1] = floorHeight; // the Y value
                plane.transform.position = position;

                meshFilter.mesh = topMesh;

                plane.transform.parent = visibilityPlanesGroup.transform;

                VisibilityPlaneData planeData = plane.AddComponent<VisibilityPlaneData>();
                planeData.OriginalFloorHeight = floorHeight;

                Bounds meshRendererBounds = plane.GetComponent<MeshRenderer>().bounds;
                float planeWidth = meshRendererBounds.extents.x * 2;
                float planeHeight = meshRendererBounds.extents.z * 2;

                int widthResolution = (int)Mathf.Floor(planeWidth * analysisResolution);
                int heightResolution = (int)Mathf.Floor(planeHeight * analysisResolution);

                planeData.SetResolution(widthResolution, heightResolution);
                planeData.GenerateAnalyzablePoints();
            }
        }
        foreach(Transform childTransform in goElement.transform) {
            GameObject child = childTransform.gameObject;
            GeneratePlaneForGameObject(analysisResolution, child);
        }
    }
}
