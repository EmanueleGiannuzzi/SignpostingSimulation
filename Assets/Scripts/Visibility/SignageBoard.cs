
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Transform))]
//[ExecuteInEditMode]
public class SignageBoard : MonoBehaviour
{
    [Header("Signage Board Parameters")]
    public float viewingDistance;
    public float viewingAngle;
    public Color color;

    [Header("Analysys Result")]
    public float[] coveragePerAgentType;//[0,1]

    public void Start() {
        color = new Color(
         Random.Range(0f, 1f),
         Random.Range(0f, 1f),
         Random.Range(0f, 1f)
       );
    }

    //[Header("Editor Settings")]
    //public bool autoUpdate;

    public Vector3 GetDirection() {
        return this.transform.up;
    }

    public Vector3 GetWorldCenterPoint() {
        return this.transform.position;
    }

    public float GetViewingAngle() {
        return viewingAngle;
    }

    public float GetViewingDistance() {
        return viewingDistance;
    }

    public Color GetColor() {
        return color;
    }

    void Update() {
        //if(autoUpdate && !EditorApplication.isPlaying && transform.hasChanged) {
        //    FindObjectOfType<VisibilityHandler>().GenerateVisibilityData();
        //}
    }
}
