using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public static class Utility {

    public static bool PolyContainsPoint(Vector3[] polyPoints, Vector3 p) {
        var j = polyPoints.Length - 1;
        var inside = false;
        for(int i = 0; i < polyPoints.Length; j = i++) {
            var pi = polyPoints[i];
            var pj = polyPoints[j];
            if(((pi.z <= p.z && p.z < pj.z) || (pj.z <= p.z && p.z < pi.z)) &&
                (p.x < (pj.x - pi.x) * (p.z - pi.z) / (pj.z - pi.z) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    public static bool HorizontalPlaneContainsPoint(Mesh mesh, Vector3 aLocalPoint, float squareWidthLength, float squareHeightLenght) {
        Vector3[] verts = mesh.vertices;
        int[] tris = mesh.triangles;
        int triangleCount = tris.Length / 3;
        for(int i = 0; i < triangleCount; i++) {
            Vector3[] trianglePoly = { verts[tris[i * 3]], verts[tris[i * 3 + 1]], verts[tris[i * 3 + 2]] };

            if(PolyContainsPoint(trianglePoly, aLocalPoint)
                || PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x - squareWidthLength, aLocalPoint.y, aLocalPoint.z - squareHeightLenght))
                || PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x - squareWidthLength, aLocalPoint.y, aLocalPoint.z + squareHeightLenght))
                || PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x + squareWidthLength, aLocalPoint.y, aLocalPoint.z - squareHeightLenght))
                //|| PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x + squareWidthLength, aLocalPoint.y, aLocalPoint.z + squareHeightLenght))
                ) {
                return true;
            }
        }
        return false;
    }

    public static void FillArray<T>(this T[] arr, T value) {
        for(int i = 0; i < arr.Length; ++i) {
            arr[i] = value;
        }
    }

    public static void DeleteFile(string path) {
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public static System.Diagnostics.Process RunCommand(string commandFileName, string arg, bool showConsole) {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

        if(!showConsole) {
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;
        }

        startInfo.FileName = commandFileName;
        startInfo.Arguments = arg;
        process.StartInfo = startInfo;

        try {
            process.Start();
        }
        catch(Exception) {
            throw;
        }

        return process;
    }

    public static T[] RemoveAt<T>(this T[] source, int index) {
        T[] dest = new T[source.Length - 1];
        if(index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if(index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static Mesh GetTopMeshFromGameObject(GameObject gameObject, out float floorHeight) {
        MeshFilter goMeshFilter = gameObject.GetComponent<MeshFilter>();
        if(goMeshFilter == null || goMeshFilter.sharedMesh == null) {
            floorHeight = 0f;
            return null;
        }

        Mesh goMesh = goMeshFilter.sharedMesh;
        float higherCoord = -float.MaxValue;
        foreach(Vector3 vertex in goMesh.vertices) {
            if(vertex.z > higherCoord) {
                higherCoord = vertex.z;
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> invalidVerticesIDs = new List<int>();
        List<int> triangles = new List<int>();
        Dictionary<int, int> conversionTable = new Dictionary<int, int>();

        int j = 0;//New array id
        for(int i = 0; i < goMesh.vertices.Length; i++) {
            Vector3 vertex = goMesh.vertices[i];
            if(vertex.z == higherCoord) {
                Vector3 v = new Vector3(-vertex.x, vertex.y, 0);
                vertices.Add(v);
                conversionTable.Add(i, j);
                j++;
            }
            else {
                invalidVerticesIDs.Add(i);
            }
        }

        int triangleCount = goMesh.triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++) {
            int v1 = goMesh.triangles[i * 3];
            int v2 = goMesh.triangles[i * 3 + 1];
            int v3 = goMesh.triangles[i * 3 + 2];

            if(!(invalidVerticesIDs.Contains(v1)
                || invalidVerticesIDs.Contains(v2)
                || invalidVerticesIDs.Contains(v3))) {//If triangle is valid
                triangles.Add(conversionTable[v1]);
                triangles.Add(conversionTable[v3]);//Reverse order(Counter-clockwise)
                triangles.Add(conversionTable[v2]);
            }
        }

        Vector3[] meshVertices = vertices.ToArray();

        Quaternion newRotation = new Quaternion {
            eulerAngles = new Vector3(-90, 0, 0)
        };
        for(int i = 0; i < meshVertices.Length; i++) {
            meshVertices[i] = newRotation * meshVertices[i];
        }

        Mesh mesh = new Mesh {
            vertices = meshVertices,
            triangles = triangles.ToArray()
        };

        mesh.name = goMesh.name;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        floorHeight = higherCoord;

        return mesh;
    }

    public static Vector2 Vector3ToVerctor2NoY(Vector3 v3) {
        return new Vector2(v3.x, v3.z);
    }
}
