using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityTextureGenerator
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

		//TODO: Remove Debug code
  //      for(int i = 0; i < width * height; i++) {
		//	colourMap[i] = new Color(
  //           UnityEngine.Random.Range(0f, 1f),
		//	 UnityEngine.Random.Range(0f, 1f),
		//	 UnityEngine.Random.Range(0f, 1f)
		//   );

		//}

		return TextureFromColourMap(colourMap, width, height);
	}

}
