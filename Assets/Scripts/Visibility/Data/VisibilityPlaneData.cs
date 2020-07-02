using Unity.Collections;
using UnityEngine;

public class VisibilityPlaneData : MonoBehaviour {
    [ReadOnly]
    [UnityEngine.SerializeField]
    private float originalFloorHeight;

    public float OriginalFloorHeight
    {
        set { originalFloorHeight = value; }
        get { return originalFloorHeight; }
    }

}
