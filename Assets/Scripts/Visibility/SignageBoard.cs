
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

    public void Start() {
        color = new Color(
         Random.Range(0f, 1f),
         Random.Range(0f, 1f),
         Random.Range(0f, 1f)
       );
    }

    [Header("Editor Settings")]
    public bool autoUpdate;

    public Vector3 GetDirection() {
        return this.GetComponent<Transform>().up;
    }

    public Vector3 GetCenterPoint() {
        return this.GetComponent<Transform>().position;
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
