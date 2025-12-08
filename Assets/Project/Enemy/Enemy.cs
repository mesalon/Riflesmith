using UnityEngine;

public class Enemy : MonoBehaviour {
	public Blackboard blackboard;
	public EnemyLocomotion locomotion;
	public EnemyVision vision;
	public EnemyBody body;
	private UtilityAI brain;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true;

	private void Awake() {
		locomotion = new(blackboard);
		vision = new(blackboard);
		body = new(blackboard);
		brain = new(this);
	}

	private void Update() {
		if (body.isUp) {
			if (runLocomotion) locomotion.Tick();
			if (runBody) body.Tick();
			brain.Tick();
		}
	}

	private void FixedUpdate() {
		if (!blackboard.target) {
			if (runVision && body.isUp && vision.Tick(out Player player)) { 
				blackboard.target = player; 
				blackboard.targetLKP = player.transform.position;
			}
		} else {
			if (!vision.CanSeePlayer) {
				blackboard.target = null;
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
