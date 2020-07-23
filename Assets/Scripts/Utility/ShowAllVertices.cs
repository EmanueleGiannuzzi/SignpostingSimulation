using System.Collections.Generic;
using UnityEngine;

public class ShowAllVertices : MonoBehaviour {

    List<Vector3> VerticeList = new List<Vector3>();
    List<Vector3> VerticeListToShow = new List<Vector3>();
    public float sphereSize = 0.5f;

    List<Color> Colors = new List<Color>() { Color.red, Color.blue, Color.yellow, Color.green, Color.cyan, Color.magenta, Color.gray, Color.black, Color.white, Color.red, Color.blue }; //each color for each column
    void OnDrawGizmos() {

        int b = 0; //b is used to divide points into columns 
                   //if(VerticeList.Count > 0)
        for(int a = 0; a < VerticeListToShow.Count; a++) {
            Gizmos.color = Colors[b++ % Colors.Count];
            Gizmos.DrawSphere(VerticeListToShow[a], sphereSize);
        }
    }

    void Start() {

        //int width = 20;
        //int height = 20;

        //Mesh goMesh = this.GetComponent<MeshFilter>().sharedMesh;
        //float maxY = -float.MaxValue;
        //foreach(Vector3 vertex in goMesh.vertices) {
        //    Debug.Log("V: " + vertex);
        //    if(vertex.z > maxY) {
        //        maxY = vertex.z;
        //    }
        //}

        //for(int i = 0; i < goMesh.vertices.Length; i++) {
        //    Vector3 vertex = goMesh.vertices[i];
        //    if(vertex.z == maxY) {
        //        VerticeListToShow.Add(transform.TransformPoint(vertex));
        //    }
        //}



        //Mesh mesh = this.GetComponent<MeshFilter>().mesh;

        //Bounds meshRendererBounds = this.GetComponent<MeshRenderer>().bounds;
        //Vector3 cornerMin = meshRendererBounds.min;
        //float planeWidth = meshRendererBounds.extents.x * 2;
        //float planeHeight = meshRendererBounds.extents.z * 2;


        //Vector3 localMin = this.transform.InverseTransformPoint(meshRendererBounds.min);
        //Vector3 localMax = this.transform.InverseTransformPoint(meshRendererBounds.max) - localMin;


        //int widthResolution = 157;
        //int heightResolution = 197;
        //Vector2[] uvs = mesh.uv;

        //for(int i = 0; i < uvs.Length; i++) {
        //    Vector3 v = new Vector3((uvs[i].x / widthResolution) * localMax.x, this.transform.position.y, (uvs[i].y / heightResolution) * localMax.z) ;
        //    VerticeListToShow.Add(v);
        //}



        //Bounds bounds = GetComponent<MeshFilter>().mesh.bounds;
        //Debug.Log(bounds.min);
        //Debug.Log(transform.TransformPoint(bounds.center));
        //VerticeListToShow.Add(transform.TransformPoint(bounds.center));
        //VerticeListToShow.Add(transform.TransformPoint(bounds.min));
        //VerticeListToShow.Add(transform.TransformPoint(bounds.max));


        //for(int z = 0; z <= height; z++) {
        //    for(int x = 0; x <= width; x++) {
        //        //Debug.Log(((planeWidth / width) * x) + " | " + ((planeHeight / height) * z));
        //        Vector3 vi = new Vector3(cornerMin.x + ((planeWidth / width) * x), this.transform.position.y, cornerMin.z + ((planeHeight / height) * z));
        //        VerticeListToShow.Add(vi);
        //    }
        //}




        //Bounds bounds = GetComponent<MeshFilter>().mesh.bounds;
        //Debug.Log(transform.TransformPoint(bounds.center));
        //VerticeListToShow.Add(transform.TransformPoint(bounds.center));
        //VerticeListToShow.Add(transform.TransformPoint(bounds.min));
        //VerticeListToShow.Add(transform.TransformPoint(bounds.max));



        //VerticeList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices); //get vertice points from the mesh of the object

        //int width = 20;
        //int height = 20;

        //Vector3[] vertices = this.GetComponent<MeshFilter>().sharedMesh.vertices;
        //List<Vector3> corners = new List<Vector3>();
        //corners.Add(transform.TransformPoint(vertices[0]));
        //corners.Add(transform.TransformPoint(vertices[10]));
        //corners.Add(transform.TransformPoint(vertices[110]));
        //corners.Add(transform.TransformPoint(vertices[120]));
        //float planeWidth = this.GetComponent<MeshRenderer>().bounds.size.x;
        //float planeHeight = this.GetComponent<MeshRenderer>().bounds.size.z;


        //for(int y = 0; y < height; y++) {
        //    for(int x = 0; x < width; x++) {
        //        Vector3 vi = new Vector3(corners[0].x - ((planeWidth / width) * x), this.transform.position.y, corners[0].z - ((planeHeight / height) * y));
        //        VerticeListToShow.Add(vi);
        //    }
        //}




        //foreach(Vector3 point in VerticeList) //all the points are added to be shown on the editor
        //{
        //    VerticeListToShow.Add(transform.TransformPoint(point));
        //}
        //Debug.Log(name + " has " + VerticeListToShow.Count + " vertices on it.");
    }

}