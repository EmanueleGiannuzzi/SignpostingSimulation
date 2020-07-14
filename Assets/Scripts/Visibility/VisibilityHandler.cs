﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisibilityHandler : MonoBehaviour {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;

    [Header("Texture Color")]
    public Color nonVisibleColor;

    [Header("Resolution (point/meter)")]
    public int resolution = 10;

    //public GameObject visibilityPlaneGroup

    private SignageBoard[] signageBoards;

    public Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfos;//1 for each visibility plane mesh

    [HideInInspector]
    public float progressAnalysis = -1f;

    private GameObject GetVisibilityPlane(int visPlaneId) {//TODO
        //return visibilityPlaneGroup.transform.GetChild(visPlaneId).gameObject;
        return FindObjectOfType<VisibilityPlaneGenerator>().GetVisibilityPlanesGroup().transform.GetChild(visPlaneId).gameObject;
    }

    public SignageBoard GetSignageBoard(int signageBoardID) {
        return signageBoards[signageBoardID];
    }

    public int GetVisibilityPlaneSize() {
        //return visibilityPlaneGroup.transform.childCount;
        return FindObjectOfType<VisibilityPlaneGenerator>().GetVisibilityPlanesGroup().transform.childCount;
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

    //public List<int> GetSignboardIDsInCoord(Vector2Int coord, int agentTypeID) {
    //    List<int> result = new List<int>();

    //    foreach(Dictionary<Vector2Int, VisibilityInfo>[] visInfos in this.visibilityInfos) {
    //        VisibilityInfo visInfo = visInfos[agentTypeID][coord];
    //        if(visInfo != null) {
    //            result.AddRange(visInfo.GetVisibleBoards());
    //            break;
    //        }
    //    }

    //    return result;
    //}

    public void ShowVisibilityPlane(int agentTypeID) {//TODO: Use enumerator
        for(int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];

            Vector3 position = visibilityPlane.transform.position;
            VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            float originalFloorHeight = planeData.OriginalFloorHeight;
            position[1] = originalFloorHeight + agentTypes[agentTypeID].Value; // the Y value
            visibilityPlane.transform.position = position;

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;
            int widthResolution = planeData.GetAxesResolution().x;
            int heightResolution = planeData.GetAxesResolution().y;
            //int widthResolution = (int)Mathf.Floor(planeWidth * this.resolution);
            //int heightResolution = (int)Mathf.Floor(planeHeight * this.resolution);


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
            //meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
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

            int widthResolution = (int)Mathf.Floor(planeWidth * this.resolution);
            int heightResolution = (int)Mathf.Floor(planeHeight * this.resolution);
            VisibilityPlaneData visibilityPlaneData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            visibilityPlaneData.ValidMeshPointsCount = 0;
            visibilityPlaneData.SetResolution(widthResolution, heightResolution);
            float originalFloorHeight = visibilityPlaneData.OriginalFloorHeight;

            float agentTypeProgress = planesProgress / agentTypes.Length;
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                StringFloatTuple tuple = agentTypes[agentTypeID];

                Vector3 position = visibilityPlane.transform.position;
                position[1] = originalFloorHeight + tuple.Value; // the Y value
                visibilityPlane.transform.position = position;

                float signageboardProgress = agentTypeProgress / signageBoards.Length;
                for(int signageboardID = 0; signageboardID < signageBoards.Length; signageboardID++) {
                    SignageBoard signageboard = signageBoards[signageboardID];

                    Vector3 p = signageboard.GetWorldCenterPoint();
                    Vector3 n = signageboard.GetDirection();
                    float theta = (signageboard.GetViewingAngle() * Mathf.PI) / 180;
                    float d = signageboard.GetViewingDistance();

                    float resolutionProgress = signageboardProgress / (heightResolution * widthResolution);
                    for(int z = 0; z < heightResolution; z++) {
                        for(int x = 0; x < widthResolution; x++) {
                            Vector3 vi = new Vector3(cornerMin.x - ((planeWidth / widthResolution) * x), visibilityPlane.transform.position.y, cornerMin.z - ((planeHeight / heightResolution) * z));
                            Vector3 pToViDirection = vi - p;
                            //Debug.DrawLine(vi, p, Color.green);

                            bool isVisible = false;

                            if(Utility.HorizontalPlaneContainsPoint(visibilityPlane.GetComponent<MeshFilter>().sharedMesh, visibilityPlane.transform.InverseTransformPoint(vi))) {
                            //if(Utility.PolyContainsPoint(outerVertices, visibilityPlane.transform.InverseTransformPoint(vi))) {
                                if(agentTypeID == 0 && signageboardID == 0) {
                                    visibilityPlaneData.ValidMeshPointsCount++;
                                }
                                if((Vector3.Dot((vi - p), n) / ((vi - p).magnitude * n.magnitude)) >= Mathf.Cos(theta / 2)
                                && ((vi - p).magnitude <= d)) {
                                    Ray ray = new Ray(p, pToViDirection);
                                    float maxDistance = Vector3.Distance(p, vi);
                                    RaycastHit hit;
                                    if(!Physics.Raycast(ray, out hit, maxDistance)) {//(ray, out hit, maxDistance)
                                        isVisible = true;
                                    }
                                    //else {
                                    //    Debug.DrawRay(p, pToViDirection, Color.red);
                                    //}
                                }
                            }

                            if(isVisible) {
                                Vector2Int coords = new Vector2Int(x, z);
                                if(visInfos[agentTypeID].ContainsKey(coords)) {
                                    visInfos[agentTypeID][coords].AddVisibleBoard(signageboardID);
                                }
                                else {
                                    VisibilityInfo vinfo = new VisibilityInfo(vi);
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
        CalculateSignCoverage();
        this.progressAnalysis = 0f;
        EditorUtility.ClearProgressBar();

        this.progressAnalysis = -1f;
    }


    public void CalculateSignCoverage() {
        int[,] signageboardCoverage = new int[signageBoards.Length, agentTypes.Length];
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

        for(int signageboardID = 0; signageboardID < signageBoards.Length; signageboardID++) {
            SignageBoard signageboard = signageBoards[signageboardID];
            signageboard.coveragePerAgentType = new float[agentTypes.Length];
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                float coverage = (float)signageboardCoverage[signageboardID, agentTypeID] / visibilityGroupMaxSize;
                signageboard.coveragePerAgentType[agentTypeID] = coverage;
            }
        }
    }
}
