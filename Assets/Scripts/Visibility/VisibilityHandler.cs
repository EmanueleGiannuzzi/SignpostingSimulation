using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class VisibilityHandler {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;

    [Header("Texture Color")]
    public Color nonVisibleColor = new(1f, 0f, 0f, 0.5f);//red

    [Header("Resolution (point/meter)")]
    public int resolution = 10;

    public Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfos;//1 for each visibility plane mesh

    [HideInInspector]
    public float progressAnalysis = -1f;
    private bool done = false;

    private readonly Environment environment;

    private Texture2D[,] resultTextures;

    public VisibilityHandler(Environment environment) {
        this.environment = environment;
    }

    public bool IsCoverageReady() {
        return done;
    }

    public void ClearAllData() {
        visibilityInfos = null;
        resultTextures = null;
        
        done = false;
    }

    public GameObject GetVisibilityPlane(int visPlaneId) {//TODO: Unificare con SignboardGridGenerator
        return environment.GetVisibilityPlaneGenerator().GetVisibilityPlanesGroup().transform.GetChild(visPlaneId).gameObject;
    }

    public int GetVisibilityPlaneSize() {
        return environment.GetVisibilityPlaneGenerator().GetVisibilityPlanesGroup().transform.childCount;
    }

    private SignBoard[] GetSignboardArray() {
        return environment.signageBoards;
    }

    public void GenerateVisibilityData() {
        if(GetSignboardArray().Length <= 0) {
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

        analyzeSignboards();
        generateTextures();
        
        done = true;
        Debug.Log("Done Calculating Visibility Areas");
    }

    private void generateTextures() {
        resultTextures = new Texture2D[GetVisibilityPlaneSize(), agentTypes.Length];
        
        for (int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];
            VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            
            int widthResolution = planeData.GetAxesResolution().x;
            int heightResolution = planeData.GetAxesResolution().y;

            for (int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                resultTextures[visPlaneId, agentTypeID] = VisibilityTextureGenerator.TextureFromVisibilityData(visInfos[agentTypeID],
                    GetSignboardArray(), widthResolution, heightResolution, nonVisibleColor);
            }
        }
    }

    public Texture2D GetResultTexture(int visPlaneId, int agentTypeID) {
        return resultTextures[visPlaneId, agentTypeID];
    }

    public void ShowVisibilityPlane(int agentTypeID) {
        if(this.visibilityInfos == null) {
            return;
        }

        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);

            Vector3 position = visibilityPlane.transform.position;
            VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            float originalFloorHeight = planeData.OriginalFloorHeight;
            position[1] = originalFloorHeight + agentTypes[agentTypeID].Value; // the Y value
            visibilityPlane.transform.position = position;

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            
            Vector3[] meshVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector2[] uvs = new Vector2[meshVertices.Length];

            Vector3 localMin = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.min);
            Vector3 localMax = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.max) - localMin;

            for(int i = 0; i < meshVertices.Length; i++) {
                Vector3 normVertex = meshVertices[i] - localMin;
                uvs[i] = new Vector2(1f - normVertex.x / localMax.x, 1f - normVertex.z / localMax.z);
            }
            visibilityPlane.GetComponent<MeshFilter>().sharedMesh.uv = uvs;

            MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.mainTexture = GetResultTexture(visPlaneId, agentTypeID);
        }
    }

    private void analyzeSignboards() {
        this.progressAnalysis = 1f;

        float progressBarStep = 1f / GetVisibilityPlaneSize() / agentTypes.Length / GetSignboardArray().Length;
        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];

            VisibilityPlaneData visibilityPlaneData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            float originalFloorHeight = visibilityPlaneData.OriginalFloorHeight;

            //float agentTypeProgress = planesProgress / agentTypes.Length;
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                StringFloatTuple tuple = agentTypes[agentTypeID];

                Vector3 position = visibilityPlane.transform.position;
                position[1] = originalFloorHeight + tuple.Value; // the Y value
                visibilityPlane.transform.position = position;

                for(int signageboardID = 0; signageboardID < GetSignboardArray().Length; signageboardID++) {
                    SignBoard signBoard = GetSignboardArray()[signageboardID];
                    Vector3 p = signBoard.GetWorldCenterPoint();
                    Vector3 n = signBoard.GetDirection();
                    float theta = (signBoard.GetViewingAngle() * Mathf.PI) / 180;
                    float d = signBoard.GetViewingDistance();

                    foreach(Vector2 vi2 in visibilityPlaneData.GetPointsForAnalysis().Keys) {
                        Vector3 vi = new Vector3(vi2.x, visibilityPlane.transform.position.y, vi2.y);

                        bool isVisible = false;

                        Vector3 pToViDirection = vi - p;

                        if((Vector3.Dot((vi - p), n) / ((vi - p).magnitude * n.magnitude)) >= Mathf.Cos(theta / 2) && ((vi - p).magnitude <= d)) {
                            Ray ray = new Ray(p, pToViDirection);
                            float maxDistance = Vector3.Distance(p, vi);
                            if(!Physics.Raycast(ray, out _, maxDistance)) {
                                isVisible = true;
                            }
                        }

                        if(isVisible) {
                            Vector2Int coordsToSave = visibilityPlaneData.GetPointsForAnalysis()[vi2];
                            if(visInfos[agentTypeID].ContainsKey(coordsToSave)) {
                                visInfos[agentTypeID][coordsToSave].AddVisibleBoard(signageboardID);
                            }
                            else {
                                VisibilityInfo vinfo = new VisibilityInfo(vi);
                                vinfo.AddVisibleBoard(signageboardID);
                                visInfos[agentTypeID].Add(coordsToSave, vinfo);
                            }
                        }
                    }

                    this.progressAnalysis -= progressBarStep;
                    if(EditorUtility.DisplayCancelableProgressBar("Visibility Handler", "Generating Signboard Visibility Data", 1f - progressAnalysis)) {
                        EditorUtility.ClearProgressBar();
                        this.progressAnalysis = -1f;
                        return;
                    }
                }

            }
        }

        CalculateSignCoverage();
        //this.progressAnalysis = 0f;
        EditorUtility.ClearProgressBar();

        this.progressAnalysis = -1f;
    }


    public void CalculateSignCoverage() {
        int[,] signageboardCoverage = new int[GetSignboardArray().Length, agentTypes.Length];
        int visibilityGroupMaxSize = 0;
        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfosPerMesh = this.visibilityInfos[visPlaneId];
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                Dictionary<Vector2Int, VisibilityInfo> visInfosPerAgentType = visInfosPerMesh[agentTypeID];
                foreach(KeyValuePair<Vector2Int, VisibilityInfo> entry in visInfosPerAgentType) {
                    foreach(int signageboardID in entry.Value.GetVisibleBoards()) {
                        signageboardCoverage[signageboardID, agentTypeID]++;
                    }
                }
            }
            visibilityGroupMaxSize += GetVisibilityPlane(visPlaneId).GetComponent<VisibilityPlaneData>().ValidMeshPointsCount;
        }

        for(int signageboardID = 0; signageboardID < GetSignboardArray().Length; signageboardID++) {
            SignBoard signageboard = GetSignboardArray()[signageboardID];
            signageboard.coveragePerAgentType = new float[agentTypes.Length];
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                float coverage = (float)signageboardCoverage[signageboardID, agentTypeID] / visibilityGroupMaxSize;
                signageboard.coveragePerAgentType[agentTypeID] = coverage;
            }
        }
    }
}
