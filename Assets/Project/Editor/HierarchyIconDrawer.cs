using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DrawIconInHierarchy {
    static DrawIconInHierarchy() {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect) {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null)
            return;

        Texture2D icon = EditorGUIUtility.GetIconForObject(gameObject) as Texture2D;
        if (icon == null) return;
        Rect iconRect = new Rect(selectionRect.xMax, selectionRect.y, 16, 16);
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
    }
}