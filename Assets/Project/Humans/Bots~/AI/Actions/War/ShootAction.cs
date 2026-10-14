using UnityEngine;

public class ShootAction : AIAction {
	private readonly Bot ctx;
	private AIBrain brain => ctx.brain;

	public ShootAction(Bot ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return brain.targetInSight ? 1 : 0;
	}

	public override void Enter() {
		ctx.motion.Stop();
	}

	public override void Tick() {
		ctx.handling.FireAt(brain.targetInSight.Center); 
	}

	public override void Exit() {
		ctx.handling.ADS(false);
	}
}
