using UnityEngine;
using System.IO;
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

}
