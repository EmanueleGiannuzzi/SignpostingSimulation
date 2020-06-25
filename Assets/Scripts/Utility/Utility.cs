using UnityEngine;
using System;

public static class Utility
{

    public static bool HorizontalPlaneContainsPoint(Mesh aMesh, Vector3 aLocalPoint)
    {
        Vector3[] verts = aMesh.vertices;
        int[] tris = aMesh.triangles;
        int triangleCount = tris.Length / 3;
        for(int i = 0; i < triangleCount; i++) {
            Vector3 v1 = verts[tris[i * 3]];
            Vector3 v2 = verts[tris[i * 3 + 1]];
            Vector3 v3 = verts[tris[i * 3 + 2]];

            Barycentric bc = new Barycentric(v1, v2, v3, aLocalPoint);
            if(bc.IsInside) {
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
}
