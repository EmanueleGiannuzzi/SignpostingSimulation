using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AutomaticMarkerGenerator : MonoBehaviour {
    public GameObject ifcGameObject;
    public Material markerMaterial;

    private GameObject markerParent;
    private const string MARKERS_GROUP_NAME = "MarkersGroup";

    public string[] IfcTraversableTags = { "IfcDoor" };
    
    public delegate void OnMarkersGenerated(List<IRouteMarker> markers);
    public event OnMarkersGenerated OnMarkersGeneration;

    private void Start() {
        AddMarkersToTraversables();
    }

    private bool IsTraversableTag(string ifcTag) {
        return IfcTraversableTags.Contains(ifcTag);
    }

    private IEnumerable<IFCData> IfcTraversables() {
        return ifcGameObject.GetComponentsInChildren<IFCData>().Where(ifcData => IsTraversableTag(ifcData.IFCClass));
    }

    public void AddMarkersToTraversables() {
        ResetMarkers();

        List<IRouteMarker> markers = new();

        // foreach (InputArea existingMarker in FindObjectsOfType<InputArea>()) {
        //     markers.Add(existingMarker);
        // }
        
        if (!ifcGameObject) {
            Debug.LogError("[AutomaticMarkerGenerator]: No IFC Object found");
            return;
        }

        float progress = 0f;
        IEnumerable<IFCData> traversables = IfcTraversables();
        float progressBarStep = 1f / traversables.Count();

        int spawnedMarkers = 0;
        foreach (IFCData traversable in IfcTraversables()) {
            Renderer traversableRenderer = traversable.GetComponent<Renderer>();
            if (!traversableRenderer) {
                return;
            }
            
            Bounds traversableRendererBounds = traversableRenderer.bounds;
            Vector3 traversableCenter = traversableRendererBounds.center;

            if(TraversableCenterProjectionOnNavMesh(traversableCenter, out Vector3 projectionOnNavmesh)
               && traversableCenter.y > projectionOnNavmesh.y) {
                float widthX = Mathf.Max(1f, traversableRendererBounds.extents.x*2);
                float widthZ = Mathf.Max(1f, traversableRendererBounds.extents.z*2);
                IntermediateMarker marker = SpawnMarker(projectionOnNavmesh, widthX, widthZ, $"IntermediateMarker-{spawnedMarkers}");
                spawnedMarkers++;
                
                markers.Add(marker);
            } 
            
            progress -= progressBarStep;
            if(EditorUtility.DisplayCancelableProgressBar("Automatic Marker Generator", "Generating Routing Markers", 1f - progress)) {
                EditorUtility.ClearProgressBar();
                return;
            }
        }
        OnMarkersGeneration?.Invoke(markers);
        EditorUtility.ClearProgressBar();
    }

    [CanBeNull]
    private string GetStoreyName(GameObject traversableGO) {
        while (true) {
            Transform parent = traversableGO.transform.parent;
            if (!parent) {
                return null;
            }

            IFCData parentData = parent.GetComponent<IFCData>();
            if (!parentData || parentData.IFCClass != "IfcBuildingStorey") {
                traversableGO = parent.gameObject;
                continue;
            }

            return parentData.STEPName;
        }
    }

    private bool TraversableCenterProjectionOnNavMesh(Vector3 traversableCenter, out Vector3 result) {
        if (NavMesh.SamplePosition(traversableCenter, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    private void ResetMarkers() {
        foreach (var markerGroup in GameObject.FindGameObjectsWithTag(MARKERS_GROUP_NAME)) {
            DestroyImmediate(markerGroup);
        }
        markerParent = new GameObject(MARKERS_GROUP_NAME) {
            tag = MARKERS_GROUP_NAME
        };
    }

    private IntermediateMarker SpawnMarker(Vector3 pos, float widthX, float widthZ, string name) {
        GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        markerGO.transform.parent = markerParent.transform;
        pos += new Vector3(0f, 0.01f, 0f);
        markerGO.transform.position = pos;
        markerGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        markerGO.transform.localScale = new Vector3(widthX, widthZ, 1.0f);
        markerGO.GetComponent<Renderer>().sharedMaterial = markerMaterial;
        markerGO.layer = 10;
        markerGO.name = name;
        IntermediateMarker marker = markerGO.AddComponent<IntermediateMarker>();
        MeshCollider markerCollider = markerGO.GetComponent<MeshCollider>();
        markerCollider.convex = true;
        markerCollider.isTrigger = true;

        return marker;
    }
}