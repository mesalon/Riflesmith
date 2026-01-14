/* AI Todo
Change the reaction of shootaction based on alertness
Modify vision by alertness
The handling of the gun is really janky and so is the viewing. I should fix that
 */
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public class Enemy : Actor {
	public EnemyVision vision;
	public EnemyBody body;
	public EnemyHandling handling;
	public EnemyLocomotion motionController;
	public EnemyBrain brain;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true, runHandling = true;

	public EnemyConfig cfg;
	public SimpleFirearm weapon;
	public Transform eyes;
	public Transform weaponAimPose, weaponRestPose;
	public Transform lookTarget;
	public Animator anim;

	private void Awake() {
		motionController = new(this);
		vision = new(this);
		handling = new(this);
		brain = new(this);
		body = new();
	}

	private void Update() {
		if (body.isUp) {
			if (runLocomotion) motionController.Tick();
			if (runHandling) handling.Tick();
			if (runBrain) brain.Tick();
		}
	}


	public override void Damage(float amount) {
		health = Mathf.Max(0, health - amount);
		body.strength = Mathf.Max(0, body.strength - amount);
	}

	[ContextMenu("Generate Vision Heatmap")]
	private void GenerateVisionHeatmap() {
		vision.GenerateHeatmap();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		EditorGUILayout.Space();
		Enemy e = (Enemy)target;
		if (e.brain != null) {
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
}
#endif
