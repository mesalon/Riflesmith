using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public class Enemy : MonoBehaviour {
	public Blackboard blackboard;
	public EnemyLocomotion locomotion;
	public EnemyVision vision;
	public EnemyBody body;
	public EnemyHandling handling;
	public UtilityAI brain;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true, runHandling = true;

	private void Awake() {
		locomotion = new(blackboard);
		vision = new(blackboard);
		body = new(blackboard);
		handling = new(this);
		brain = new(this);
	}

	private void Update() {
		if (body.isUp) {
			if (runLocomotion) locomotion.Tick();
			if (runBody) body.Tick();
			if (runHandling) handling.Tick();
			if (runBrain) brain.Tick();
		}
	}

	private void FixedUpdate() {
		if (blackboard.target) {
			blackboard.targetLKP = blackboard.target.rig.head.position;
		} else {
			if (runVision && body.isUp && vision.Tick(out Player player)) { 
				blackboard.target = player; 
				blackboard.LKPAge = 0;
			}
		}
		if (!vision.CanSeePlayer) {
			blackboard.target = null;
			blackboard.LKPAge += Time.fixedDeltaTime;
		}
	}

	[ContextMenu("Generate Vision Heatmap")]
	private void GenerateVisionHeatmap() {
		vision.GenerateHeatmap();
	}
}

[System.Serializable] public struct BurstCfg {
	public bool enableFire;
	[Range(0, 1)] public float range, skill, ammo, intent, recoil;
	public IntRange burst;
	public FloatRange delay;
	public float rangeBurstWeight, skillBurstWeight, ammoBurstWeight, intentBurstWeight, recoilBurstWeight, panicBurstWeight;
	public float rangeDelayWeight, skillDelayWeight, ammoDelayWeight, intentDelayWeight, recoilDelayWeight, panicDelayWeight;
	public float burstWeightBase, delayWeightBase;
	public float inconsistencyBase;
}

#if UNITY_EDITOR
[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		EditorGUILayout.Space();
		Enemy e = (Enemy)target;
		var topItems = e.brain.actions
			.Select(a => new { Action = a, Score = a.GetScore() }) 
			.OrderByDescending(x => x.Score)
			.Take(5)
			.ToList();
		for (int i = 0; i < topItems.Count(); i++) {
			EditorGUILayout.LabelField($"{i}: {topItems[i].Action} | {topItems[i].Score}");
		}
	}
}
#endif
