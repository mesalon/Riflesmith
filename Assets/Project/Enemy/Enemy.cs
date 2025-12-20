using UnityEngine;

public class Enemy : MonoBehaviour {
	public Blackboard blackboard;
	public EnemyLocomotion locomotion;
	public EnemyVision vision;
	public EnemyBody body;
	public EnemyHandling handling;
	private UtilityAI brain;
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
		if (!blackboard.target) {
			if (runVision && body.isUp && vision.Tick(out Player player)) { 
				blackboard.target = player; 
				blackboard.seenTime = 0;
			}
		} else {
			blackboard.targetLKP = blackboard.target.rig.head.position;
			if (!vision.CanSeePlayer) {
				blackboard.target = null;
				blackboard.seenTime += Time.fixedDeltaTime;
			}
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
