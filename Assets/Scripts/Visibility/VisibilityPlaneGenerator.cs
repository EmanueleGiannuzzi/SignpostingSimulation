using UnityEngine;

using System.Linq;

public class VisibilityPlaneGenerator : MonoBehaviour {
    public string[] areaToAnalize = { "IfcSlab" };

    public GameObject ifcGameObject;

    private GameObject visibilityPlanesGroup;

    private readonly string VISIBILITY_GROUP_NAME = "VisibilityPlanesGroup";

    public GameObject GetVisibilityPlanesGroup() {
        return visibilityPlanesGroup;
        //int size = visibilityPlanesGroup.transform.childCount;
        //GameObject[] children = new GameObject[size];
        //for(int i = 0; i < size; i++) {
        //    children[i] = visibilityPlanesGroup.transform.GetChild(i).gameObject;
        //}
        //return children;
    }

    private bool ShoudAnalizeArea(string ifcClass) {
        return this.areaToAnalize.Contains(ifcClass);
    }

    public void GenerateVisibilityPlanes() {
        DestroyImmediate(GameObject.Find(VISIBILITY_GROUP_NAME));
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

                float floorHeight;
                Mesh topMesh = Utility.GetTopMeshFromGameObject(goElement, out floorHeight);
                Debug.Log("FH: " + floorHeight);

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
