/* AI Todo
Change the reaction of shootaction based on alertness
Modify vision by alertness
The handling of the gun is really janky and so is the viewing. I should fix that
 */
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public class Enemy : Actor {
	public EnemyVision vision;
	public EnemyBody body;
	public EnemyHandling handling;
	public EnemyLocomotion motionController;
	public UtilityAI brain;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true, runHandling = true;

	public EnemyConfig cfg;
	public EnemyFirearm weapon;
	public Transform eyes;
	public Transform weaponAimPose, weaponRestPose;
	public Transform weaponHandle;
	public Transform ikTarget;
	public Animator anim;
	public AnimationClip getUpClip;
	public Transform focus;
	public Transform dingle;
	public Seeker seeker;
	public Transform coreRag;
	public List<Transform> ragdollReference;
	public List<ConfigurableJoint> joints;
	public bool coverDebug, coverDebugFull;
	public float aimError, fixAmount, fixVariance;

	[HideInInspector] public CoverQuery cover;
	[HideInInspector] public Actor target;
	[HideInInspector] public Vector3? aimFocus;
	[HideInInspector] public Vector3? targetLKP;
	[HideInInspector] public float confidence, alertness, suppression;
	[HideInInspector] public float LKPAge;
	[HideInInspector] public bool expectsToSeeTarget;

	private void Awake() {
		motionController = new(this);
		vision = new(this);
		body = new(this);
		handling = new(this);
		brain = new(this);
	}

	private void Update() {
		if (coverDebug && cover != null && cover.TryGetCover(out CoverTask _)) { cover.ShowDebug(coverDebugFull); }
		if (body.isUp) {
			if (runLocomotion) locomotion.Tick();
			if (runHandling) handling.Tick();
			if (runBrain) brain.Tick();
		}
		if (runBody) body.Tick();
	}

	private void FixedUpdate() {
		if (target) {
			targetLKP = target.Center;
		} else {
			if (runVision && body.isUp && vision.Tick(out Actor target)) { 
				this.target = target; 
				LKPAge = 0;
			}
		}
		if (!vision.HasLOS(target)) {
			target = null;
			if (expectsToSeeTarget) LKPAge += Time.fixedDeltaTime;
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
