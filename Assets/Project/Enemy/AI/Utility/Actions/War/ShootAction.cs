using UnityEngine;

public class ShootAction : AIAction {
	private readonly Enemy ctx;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return ctx.blackboard.target ? 0.6f : 0;
	}

	public override void Enter() {
		ctx.locomotion.Stop();
		ctx.locomotion.ADS(true);
	}
	float t;
	public override void Tick() {
		ctx.locomotion.Focus(ctx.blackboard.target.rig.head.position);
		ctx.blackboard.weapon.FireOnce();
		t += Time.deltaTime;
	}
	public override void Exit() { 
		ctx.locomotion.ADS(false);
	}
}
