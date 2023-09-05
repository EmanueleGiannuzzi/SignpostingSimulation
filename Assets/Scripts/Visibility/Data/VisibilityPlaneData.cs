using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;

public class VisibilityPlaneData : MonoBehaviour {
    [ReadOnly]
    [SerializeField]
    private float originalFloorHeight;
    public float OriginalFloorHeight {
        set { originalFloorHeight = value; }
        get { return originalFloorHeight; }
    }

    private readonly Dictionary<Vector2, Vector2Int> AnalyzablePoints = new();

    [ReadOnly]
    [SerializeField]
    private Vector2Int axesResolution;

    public int ValidMeshPointsCount => AnalyzablePoints.Count;

    public void SetResolution(int widthRes, int heightRes) {
        axesResolution = new Vector2Int(widthRes, heightRes);
    }

    public Vector2Int GetAxesResolution() {
        return axesResolution;
    }

    public Dictionary<Vector2, Vector2Int> GetPointsForAnalysis() {
        return AnalyzablePoints;
    }

    public void GenerateAnalyzablePoints() {
        if(axesResolution == null) {
            Debug.LogError("Trying to generate analyzablePoints without setting Width and Height Resolutions.");
            return;
        }
        AnalyzablePoints.Clear();

        GameObject visibilityPlane = this.gameObject;
        Mesh visibilityPlaneMesh = visibilityPlane.GetComponent<MeshFilter>().sharedMesh;
        Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
        Vector3 cornerMax = meshRendererBounds.max;
        float planeWidth = meshRendererBounds.extents.x * 2;
        float planeHeight = meshRendererBounds.extents.z * 2;
        int widthResolution = axesResolution.x;
        int heightResolution = axesResolution.y;

        float progress = 0f;
        float progressStep = 1f / (heightResolution*widthResolution);
        for(int z = 0; z < heightResolution; z++) {
            for(int x = 0; x < widthResolution; x++) {
                Vector3 vi = new Vector3(cornerMax.x - ((planeWidth / widthResolution) * x), 0f, cornerMax.z - ((planeHeight / heightResolution) * z));
                if(Utility.HorizontalPlaneContainsPoint(visibilityPlaneMesh, visibilityPlane.transform.InverseTransformPoint(vi), (planeWidth / widthResolution), (planeHeight / heightResolution))) {
                    AnalyzablePoints.Add(new Vector2(vi.x, vi.z), new Vector2Int(x, z));
                }
                progress += progressStep;
            }
            if(EditorUtility.DisplayCancelableProgressBar("Visibility Plane Generator", "Generating Visibility Plane data", progress)) {
                EditorUtility.ClearProgressBar();
                return;
            }
        }
        EditorUtility.ClearProgressBar();
    }

    // private void OnDrawGizmos() {
    //     foreach (Vector2 vi2 in AnalyzablePoints.Keys) {
    //         Vector3 vi = new Vector3(vi2.x, transform.position.y + 0.5f, vi2.y);
    //         Gizmos.color = Color.blue;//TODO: Remove
    //         Gizmos.DrawSphere(vi, 0.05f);//TODO: Remove
    //     }
    // }
}
