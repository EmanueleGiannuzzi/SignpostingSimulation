using UnityEngine;

[System.Serializable]
public class SignboardGridGenerator {
    public SignBoard SignboardTemplate;
    public float resolution; // point/meter

    public float signboardHeight;
    public float sigboardOrientation;

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

    private void GenerateSignboardBack(GameObject signboardObj, Material signboardBackMaterial) {
        GameObject signboardBackObj = GameObject.Instantiate(signboardObj, signboardObj.transform, true);
        signboardBackObj.name = signboardObj.name + "(Mirror)";
        Object.Destroy(signboardBackObj.GetComponent<Collider>());
        signboardBackObj.transform.rotation = Quaternion.AngleAxis(180f, Vector3.up) * signboardObj.transform.rotation;

        signboardBackObj.GetComponent<MeshRenderer>().sharedMaterial = signboardBackMaterial;
    }

    private GameObject GenerateMainSignboard(int gridX, int gridZ, Color signboardColor, Vector3 position, float sideWidth, float sideHeight, GameObject visPlaneParent, Material signboardBackMaterial) {
        GameObject signboardObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Object.Destroy(signboardObj.GetComponent<Collider>());
        signboardObj.name = "Signboard [" + gridX + ", " + gridZ + "]";
        position.x -= sideWidth / 2;
        position.z -= sideHeight / 2;
        signboardObj.transform.position = position;
        signboardObj.transform.rotation = Quaternion.Euler(-90f, 0f, sigboardOrientation);
        signboardObj.transform.localScale = new Vector3(-0.08f, 1f, 0.08f);
        signboardObj.transform.parent = visPlaneParent.transform;

        signboardObj.AddComponent<SignBoard>();
        SignBoard signboard = signboardObj.GetComponent<SignBoard>();
        signboard.CopyDataFrom(SignboardTemplate);
        signboard.Color = signboardColor;

        MeshRenderer signboardRenderer = signboardObj.GetComponent<MeshRenderer>();
        Material tempMaterial = new Material(signboardRenderer.sharedMaterial) {
            color = signboardColor
        };
        signboardRenderer.sharedMaterial = tempMaterial;

        GenerateSignboardBack(signboardObj, signboardBackMaterial);

        signboardObj.AddComponent<GridSignageboard>();
        GridSignageboard gridSignboard = signboardObj.GetComponent<GridSignageboard>();
        gridSignboard.planeLocalIndex = new Vector2Int(gridX, gridZ);

        return signboardObj;
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
            int widthResolution = (int)Mathf.Floor(planeWidth * this.resolution);
            int heightResolution = (int)Mathf.Floor(planeHeight * this.resolution);

            Vector3 cornerMax = meshRendererBounds.max;

            Material signboardBackMaterial = new Material(Shader.Find("Standard"));
            signboardBackMaterial.color = Color.gray;

            for(int z = 0; z < heightResolution; z++) {
                for(int x = 0; x < widthResolution; x++) {
                    float sideWidth = planeWidth / widthResolution;
                    float sideHeight = planeHeight / heightResolution;
                    Vector3 position = new Vector3(cornerMax.x - (sideWidth * x), visibilityPlaneHeight + signboardHeight, cornerMax.z - (sideHeight * z));
                    if(Utility.HorizontalPlaneContainsPoint(visibilityPlane.GetComponent<MeshFilter>().sharedMesh, visibilityPlane.transform.InverseTransformPoint(position), (planeWidth / widthResolution), (planeHeight / heightResolution))) {
                        Color signboardColor = new Color(
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f)
                        );
                        GameObject signboardObj = GenerateMainSignboard(x, z, signboardColor, position, sideWidth, sideHeight, visPlaneParent, signboardBackMaterial);
                    }
                }
            }
        }
    }
}
