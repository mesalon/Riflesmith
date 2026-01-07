using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrabPoseUtility))]
public class BonePoseCaptureEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Capture")) { ((GrabPoseUtility)target).Capture(); }
    }
}
