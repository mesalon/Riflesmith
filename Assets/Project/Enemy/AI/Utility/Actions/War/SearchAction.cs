public class SearchAction : AIAction {
	private Blackboard board => ctx.board;
	private readonly Enemy ctx;

	public SearchAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return board.targetLKP != null ? 0.55f : 0;
	}

	public override void Enter() {
		ctx.locomotion.Move(board.targetLKP.Value, Pace.Jog);
	}
	public override void Tick() {
	}
	public override void Exit() { }
}

