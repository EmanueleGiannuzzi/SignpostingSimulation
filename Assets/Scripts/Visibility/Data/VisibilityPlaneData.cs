using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class VisibilityPlaneData : MonoBehaviour {
    [ReadOnly]
    [UnityEngine.SerializeField]
    private float originalFloorHeight;
    public float OriginalFloorHeight {
        set { originalFloorHeight = value; }
        get { return originalFloorHeight; }
    }

    [ReadOnly]
    [UnityEngine.SerializeField]
    private Vector2Int axesResolution; 

    [ReadOnly]
    [UnityEngine.SerializeField]
    private int validMeshPointsCount; 
    public int ValidMeshPointsCount {
        set { validMeshPointsCount = value; }
        get { return validMeshPointsCount; }
    }

    public void SetResolution(int widthRes, int heightRes) {
        axesResolution = new Vector2Int(widthRes, heightRes);
    }

    public Vector2Int GetAxesResolution() {
        return axesResolution;
    }

    

}
