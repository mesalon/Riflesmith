using UnityEngine;

public class ShootAction : AIAction {
	private readonly Blackboard ctx;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx.blackboard;
	}

	public override float GetScore() {
		return ctx.target ? 0.5f : 0;
	}

	public override void Execute() {
		Debug.Log("Shooting");
	}
}
