using UnityEngine;

using System.Linq;

[System.Serializable]
public class VisibilityPlaneGenerator {
    public string[] areaToAnalize = { "IfcSlab" };

    public GameObject ifcGameObject;

    private GameObject visibilityPlanesGroup;//Child of this object are the Visibility Planes Generated

    private readonly string VISIBILITY_GROUP_NAME = "VisibilityPlanesGroup";

    public GameObject GetVisibilityPlanesGroup() {
        return visibilityPlanesGroup;
    }

    private bool ShoudAnalizeArea(string ifcClass) {
        return this.areaToAnalize.Contains(ifcClass);
    }

    public void GenerateVisibilityPlanes(int analysisResolution) {
        GameObject.DestroyImmediate(GameObject.Find(VISIBILITY_GROUP_NAME));
        visibilityPlanesGroup = new GameObject(VISIBILITY_GROUP_NAME);

        GeneratePlaneForGameObject(analysisResolution, ifcGameObject);
    }

    private void GeneratePlaneForGameObject(int analysisResolution, GameObject goElement) {
        IFCData ifcData = goElement.GetComponent<IFCData>();

        if(ifcData != null) {
            string ifClass = ifcData.IFCClass;
            if(ShoudAnalizeArea(ifClass)) {
                GameObject plane = new GameObject(ifcData.STEPName);
                MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();

                meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));

                Mesh topMesh = Utility.GetTopMeshFromGameObject(goElement, out float floorHeight);

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
