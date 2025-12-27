using UnityEngine;

public class MoveUpAction : AIAction {
	private Blackboard board => ctx.board;
	private readonly Enemy ctx;
	private Vector3 advance;

	public MoveUpAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return (board.targetLKP == null ? 0 : 1) * board.confidence;
	}

	public override void Enter() {
		ctx.locomotion.Stop();
		advance = ctx.transform.position + (board.targetLKP.Value - ctx.transform.position).normalized * 5;
		CoverParams cfg = CoverParams.Default;
		ctx.handling.ADS(true);
		cfg.urgency = 0;
		cfg.posBias = advance;
		board.cover = new(ctx.transform.position, board.targetLKP.Value, board.seeker, cfg);
	}

	public override void Tick() {
		Ext.DrawCube(advance, Quaternion.identity, Vector3.one * 0.25f, Color.gold);
		if (board.cover.TryGetCover(out CoverTask cover)) {
			ctx.locomotion.Move(cover.point.position, Pace.Jog);
			ctx.handling.FireAt(board.targetLKP.Value);

			if ((cover.point.position - ctx.transform.position).sqrMagnitude < 0.01f) { board.confidence -= 0.5f; }
		} else {
			board.cover.Search();
		}
	}

	public override void Exit() { }
}

