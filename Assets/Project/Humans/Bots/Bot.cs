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

public class Bot : Interactor {
	public Vector3 test;
	public BotConfig cfg;
	public Transform eyes;
	public Transform weaponHolster, weaponAimPose, weaponRestPose;
	public Transform ikLookTarget;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true, runHandling = true;

	public BotVision vision;
	public BotBody body;
	public BotHandling handling;
	public BotLocomotion motionController;
	public AIBrain brain;
	[HideInInspector] public Human self;

	private void Awake() {
		self = GetComponent<Human>();
		motionController = new(this);
		vision = new(this);
		handling = new(this);
		brain = new(this);
		body = new();
	}

	private void Update() {
		if (!handling.weapon) {
			Collider[] overlap = Physics.OverlapSphere(self.Center, 2f);
			foreach (Collider col in overlap) {
				if (col.TryGetComponent(out SimpleFirearm gun)) { 
					handling.weapon = gun; 
					handling.weapon.AttachTo(weaponHolster);
				}
			}
		}
		if (body.isUp) {
			if (runLocomotion) motionController.Tick();
			if (runHandling) handling.Tick();
			if (runBrain) brain.Tick();
		}
	}

	public void Damage(float amount) {
		Damage(amount);
		body.strength = Mathf.Max(0, body.strength - amount);
	}

	void OnAnimatorMove() {
		motionController.AnimatorMove();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Bot))]
public class BotEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		EditorGUILayout.Space();
		Bot e = (Bot)target;
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
