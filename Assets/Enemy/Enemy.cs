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
		try {
			brain = new SelectorNode(new() {
					new SelectorNode(new() {
							// Survive
							new SequenceNode(new() {
									new IsPlayerVisible(this),
									new FindCover(this),
									new MoveToCoverTask(this),
									}),
							// Engage
							new SequenceNode(new() {
									new HasLOS(this),
									new HasAmmo(this),
									new EngageEnemyTask(this),
									}),
							// Hunt
							// Patrol
							//new MoveToCoverTask(this),
							}),
					new PatrolTask(this),
					});
		} catch (System.Exception e) {
			Debug.Log($"Error in creation of brain.\n{e}");
		}
	}

	private void Update() {
		Node active = null;
		if (body.isUp) {
			locomotion.Tick();
			vision.Tick();
			vision.Debug();
			body.Tick();
			try {
				brain.Evaluate(out active);
			} catch (System.Exception e) {
				Debug.Log($"Error in execution of brain.\n{e}");
			}
		}
		Ext.Label(transform.position + 2 * Vector3.up, $"node: {active}");
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
