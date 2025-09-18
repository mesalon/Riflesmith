using UnityEngine;
using TMPro;

public static class DebugOverlay {
    private static GameObject debugOverlayPrefab {
        get {
            if (_debugOverlayPrefab == null)
                _debugOverlayPrefab = Resources.Load<GameObject>("DebugOverlayPrefab");
            return _debugOverlayPrefab;
        }
    }

    private static GameObject _debugOverlayPrefab;
    private const string DebugOverlayName = "DebugOverlay";

    public static void CreateOverlay(Transform targetTransform, float offset, params (string name, object value)[] variables) {
        if (debugOverlayPrefab == null) return;

        Transform debugOverlayTransform = targetTransform.Find(DebugOverlayName);
        GameObject debugOverlay;
        TextMeshPro tmp;

        if (debugOverlayTransform == null) {
            debugOverlay = GameObject.Instantiate(debugOverlayPrefab, targetTransform);
            debugOverlay.name = DebugOverlayName;
            debugOverlay.transform.localPosition = Vector3.up * offset / targetTransform.lossyScale.y;
            tmp = debugOverlay.GetComponent<TextMeshPro>();
            if (tmp == null) return;
        }
        else {
            debugOverlay = debugOverlayTransform.gameObject;
            tmp = debugOverlay.GetComponent<TextMeshPro>();
        }

        System.Text.StringBuilder debugText = new System.Text.StringBuilder();
        foreach ((string name, object value) variable in variables) {
            debugText.AppendLine($"{variable.name}: {variable.value}");
        }

        tmp.text = debugText.ToString();
    }
}