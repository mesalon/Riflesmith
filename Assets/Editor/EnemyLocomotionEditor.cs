using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (EnemyLocomotion))]
public class EnemyLocomotionEditor : Editor {
    private void OnSceneGUI() {
        EnemyLocomotion ctx = (EnemyLocomotion)target;
        if (!Application.isPlaying) return;
        Ext.DrawPath(ctx.path.corners, Color.green, Color.red);
    }
}