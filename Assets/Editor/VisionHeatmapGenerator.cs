using UnityEngine;
using UnityEditor;
using System.IO;

public class VisionHeatmapGenerator {
	private const int textureSize = 512;
	private const float maxDistance = 25f;

	[MenuItem("Tools/Vision/Generate Heatmap for Selected Enemy")]
	private static void GenerateHeatmap() {
		if (Selection.activeGameObject is GameObject go && go.TryGetComponent(out Enemy enemy) && enemy.vision is EnemyVision vision) {
			Texture2D heatmap = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
			Color[] pixels = new Color[textureSize * textureSize];
			for (int y = 0; y < textureSize; y++) {
				for (int x = 0; x < textureSize; x++) {
					float xPos = ((float)x / (textureSize - 1) - 0.5f) * 2 * maxDistance;
					float zPos = ((float)y / (textureSize - 1) - 0.5f) * 2 * maxDistance;
					Vector3 targetPosition = new Vector3(xPos, 0, zPos);
					
					float spotTime = vision.GetRate(Vector3.Angle(Vector3.forward, targetPosition.normalized), 0);

					Color pixelColor;
					if (float.IsPositiveInfinity(spotTime)) { pixelColor = Color.magenta; }
					else { pixelColor = Ext.MultiLerp(new[] { Color.black, Color.blue, Color.yellow, Color.red }, Mathf.InverseLerp(0, 15, spotTime)); }

					pixels[y * textureSize + x] = pixelColor;
				}
			}

			heatmap.SetPixels(pixels);
			heatmap.Apply();
			byte[] bytes = heatmap.EncodeToPNG();

			string path = "Assets/Editor/vision_heatmap.png";
			if (!string.IsNullOrEmpty(path)) {
				File.WriteAllBytes(path, bytes);
				Debug.Log("Vision heatmap saved to: " + path);
				AssetDatabase.Refresh();
			}
			Object.DestroyImmediate(heatmap);
		} else {
			EditorUtility.DisplayDialog("Error", "Please select a gameobject with an Enemy attached", "Damn");
		}
	}
}
