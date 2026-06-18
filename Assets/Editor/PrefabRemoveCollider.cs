using UnityEngine;
using UnityEditor;

public static class MeshColliderAffixer {
    [MenuItem("Tools/Affix Mesh Colliders")]
    private static void Affix() {
        foreach (var root in Selection.gameObjects) {
            Mesh foundMesh = null;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true)) {
                if (r is MeshRenderer && r.TryGetComponent<MeshFilter>(out var mf) && mf.sharedMesh) { foundMesh = mf.sharedMesh; break; }
                if (r is SkinnedMeshRenderer smr && smr.sharedMesh) { foundMesh = smr.sharedMesh; break; }
            }

            if (foundMesh) {
                var mc = Undo.AddComponent<MeshCollider>(root);
                mc.sharedMesh = foundMesh;
            }
        }
    }
}
