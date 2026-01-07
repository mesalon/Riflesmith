using UnityEngine;

public class MoveUpAction : AIAction {
	private readonly Enemy ctx;
	private Vector3 advance;

	public MoveUpAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return (ctx.targetLKP == null ? 0 : 1) * ctx.confidence;
	}

	public override void Enter() {
		ctx.motionController.Stop();
		advance = ctx.transform.position + (ctx.targetLKP.Value - ctx.transform.position).normalized * 5;
		CoverParams cfg = CoverParams.Default;
		ctx.handling.ADS(true);
		cfg.urgency = 0;
		cfg.posBias = advance;
		ctx.cover = new(ctx.transform.position, ctx.targetLKP.Value, ctx.seeker, cfg);
	}

	public override void Tick() {
		Ext.DrawCube(advance, Quaternion.identity, Vector3.one * 0.25f, Color.gold);
		if (ctx.cover.TryGetCover(out CoverTask cover)) {
			ctx.motionController.Move(cover.point.position, Pace.Jog);
			ctx.handling.FireAt(ctx.targetLKP.Value);

			if ((cover.point.position - ctx.transform.position).sqrMagnitude < 0.01f) { ctx.confidence -= 0.5f; }
		} else {
			ctx.cover.Search();
		}
	}

	public override void Exit() { }
}

