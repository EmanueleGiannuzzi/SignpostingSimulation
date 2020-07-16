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

    public void GenerateVisibilityPlanes() {
        GameObject.DestroyImmediate(GameObject.Find(VISIBILITY_GROUP_NAME));
        visibilityPlanesGroup = new GameObject(VISIBILITY_GROUP_NAME);

        GeneratePlaneForGameObject(ifcGameObject);
    }

    private void GeneratePlaneForGameObject(GameObject goElement) {
        IFCData ifcData = goElement.GetComponent<IFCData>();

        if(ifcData != null) {
            string ifClass = ifcData.IFCClass;
            if(ShoudAnalizeArea(ifClass)) {
                GameObject plane = new GameObject(ifcData.STEPName);
                MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();

                //plane.transform.position = goElement.transform.position;
                //plane.transform.rotation = goElement.transform.rotation;
                //plane.transform.localScale = goElement.transform.localScale;

                meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));

                Mesh topMesh = Utility.GetTopMeshFromGameObject(goElement, out float floorHeight);
                //Debug.Log("FH: " + floorHeight);

                Vector3 position = goElement.transform.position;
                position[1] = floorHeight; // the Y value
                plane.transform.position = position;

                meshFilter.mesh = topMesh;

                plane.transform.parent = visibilityPlanesGroup.transform;

                VisibilityPlaneData planeData = plane.AddComponent<VisibilityPlaneData>();
                planeData.OriginalFloorHeight = floorHeight;
            }
        }
        foreach(Transform childTransform in goElement.transform) {
            GameObject child = childTransform.gameObject;
            GeneratePlaneForGameObject(child);
        }
    }
}
