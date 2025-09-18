using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof (EnemyAI))]
public class EnemyAIEditor : Editor {
    private Color arcColorMax = new Color(0, 1, 0, 0.25f);
    private Color arcColorMin = new(1, 1, 1, 0.1f);
    
    void OnSceneGUI() {
        EnemyAI ai = (EnemyAI)target;
        Vector3 drawPos = ai.transform.position;
        if(ai.eyes) drawPos = ai.eyes.transform.position;
        Handles.color = arcColorMax;
        Handles.DrawWireArc(drawPos, Vector3.up, Vector3.forward, 360, ai.sightRange);

        LayerMask mask = LayerMask.GetMask("Environment");

        for (int i = 0; i <= 10; i++) {
						continue; // todo: figure out what to do with this annoying fucking nonsense
            float angle = Mathf.Lerp(0, ai.viewAngleMax, i / 10f);
            Handles.color = Color.Lerp(arcColorMax, arcColorMin, i / 10f);
            DrawRay(drawPos, DirFromAngle(-angle), ai.sightRange, mask);
            DrawRay(drawPos, DirFromAngle(angle), ai.sightRange, mask);
        }
        
        string debug = "";
        //debug += (ai.currentState != null ? ai.fsm.currentState.ToString() : "No state") + "\n";
        debug += (ai.target ? ai.target.name : "No target") + "\n";    
        if(ai.debugs) Handles.Label(drawPos + Vector3.up * 3f, debug, ai.gui);
    }

    private void DrawRay(Vector3 drawPos, Vector3 angle, float range, LayerMask mask) {
        RaycastHit hitA;
        Vector3 endPoint = drawPos + angle * range;
        if (Physics.Raycast(drawPos, angle, out hitA, range, mask)) { endPoint = hitA.point; }
        Handles.DrawLine(drawPos, endPoint);
    }
    
    public Vector3 DirFromAngle(float angleInDegrees) {
        angleInDegrees += ((EnemyAI)target).transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

		public override void OnInspectorGUI() {
			if (GUILayout.Button("Find cover")) {
				EnemyAI ai = (EnemyAI)target;
				ai.ResetCover();
			}
			base.OnInspectorGUI();
		}
}
