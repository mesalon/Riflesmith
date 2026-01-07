public class SearchAction : AIAction {
	private readonly Enemy ctx;

	public SearchAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return ctx.targetLKP != null ? 0.55f : 0;
	}

	public override void Enter() {
		ctx.motionController.Move(ctx.targetLKP.Value, Pace.Jog);
	}
	public override void Tick() {
	}
	public override void Exit() { }
}

