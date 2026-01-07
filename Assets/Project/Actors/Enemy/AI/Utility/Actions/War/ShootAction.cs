using UnityEngine;

public class ShootAction : AIAction {
	private readonly Enemy ctx;
	private bool combatRoutine;
	private bool isPeeking;
	float t;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return (ctx.targetLKP == null ? 0 : 0.8f) * 1 - ctx.LKPAge * 0.05f;
	}

	public override void Enter() {
		ctx.motionController.Stop();
		ctx.cover = new(ctx.transform.position, ctx.targetLKP.Value, ctx.seeker, CoverParams.Default);
	}

	public override void Tick() {
		if (ctx.cover.TryGetCover(out CoverTask cover)) {
			if (!combatRoutine) {
				ctx.motionController.Move(cover.point.position, Pace.Jog);
				if ((cover.point.position - ctx.transform.position).sqrMagnitude < 0.01f) {
					combatRoutine = true;
					t = 3;
				}
			} else {
				if (isPeeking) { ctx.handling.FireAt(ctx.targetLKP.Value); }
				if (t >= 4) {
					if (isPeeking) {
						ctx.motionController.Move(cover.point.position, Pace.Walk);
						ctx.expectsToSeeTarget = false;
						isPeeking = false;
					} else {
						ctx.motionController.Move(cover.ReturnFirePoint, Pace.Walk);
						ctx.expectsToSeeTarget = true;
						isPeeking = true;
					}
					t = 0;
				}
				t += Time.deltaTime;
			}
		} else {
			ctx.cover.Search();
		}
	}

	public override void Exit() {
		ctx.handling.ADS(false);
	}
}
