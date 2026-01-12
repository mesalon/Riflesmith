public class SearchAction : AIAction {
	private readonly Enemy ctx;
	private readonly EnemyBrain brain;

	public SearchAction(Enemy ctx) {
		this.ctx = ctx;
		brain = ctx.brain;
	}

	public override float GetScore() {
		return brain.targetLKP != null ? 0.55f : 0;
	}

	public override void Enter() {
		ctx.motionController.Move(brain.targetLKP.Value, Pace.Jog);
	}
	public override void Tick() {
	}
	public override void Exit() { }
}

