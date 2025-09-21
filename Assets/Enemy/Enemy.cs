using UnityEngine;

public class Enemy : MonoBehaviour {
	public Blackboard blackboard;
	public EnemyLocomotion locomotion;
	public EnemyVision vision;
	public EnemyBody body;
	private Node brain;

	private void Awake() {
		locomotion = new(blackboard);
		vision = new(blackboard);
		body = new(blackboard);
		brain = new SelectorNode(new() {
				new SelectorNode(new() {
						new GTFOTask(this),
						new MoveToCoverTask(this),
						new EngageEnemyTask(this),
						}),
				new PatrolTask(this),
				});
	}

	private void Update() {
		if (body.isUp) {
			locomotion.Tick();
			vision.Tick();
			body.Tick();
			brain.Evaluate(out Node active);
		}
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
