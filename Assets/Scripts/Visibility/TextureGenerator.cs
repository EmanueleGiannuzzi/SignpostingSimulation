using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityTextureGenerator : MonoBehaviour
{
	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colourMap);
		texture.Apply();
		return texture;
	}

	public static Texture2D TextureFromVisibilityData(Dictionary<Vector2Int, VisibilityInfo> visibilityData, SignageBoard[] signageBoards, int width, int height, Color nonVisibleColor) {
		Color[] colourMap = new Color[width * height];
		Utility.FillArray(colourMap, nonVisibleColor);

		foreach(KeyValuePair<Vector2Int, VisibilityInfo> entry in visibilityData) {
			Vector2Int coords = entry.Key;
			Color visibleColor = Color.white;
			foreach(int signageboardID in entry.Value.GetVisibleBoards()) {
				visibleColor *= signageBoards[signageboardID].GetColor();
			}
			colourMap[coords.y * width + coords.x] = visibleColor;
		}

		return TextureFromColourMap(colourMap, width, height);
	}



	//public static Texture2D TextureFromSignboard(GameObject plane, SignageBoard signageBoard, Color colorVisible, Color colorNotVisible) {
	//	int width = 512;
	//	int height = 512;

	//	Vector3 p = signageBoard.getCenterPoint();
	//	Vector3 n = signageBoard.getDirection();
	//	float theta = (signageBoard.getViewingAngle() * Mathf.PI) / 180;
	//	float d = signageBoard.getViewingDistance();

	//	Vector3[] vertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;
	//	Vector3 corner = plane.transform.TransformPoint(vertices[0]);
	//	float planeWidth = plane.GetComponent<MeshRenderer>().bounds.size.x;
	//	float planeHeight = plane.GetComponent<MeshRenderer>().bounds.size.z;

	//	Color[] colourMap = new Color[width * height];

	//	for(int y = 0; y < height; y++) {
	//		for(int x = 0; x < width; x++) {
	//			Vector3 vi = new Vector3(corner.x - ((planeWidth / width) * x), plane.transform.position.y, corner.z - ((planeHeight / height) * y));
	//			Color colorToApply = colorNotVisible;

	//			if((Vector3.Dot((vi - p), n) / ((vi - p).magnitude * n.magnitude)) >= Mathf.Cos(theta / 2)
	//				&& ((vi - p).magnitude <= d) ) {

	//				Ray ray = new Ray(vi, p);
	//				float maxDistance = Vector3.Distance(p, vi);
	//				RaycastHit hit;
	//				if(!Physics.Raycast(ray, out hit)) {
	//					colorToApply = colorVisible;
	//				}
	//    //               else {
	//				//	Debug.DrawLine(p, vi, Color.red);
	//				//}
	//			}
	//			colourMap[y * width + x] = colorToApply;

	//		}
	//	}

	//	return TextureFromColourMap(colourMap, width, height);
	//}
}
