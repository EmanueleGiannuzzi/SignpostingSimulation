using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisibilityHandler : MonoBehaviour {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;

    [Header("Texture Color")]
    public Color nonVisibleColor;

    [Header("Resolution (point/meter)")]
    public int resolution = 10;

    public GameObject visibilityPlaneGroup;

    //[Header("Visibility Plane")]
    //public GameObject visibilityPlane;

    private SignageBoard[] signageBoards;

    private Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfos;//1 for each visibility plane mesh

    [HideInInspector]
    public float progressAnalysis = -1f;

    private GameObject GetVisibilityPlane(int visPlaneId) {//TODO
        return visibilityPlaneGroup.transform.GetChild(visPlaneId).gameObject;
        //return FindObjectOfType<VisibilityPlaneGenerator>().GetVisibilityPlanesGroup().transform.GetChild(visPlaneId).gameObject;
    }

    private int GetVisibilityPlaneSize() {
        return visibilityPlaneGroup.transform.childCount;
        //return FindObjectOfType<VisibilityPlaneGenerator>().GetVisibilityPlanesGroup().transform.childCount;
    }

    public void Init() {
        signageBoards = FindObjectsOfType<SignageBoard>();

        Debug.Log(signageBoards.Length + " Signage Boards found.");
    }

    public void GenerateVisibilityData() {
        if(signageBoards.Length <= 0) {
            Debug.LogError("No Signage Boards found, please press Init first.");
            return;
        }

        visibilityInfos = new Dictionary<Vector2Int, VisibilityInfo>[GetVisibilityPlaneSize()][];
        for(int i = 0; i < GetVisibilityPlaneSize(); i++) {
            visibilityInfos[i] = new Dictionary<Vector2Int, VisibilityInfo>[agentTypes.Length];
            for(int j = 0; j < agentTypes.Length; j++) {
                visibilityInfos[i][j] = new Dictionary<Vector2Int, VisibilityInfo>();
            }
        }

        AnalyzeSignboards();

        Debug.Log("Done calculating");
    }

    public Dictionary<Vector2Int, VisibilityInfo>[] GetVisibilityInfo(int id) {
        return visibilityInfos[id];
    }

    public void ShowVisibilityPlane(int agentTypeID) {//TODO: Use enumerator
        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];

            Vector3 position = visibilityPlane.transform.position;
            position[1] = agentTypes[agentTypeID].Value; // the Y value
            visibilityPlane.transform.position = position;

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;
            int widthResolution = (int)Mathf.Floor(planeWidth * this.resolution);
            int heightResolution = (int)Mathf.Floor(planeHeight * this.resolution);


            Vector3[] meshVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector2[] uvs = new Vector2[meshVertices.Length];

            Vector3 localMin = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.min);
            Vector3 localMax = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.max) - localMin;

        for(int i = 0; i < meshVertices.Length; i++) {
                Vector3 normVertex = meshVertices[i] - localMin;
                uvs[i] = new Vector2(1f - (float)(normVertex.x / localMax.x), 1f - (float)(normVertex.z / localMax.z));
            }
            visibilityPlane.GetComponent<MeshFilter>().sharedMesh.uv = uvs;

            Texture2D texture = VisibilityTextureGenerator.TextureFromVisibilityData(visInfos[agentTypeID], signageBoards, widthResolution, heightResolution, nonVisibleColor);
            MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }

    private void AnalyzeSignboards() {
        this.progressAnalysis = 1f;

        float planesProgress = progressAnalysis / GetVisibilityPlaneSize();
        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            Vector3 cornerMin = meshRendererBounds.max;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;

            float agentTypeProgress = planesProgress / agentTypes.Length;
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                StringFloatTuple tuple = agentTypes[agentTypeID];

                Vector3 position = visibilityPlane.transform.position;
                position[1] += tuple.Value; // the Y value
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

                    int widthResolution = (int)Mathf.Floor(planeWidth * this.resolution);
                    int heightResolution = (int)Mathf.Floor(planeHeight * this.resolution);

                    float resolutionProgress = signageboardProgress / (heightResolution * widthResolution);
                    for(int z = 0; z < heightResolution; z++) {
                        for(int x = 0; x < widthResolution; x++) {
                            Vector3 vi = new Vector3(cornerMin.x - ((planeWidth / widthResolution) * x), visibilityPlane.transform.position.y, cornerMin.z - ((planeHeight / heightResolution) * z));
                            Debug.DrawLine(vi, p, Color.green);

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
                                if(visInfos[agentTypeID].ContainsKey(coords)) {
                                    visInfos[agentTypeID][coords].AddVisibleBoard(signageboardID);
                                }
                                else {
                                    VisibilityInfo vinfo = new VisibilityInfo();
                                    vinfo.AddVisibleBoard(signageboardID);
                                    visInfos[agentTypeID].Add(coords, vinfo);
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
        }
        this.progressAnalysis = 0f;
        EditorUtility.ClearProgressBar();

        this.progressAnalysis = -1f;
    }

    

    //public GameObject testObject;
    //public static Material visibilityPlaneMaterial;
    public void Test() {
        //GameObject visibilityPlane = new GameObject("VisibilityPlaneTEST");
        //MeshFilter meshFilter = visibilityPlane.AddComponent<MeshFilter>();
        //MeshRenderer meshRenderer = visibilityPlane.AddComponent<MeshRenderer>();

        //visibilityPlane.transform.position = testObject.transform.position;
        //visibilityPlane.transform.rotation = testObject.transform.rotation;
        //visibilityPlane.transform.localScale = testObject.transform.localScale;

        //meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));

        //Mesh topMesh = Utility.GetTopMeshFromGameObject(testObject);
        //meshFilter.mesh = topMesh;
    }
}
