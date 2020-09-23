using UnityEngine;

[System.Serializable]
public class SignboardGridGenerator {
    public SignageBoard SignboardTemplate;
    public float resolution; // point/meter

    public float signboardHeight;

    private readonly Environment environment;

    private readonly string SIGNBOARDS_GRID_NAME = "SignboardsGrid";
    private GameObject signboardGridGroup;//Child of this object are the SigboardsGenerated

    public SignboardGridGenerator(Environment environment) {
        this.environment = environment;
    }

    public GameObject GetSignboardGridGroup() {
        return signboardGridGroup;
    }

    private GameObject GetVisibilityPlane(int visPlaneId) {
        return environment.GetVisibilityPlaneGenerator().GetVisibilityPlanesGroup().transform.GetChild(visPlaneId).gameObject;
    }

    public int GetVisibilityPlaneSize() {
        return environment.GetVisibilityPlaneGenerator().GetVisibilityPlanesGroup().transform.childCount;
    }

    public void DeleteObjects() {
        GameObject.DestroyImmediate(GameObject.Find(SIGNBOARDS_GRID_NAME));
    }

    public void GenerateGrid() {
        DeleteObjects();
        signboardGridGroup = new GameObject(SIGNBOARDS_GRID_NAME);

        GenerateGrid(signboardGridGroup);
    }

    public void GenerateGrid(GameObject parent) {

        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);

            GameObject visPlaneParent = new GameObject(visibilityPlane.name);
            visPlaneParent.transform.parent = parent.transform;

            float visibilityPlaneHeight = visibilityPlane.GetComponent<VisibilityPlaneData>().OriginalFloorHeight;

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;
            int widthResolution = (int)Mathf.Floor((float)planeWidth * this.resolution);
            int heightResolution = (int)Mathf.Floor((float)planeHeight * this.resolution);

            Vector3 cornerMax = meshRendererBounds.max;


            for(int z = 0; z < heightResolution; z++) {
                for(int x = 0; x < widthResolution; x++) {
                    Vector3 position = new Vector3(cornerMax.x - ((planeWidth / widthResolution) * x), visibilityPlaneHeight + signboardHeight, cornerMax.z - ((planeHeight / heightResolution) * z));
                    if(Utility.HorizontalPlaneContainsPoint(visibilityPlane.GetComponent<MeshFilter>().sharedMesh, visibilityPlane.transform.InverseTransformPoint(position), (planeWidth / widthResolution), (planeHeight / heightResolution))) {
                        GameObject signageboardObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        signageboardObj.name = "Signboard [" + x + ", " + z + "]";
                        signageboardObj.transform.position = position;
                        signageboardObj.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                        signageboardObj.transform.localScale = new Vector3(-0.08f, 1f, 0.08f);
                        signageboardObj.transform.parent = visPlaneParent.transform;

                        signageboardObj.AddComponent<SignageBoard>();
                        SignageBoard signageboard = signageboardObj.GetComponent<SignageBoard>();
                        signageboard.CopyDataFrom(SignboardTemplate);
                        signageboard.Color = new Color(
                         Random.Range(0f, 1f),
                         Random.Range(0f, 1f),
                         Random.Range(0f, 1f)
                       );

                        signageboardObj.AddComponent<GridSignageboard>();
                        GridSignageboard gridSignageboard = signageboardObj.GetComponent<GridSignageboard>();
                        gridSignageboard.planeLocalIndex = new Vector2Int(x, z);
                    }
                }
            }
        }
    }
}
