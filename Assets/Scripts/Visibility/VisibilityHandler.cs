﻿using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.AssetImporters;

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


    //void Start() {

    //}

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

        //foreach(StringFloatTuple tuple in agentTypes) {
        //    Vector3 position = visibilityPlane.transform.position;
        //    position[1] = tuple.Value; // the Y value
        //    visibilityPlane.transform.position = position;
        //    AnalyzeSignboard();
        //for(int i = 0; i < signageBoards.Length; i++) {
        //AnalyzeSignboard(tuple.Value, signageBoards[i], i, ref visibilityInfos[i]);
        //}
        //}
        //MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
        //meshRenderer.sharedMaterial.mainTexture = VisibilityTextureGenerator.TextureFromSignboard(plane, signageBoard, visibleColor, notVisibleColor);
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
        Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
        Vector3 cornerMin = meshRendererBounds.max;
        float planeWidth = meshRendererBounds.extents.x * 2;
        float planeHeight = meshRendererBounds.extents.z * 2;

        foreach(StringFloatTuple tuple in agentTypes) {
            Vector3 position = visibilityPlane.transform.position;
            position[1] = tuple.Value; // the Y value
            visibilityPlane.transform.position = position;

            //float eyeHeight = tuple.Value;

            Vector3[] localVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector3[] worldVertices = new Vector3[localVertices.Length];
            for(int i = 0; i < localVertices.Length; ++i) {
                worldVertices[i] = visibilityPlane.transform.TransformPoint(localVertices[i]);
            }

            for(int signageboardID = 0; signageboardID < signageBoards.Length; signageboardID++) {

                SignageBoard signageboard = signageBoards[signageboardID];
                Vector3 p = signageboard.GetCenterPoint();
                Vector3 n = signageboard.GetDirection();
                float theta = (signageboard.GetViewingAngle() * Mathf.PI) / 180;
                float d = signageboard.GetViewingDistance();


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
                            if(visibilityInfos[signageboardID].ContainsKey(coords)) {
                                visibilityInfos[signageboardID][coords].AddVisibleBoard(signageboardID);
                            }
                            else {
                                VisibilityInfo vinfo = new VisibilityInfo();
                                vinfo.AddVisibleBoard(signageboardID);
                                visibilityInfos[signageboardID].Add(coords, vinfo);
                            }
                        }
                    }
                }
            }
        }
    }
}
