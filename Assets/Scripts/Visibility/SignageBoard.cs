using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Transform))]
//[ExecuteInEditMode]
//[InitializeOnLoad]
public class SignageBoard : MonoBehaviour {
    [Header("Signage Board Parameters")]
    public float ViewingDistance;
    public float ViewingAngle;
    public Color Color = Color.black;
    public float MinimumReadingTime;

    [Header("Analysys Result")]
    public float[] coveragePerAgentType;//[0,1]

    //[Header("Editor Settings")]
    //public bool autoUpdate;
    //public SignageBoard() {
    //    SceneView.duringSceneGui += view => {
    //        Event e = Event.current;
    //        if(autoUpdate && e != null && e.type == EventType.MouseUp && e.button == 0 && !EditorApplication.isPlaying && transform.hasChanged) {
    //            FindObjectOfType<Environment>().InitVisibilityHandlerData();
    //        }
    //    };
    //}

    public void UpdateVisibilityPlane() {
        
    }

    public void Start() {
        if(Color == Color.black) {
            Color = new Color(
             Random.Range(0f, 1f),
             Random.Range(0f, 1f),
             Random.Range(0f, 1f)
           );
        }
    }


    public Vector3 GetDirection() {
        return this.transform.up;
    }

    public Vector3 GetWorldCenterPoint() {
        return this.transform.position;
    }

    public float GetViewingAngle() {
        return ViewingAngle;
    }

    public float GetViewingDistance() {
        return ViewingDistance;
    }

    public Color GetColor() {
        return Color;
    }
}
