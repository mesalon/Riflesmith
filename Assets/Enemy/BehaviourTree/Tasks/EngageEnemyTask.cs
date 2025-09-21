using UnityEngine;

public class EngageEnemyTask : Node { // This system is flawed. A shooter's firing solution is not based on just simple variables like this. It is based on moving the gun back from the recoiled position onto target before firing.
	// todo: this system fucking blows
	private EnemyFirearm gun => ctx.blackboard.weapon;
	private readonly Enemy ctx;

	public EngageEnemyTask(Enemy ctx) {
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		gun.triggerState = true;
		return NodeState.Running;
	}
}
/*

	private float t;
	private int currentBurstLength;
	private int burstStartRounds;
	private float currentBurstDelay;
	private float timeSinceBurst;

		active = this;
		if (!ctx.target) return NodeState.Failure;
		EnemySettings opts = ctx.settings;
		ctx.locomotion.ADS(true);
		ctx.locomotion.LookAt(ctx.target.rig.head.position);

		float range = Mathf.InverseLerp(0, 100 * 100, (ctx.target.transform.position - ctx.transform.position).sqrMagnitude);
		// todo panic, intent, etc etc holy fuck

		float inconsistency = opts.inconsistencyBase - (ctx.skill * (1 - (ctx.panic * 0.75f))); // all hail the monolith 
		if (ctx.enableFire && !gun.triggerState && timeSinceBurst > currentBurstDelay) {
			float lengthRatio = opts.burstWeightBase;
			lengthRatio += (range - 0.5f) * opts.rangeBurstWeight;
			lengthRatio += (ctx.skill - 0.5f) * opts.skillBurstWeight;
			lengthRatio += (ctx.intent - 0.5f) * opts.intentBurstWeight;
			lengthRatio += (ctx.panic - 0.5f) * opts.panicBurstWeight;
			float idealBurst = Mathf.Lerp(opts.burst.Min, opts.burst.Max, lengthRatio);
			float burstVarianceRange = (opts.burst.Max - opts.burst.Min) * 0.5f * inconsistency;
			float finalBurst = idealBurst + Random.Range(-burstVarianceRange, burstVarianceRange);
			currentBurstLength = Mathf.Clamp(Mathf.RoundToInt(finalBurst), opts.burst.Min, opts.burst.Max);

			currentBurstDelay = 0;
			burstStartRounds = gun.rounds;
			gun.triggerState = true;
		}

		if (gun.triggerState) {
			int roundsFired = burstStartRounds - gun.rounds;
			if (roundsFired >= currentBurstLength || gun.rounds == 0) {
				gun.triggerState = false;
				timeSinceBurst = 0;
				currentBurstLength = 0;

				float delayRatio = opts.delayWeightBase;
				delayRatio += (range - 0.5f) * opts.rangeDelayWeight;
				delayRatio += (ctx.skill - 0.5f) * opts.skillDelayWeight;
				delayRatio += (ctx.intent - 0.5f) * opts.intentDelayWeight;
				delayRatio += (ctx.panic - 0.5f) * opts.panicDelayWeight;
				float idealDelay = Mathf.Lerp(opts.delay.Min, opts.delay.Max, delayRatio);
				float delayVarianceRange = (opts.delay.Max - opts.delay.Min) * 0.5f * inconsistency;
				float finalDelay = idealDelay + Random.Range(-delayVarianceRange, delayVarianceRange);
				currentBurstDelay = Mathf.Clamp(finalDelay, opts.delay.Min, opts.delay.Max);
			}
		}

		string text = "";
		text += $"inconsistency: {inconsistency}\n";
		text += $"burst: {currentBurstLength}\n";
		text += $"delay: {currentBurstDelay}\n";
		Ext.Label(ctx.transform.position + Vector3.up * 2, text);
		timeSinceBurst += Time.deltaTime;
		return NodeState.Running;
 */
