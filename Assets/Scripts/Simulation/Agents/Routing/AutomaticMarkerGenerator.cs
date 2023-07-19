using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class AutomaticMarkerGenerator : MonoBehaviour {
    public GameObject ifcGameObject;

    private GameObject markerParent;
    private readonly string MARKERS_GROUP_NAME = "MarkersGroup";

    private string[] ifcTraversableTags = { "IfcDoor" };

    private RoutingGraph routingGraph = new ();

    private bool IsTraversableTag(string ifcTag) {
        return ifcTraversableTags.Contains(ifcTag);
    }

    private IEnumerable<IFCData> IfcTraversables() {
        return ifcGameObject.GetComponentsInChildren<IFCData>().Where(ifcData => IsTraversableTag(ifcData.IFCClass));
    }

    public void AddMarkersToTraversables() {
        ResetMarkers();
        
        if (!ifcGameObject) {
            Debug.LogError("[AutomaticMarkerGenerator]: No IFC Object found");
            return;
        }
        
        foreach (IFCData traversable in IfcTraversables()) {
            Renderer traversableRenderer = traversable.GetComponent<Renderer>();
            if (!traversableRenderer) {
                return;
            }
            
            Bounds traversableRendererBounds = traversableRenderer.bounds;
            Vector3 traversableCenter = traversableRendererBounds.center;

            Vector3 projectionOnNavmesh;
            if(TraversableCenterProjectionOnNavMesh(traversableCenter, out projectionOnNavmesh)
               && traversableCenter.y > projectionOnNavmesh.y) {
                float widthX = Mathf.Max(0.5f, traversableRendererBounds.extents.x*2);
                float widthZ = Mathf.Max(0.5f, traversableRendererBounds.extents.z*2);
                RouteMarker marker = SpawnMarker(projectionOnNavmesh, widthX, widthZ);

                string storeyName = GetStoreyName(traversable.gameObject);
                if (storeyName != null) {
                    routingGraph.AddVertex(marker, storeyName);
                }
            } 
        }
    }

    [CanBeNull]
    private string GetStoreyName(GameObject traversableGO) {
        Transform parent = traversableGO.transform.parent;
        if (!parent) {
            return null;
        }
        
        IFCData parentData = parent.GetComponent<IFCData>();
        if (!parentData || parentData.IFCClass != "IfcBuildingStorey") {
            return GetStoreyName(parent.gameObject);
        }

        return parentData.STEPName;
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
        routingGraph = new RoutingGraph();
        DestroyImmediate(GameObject.Find(MARKERS_GROUP_NAME));
        markerParent = new GameObject(MARKERS_GROUP_NAME);
    }

    private RouteMarker SpawnMarker(Vector3 pos, float widthX, float widthZ) {
        GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        markerGO.transform.parent = markerParent.transform;
        pos += new Vector3(0f, 0.01f, 0f);
        markerGO.transform.position = pos;
        markerGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        markerGO.transform.localScale = new Vector3(widthX, widthZ, 1.0f);
        markerGO.GetComponent<Renderer>().sharedMaterial.color = Color.white;
        markerGO.layer = 10;
        RouteMarker marker = markerGO.AddComponent<RouteMarker>();
        MeshCollider markerCollider = markerGO.GetComponent<MeshCollider>();
        markerCollider.convex = true;
        markerCollider.isTrigger = true;

        return marker;
    }

    private void DrawLineBetweenMarkers(RouteMarker marker1, RouteMarker marker2) {
        Debug.DrawLine(marker1.transform.position, marker2.transform.position, Color.blue);
    }

    private void OnDrawGizmos() {
        foreach (var edge in routingGraph.Edges) {
            DrawLineBetweenMarkers(edge.Vertex1, edge.Vertex2);
        }
    }
}
