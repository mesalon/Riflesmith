using UnityEngine;

public class MoveUpAction : AIAction {
	private readonly Bot ctx;
	private readonly AIBrain brain;
	private Vector3 advance;

	public MoveUpAction(Bot ctx) {
		this.ctx = ctx;
		brain = ctx.brain;
	}

	public override float GetScore() {
		return (brain.targetLKP == null ? 0 : 1) * brain.confidence;
	}

	public override void Enter() {
		ctx.motion.Stop();
		advance = ctx.transform.position + (brain.targetLKP.Value - ctx.transform.position).normalized * 5;
		CoverParams cfg = CoverParams.Default;
		ctx.handling.ADS(true);
		cfg.urgency = 0;
		cfg.posBias = advance;
		brain.cover = new(ctx.transform.position, brain.targetLKP.Value, null, cfg);
	}

	public override void Tick() {
		Ext.DrawCube(advance, Quaternion.identity, Vector3.one * 0.25f, Color.gold);
		if (brain.cover.TryGetCover(out CoverTask cover)) {
			ctx.motion.Move(cover.point.position, Pace.Jog);
			ctx.handling.FireAt(brain.targetLKP.Value);

			if ((cover.point.position - ctx.transform.position).sqrMagnitude < 0.01f) { brain.confidence -= 0.5f; }
		} else {
			brain.cover.Search();
		}
	}

	public override void Exit() { }
}

