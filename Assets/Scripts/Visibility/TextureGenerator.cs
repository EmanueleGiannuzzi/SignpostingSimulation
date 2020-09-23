using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityTextureGenerator
{
	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(colourMap);
		texture.Apply();
		return texture;
	}

	
	public static Texture2D TextureFromVisibilityData(Dictionary<Vector2Int, VisibilityInfo> visibilityData, SignageBoard[] signageBoards, int width, int height, Color nonVisibleColor) {
		Color[] colourMap = new Color[width * height];
		Utility.FillArray(colourMap, nonVisibleColor);

		foreach(KeyValuePair<Vector2Int, VisibilityInfo> entry in visibilityData) {
			Vector2Int coords = entry.Key;

			Color visibleColor = new Color(0, 0, 0, 0);
			foreach(int signageboardID in entry.Value.GetVisibleBoards()) {
				visibleColor += signageBoards[signageboardID].GetColor();
			}
			visibleColor /= entry.Value.GetVisibleBoards().Count;

			visibleColor.a = nonVisibleColor.a;

			colourMap[coords.y * width + coords.x] = visibleColor;
		}

		return TextureFromColourMap(colourMap, width, height);
	}

	public static Texture2D BestSignboardTexture(GameObject signboardGridGroup, int agentTypeID, int visPlaneID, int width, int height, float minVisibility, float maxVisibility, Gradient gradient) {
		Color[] colorMap = new Color[width * height];
		Utility.FillArray(colorMap, gradient.Evaluate(0f));

		foreach(Transform child in signboardGridGroup.transform.GetChild(visPlaneID)) {
			SignageBoard signboard = child.gameObject.GetComponent<SignageBoard>();
			GridSignageboard gridSignboard = child.gameObject.GetComponent<GridSignageboard>();

			float visibility = signboard.GetVisiblityForHeatmap()[agentTypeID];
            float visiblityNorm = (visibility / (maxVisibility - minVisibility)) + minVisibility;

            colorMap[gridSignboard.planeLocalIndex.y * width + gridSignboard.planeLocalIndex.x] = gradient.Evaluate(visiblityNorm);
		}

		return TextureFromColourMap(colorMap, width, height);
	}

}
