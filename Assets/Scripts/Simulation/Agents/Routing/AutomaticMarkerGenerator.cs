using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AutomaticMarkerGenerator : MonoBehaviour {
    public GameObject ifcGameObject;

    private GameObject markerParent;
    private readonly string MARKERS_GROUP_NAME = "MarkersGroup";

    private string[] ifcTraversableTags = { "IfcDoor" };

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
            if (traversableRenderer) {
                Bounds traversableRendererBounds = traversableRenderer.bounds;
                Vector3 traversableCenter = traversableRendererBounds.center;

                Vector3 projectionOnNavmesh;
                if(TraversableCenterProjectionOnNavMesh(traversableCenter, out projectionOnNavmesh)
                   && traversableCenter.y > projectionOnNavmesh.y) {
                    float widthX = Mathf.Max(0.5f, traversableRendererBounds.extents.x*2);
                    float widthZ = Mathf.Max(0.5f, traversableRendererBounds.extents.z*2);
                    SpawnMarker(projectionOnNavmesh, widthX, widthZ);
                } 
            }
        }
    }

    private bool TraversableCenterProjectionOnNavMesh(Vector3 traversableCenter, out Vector3 result) {
        if (NavMesh.SamplePosition(traversableCenter, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) {
            result = hit.position;
            //Debug.DrawLine(traversableCenter, result, Color.blue, 15f, false);
            return true;
        }
        result = Vector3.zero;
        //Debug.DrawLine(traversableCenter, traversableCenter + (3f*Vector3.down), Color.red, 15f, false);
        return false;
    }

    private void ResetMarkers() {
        DestroyImmediate(GameObject.Find(MARKERS_GROUP_NAME));
        markerParent = new GameObject(MARKERS_GROUP_NAME);
    }

    private void SpawnMarker(Vector3 pos, float widthX, float widthZ) {
        //Debug.Log($"New Marker P:{pos} [{widthX}, {widthZ}]");
        GameObject marker  = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.transform.parent = markerParent.transform;
        pos += new Vector3(0f, 0.01f, 0f);
        marker.transform.position = pos;
        marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        marker.transform.localScale = new Vector3(widthX, widthZ, 1.0f);
        marker.GetComponent<Renderer>().sharedMaterial.color = Color.white;
        marker.AddComponent<RouteMarker>();
        MeshCollider markerCollider = marker.GetComponent<MeshCollider>();
        markerCollider.convex = true;
        markerCollider.isTrigger = true;


    }
}
