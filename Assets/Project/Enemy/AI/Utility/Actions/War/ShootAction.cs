using UnityEngine;

public class ShootAction : AIAction {
	private readonly Enemy ctx;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return ctx.blackboard.target ? 0.6f : 0;
	}

	public override void Execute() {
		ctx.locomotion.Stop();
	}
}
