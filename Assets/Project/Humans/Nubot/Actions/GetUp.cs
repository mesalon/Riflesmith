using UnityEngine;

public class GetUpAction : BotAction {
	private readonly Nubot ctx;
	public GetUpAction(Nubot ctx) => this.ctx = ctx;
	public override float Score() {
		return ctx.body.pm.isAlive ? 0 : 100;
	}
	public override void Enter() { }
	public override void Tick() { 
		if (ctx.body.strength >= ctx.body.getUpStrength) {
			Debug.Log("Got up");
			ctx.body.strength -= ctx.body.getUpStrength;
			ctx.body.pm.Resurrect();
		}
	}
	public override void Exit() { }
}
