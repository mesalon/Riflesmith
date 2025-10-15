using UnityEngine;

public class Enemy : MonoBehaviour {
	public Blackboard blackboard;
	public EnemyLocomotion locomotion;
	public EnemyVision vision;
	public EnemyBody body;
	private Node brain;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true;

	private void Awake() {
		locomotion = new(blackboard);
		vision = new(blackboard);
		body = new(blackboard);
		brain = new SelectorNode(new() {
				new PatrolTask(this),
				});
	}

	private void Update() {
		Node active = null;
		if (body.isUp) {
			if (runLocomotion) locomotion.Tick();
			if (runBody) body.Tick();
			brain.Evaluate(out active);
		}
		//Ext.Label(transform.position + 2 * Vector3.up, $"node: {active}");
	}

	private void FixedUpdate() {
		if (body.isUp) {
			if (runVision) vision.Tick();
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
