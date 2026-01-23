using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SimpleSwapper : EditorWindow {
    GameObject target, replacement;

    [MenuItem("Tools/Simple Swapper")]
    static void Init() => GetWindow<SimpleSwapper>().Show();

    void OnGUI() {
        target = (GameObject)EditorGUILayout.ObjectField("Replace", target, typeof(GameObject), false);
        replacement = (GameObject)EditorGUILayout.ObjectField("With", replacement, typeof(GameObject), false);

        if (GUILayout.Button("Swap All") && target && replacement) {
            Swap();
        }
    }

    void Swap() {
        var objectsToSwap = new List<GameObject>();

        foreach (var go in FindObjectsByType<GameObject>(FindObjectsSortMode.None)) {
            if (PrefabUtility.GetCorrespondingObjectFromSource(go) == target)
                objectsToSwap.Add(go);
        }

        foreach (var go in objectsToSwap) {
            if (go == null) continue;
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacement, go.transform.parent);
            Undo.RegisterCreatedObjectUndo(newObj, "Swap Prefab");
            newObj.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);
            newObj.transform.localScale = go.transform.localScale;
            Undo.DestroyObjectImmediate(go);
        }
    }
}
