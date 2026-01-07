using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraUtility : EditorWindow {
    [MenuItem("Tools/Screenshot Utility &s")]
    static void AlignAndCapture() {
        GameObject obj = Selection.activeGameObject;
        if (!obj || !obj.TryGetComponent<Camera>(out Camera cam)) return;

        SceneView view = SceneView.lastActiveSceneView;
        if (!view) return;

        cam.transform.SetPositionAndRotation(view.camera.transform.position, view.camera.transform.rotation);
        cam.fieldOfView = view.camera.fieldOfView;

        // Use 32-bit depth for better color quality
        RenderTexture rt = new RenderTexture(2560, 1440, 32, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        // Use ARGB32 for higher color precision
        Texture2D screenshot = new Texture2D(2560, 1440, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 2560, 1440), 0, 0);
        screenshot.Apply();

        string savePath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            "Media",
            "Screenshots"
        );
        Directory.CreateDirectory(savePath);
        string filePath = Path.Combine(savePath, $"Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
        File.WriteAllBytes(filePath, screenshot.EncodeToPNG());

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenshot);
    }

    [MenuItem("Tools/Screenshot Utility &s", true)]
    static bool Validate() => Selection.activeGameObject != null;
}