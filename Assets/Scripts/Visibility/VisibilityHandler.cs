using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class VisibilityHandler : MonoBehaviour {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;

    [Header("Texture Color")]
    public Color nonVisibleColor;

    [Header("Resolution")]
    public int widthResolution = 512;
    public int heightResolution = 512;

    [Header("Visibility Plane")]
    public GameObject visibilityPlane;

    private SignageBoard[] signageBoards;

    private Dictionary<Vector2Int, VisibilityInfo>[] visibilityInfos;

    [HideInInspector]
    public float progressAnalysis = -1f;


    public void Init() {

        signageBoards = FindObjectsOfType<SignageBoard>();

        Debug.Log(signageBoards.Length + " Signage Boards found.");
    }

    public void GenerateVisibilityData() {
        if(signageBoards.Length <= 0) {
            Debug.LogError("No Signage Boards found, please press Init first.");
            return;
        }

        visibilityInfos = new Dictionary<Vector2Int, VisibilityInfo>[agentTypes.Length];
        for(int i = 0; i < agentTypes.Length; i++) {
            visibilityInfos[i] = new Dictionary<Vector2Int, VisibilityInfo>();
        }

        AnalyzeSignboards();

        Debug.Log("Done calculating");
    }

    public Dictionary<Vector2Int, VisibilityInfo>[] GetVisibilityInfo() {
        return visibilityInfos;
    }

    public void ShowVisibilityPlane(int agentTypeID) {//TODO: Use enumerator
        Vector3 position = visibilityPlane.transform.position;
        position[1] = agentTypes[agentTypeID].Value; // the Y value
        visibilityPlane.transform.position = position;

        Texture2D texture = VisibilityTextureGenerator.TextureFromVisibilityData(visibilityInfos[agentTypeID], signageBoards, widthResolution, heightResolution, nonVisibleColor);
        MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    private void AnalyzeSignboards() {
        this.progressAnalysis = 1f;

        //EditorUtility.DisplayCancelableProgressBar("Simple Progress Bar", "Shows a progress bar for the given seconds", 1f - progressAnalysis);

        Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
        Vector3 cornerMin = meshRendererBounds.max;
        float planeWidth = meshRendererBounds.extents.x * 2;
        float planeHeight = meshRendererBounds.extents.z * 2;

        float agentTypeProgress = this.progressAnalysis / agentTypes.Length;
        for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
            StringFloatTuple tuple = agentTypes[agentTypeID];

            Vector3 position = visibilityPlane.transform.position;
            position[1] = tuple.Value; // the Y value
            visibilityPlane.transform.position = position;

            //float eyeHeight = tuple.Value;

            Vector3[] localVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector3[] worldVertices = new Vector3[localVertices.Length];
            for(int i = 0; i < localVertices.Length; ++i) {
                worldVertices[i] = visibilityPlane.transform.TransformPoint(localVertices[i]);
            }

            float signageboardProgress = agentTypeProgress / signageBoards.Length;
            for(int signageboardID = 0; signageboardID < signageBoards.Length; signageboardID++) {
                SignageBoard signageboard = signageBoards[signageboardID];

                Vector3 p = signageboard.GetCenterPoint();
                Vector3 n = signageboard.GetDirection();
                float theta = (signageboard.GetViewingAngle() * Mathf.PI) / 180;
                float d = signageboard.GetViewingDistance();

                float resolutionProgress = signageboardProgress / (heightResolution * widthResolution);
                for(int z = 0; z < heightResolution; z++) {
                    for(int x = 0; x < widthResolution; x++) {
                        Vector3 vi = new Vector3(cornerMin.x - ((planeWidth / widthResolution) * x), visibilityPlane.transform.position.y, cornerMin.z - ((planeHeight / heightResolution) * z));

                        bool isVisible = false;

                        if(
                            Utility.HorizontalPlaneContainsPoint(visibilityPlane.GetComponent<MeshFilter>().sharedMesh, visibilityPlane.transform.InverseTransformPoint(vi))
                            && (Vector3.Dot((vi - p), n) / ((vi - p).magnitude * n.magnitude)) >= Mathf.Cos(theta / 2)
                            && ((vi - p).magnitude <= d)
                            ) {
                            Ray ray = new Ray(vi, p);
                            //float maxDistance = Vector3.Distance(p, vi);
                            //RaycastHit hit;
                            if(!Physics.Raycast(ray, out _)) {//(ray, out hit, maxDistance)
                                isVisible = true;
                            }
                            //else {
                            //    Debug.DrawLine(p, vi, Color.red);
                            //}
                        }

                        if(isVisible) {
                            Vector2Int coords = new Vector2Int(x, z);
                            if(visibilityInfos[agentTypeID].ContainsKey(coords)) {
                                visibilityInfos[agentTypeID][coords].AddVisibleBoard(signageboardID);
                            }
                            else {
                                VisibilityInfo vinfo = new VisibilityInfo();
                                vinfo.AddVisibleBoard(signageboardID);
                                visibilityInfos[agentTypeID].Add(coords, vinfo);
                            }
                        }
                        this.progressAnalysis -= resolutionProgress;
                    }
                    if(EditorUtility.DisplayCancelableProgressBar("Simple Progress Bar", "Shows a progress bar for the given seconds", 1f - progressAnalysis)) {
                        EditorUtility.ClearProgressBar();
                        this.progressAnalysis = -1f;
                        return;
                    }
                }
            }
        }
        this.progressAnalysis = 0f;
        EditorUtility.ClearProgressBar();

        this.progressAnalysis = -1f;
    }

    public static Mesh GetTopMeshFromGameObject(GameObject gameObject) {
        MeshFilter goMeshFilter = gameObject.GetComponent<MeshFilter>();
        if(goMeshFilter == null || goMeshFilter.sharedMesh == null) {
            return null;
        }

        Mesh goMesh = goMeshFilter.sharedMesh;
        float higherCoord = -float.MaxValue;
        foreach(Vector3 vertex in goMesh.vertices) {
            if(vertex.z > higherCoord) {
                higherCoord = vertex.z;
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> invalidVerticesIDs = new List<int>();
        List<int> triangles = new List<int>();
        Dictionary<int, int> conversionTable = new Dictionary<int, int>();

        int j = 0;//New array id
        for(int i = 0; i < goMesh.vertices.Length; i++) {
            Vector3 vertex = goMesh.vertices[i];
            if(vertex.z == higherCoord) {
                vertices.Add(vertex);
                conversionTable.Add(i, j);
                j++;
            }
            else {
                invalidVerticesIDs.Add(i);
            }
        }

        int triangleCount = goMesh.triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++) {
            int v1 = goMesh.triangles[i * 3];
            int v2 = goMesh.triangles[i * 3 + 1];
            int v3 = goMesh.triangles[i * 3 + 2];

            if(!(invalidVerticesIDs.Contains(v1) 
                || invalidVerticesIDs.Contains(v2) 
                || invalidVerticesIDs.Contains(v3))) {//If triangle is valid
                triangles.Add(conversionTable[v1]);
                triangles.Add(conversionTable[v2]);
                triangles.Add(conversionTable[v3]);
            }
        }


        Mesh mesh = new Mesh {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };

        mesh.name = goMesh.name;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    public GameObject testObject;
    public Material visibilityPlaneMaterial;
    public void Test() {
        GameObject visibilityPlane = new GameObject("VisibilityPlaneTEST");
        MeshFilter meshFilter = visibilityPlane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = visibilityPlane.AddComponent<MeshRenderer>();

        visibilityPlane.transform.position = testObject.transform.position;
        visibilityPlane.transform.rotation = testObject.transform.rotation;
        visibilityPlane.transform.localScale = testObject.transform.localScale;

        meshRenderer.material = new Material(visibilityPlaneMaterial);

        Mesh topMesh = GetTopMeshFromGameObject(testObject);
        meshFilter.mesh = topMesh;
    }
}
