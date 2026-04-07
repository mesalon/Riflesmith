public class SearchAction : AIAction {
	private readonly Bot ctx;
	private readonly AIBrain brain;

	public SearchAction(Bot ctx) {
		this.ctx = ctx;
		brain = ctx.brain;
	}

	public override float GetScore() {
		return brain.targetLKP != null ? 0.55f : 0;
	}

	public override void Enter() {
		ctx.motion.Move(brain.targetLKP.Value, Pace.Jog);
	}
	public override void Tick() {
	}
	public override void Exit() { }
}

