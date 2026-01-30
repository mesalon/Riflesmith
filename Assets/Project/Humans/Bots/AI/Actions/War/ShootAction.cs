using UnityEngine;

public class ShootAction : AIAction {
	private readonly Bot ctx;
	private readonly AIBrain brain;
	private bool combatRoutine;
	private bool isPeeking;
	float t;

	public ShootAction(Bot ctx) {
		this.ctx = ctx;
		brain = ctx.brain;
	}

	public override float GetScore() {
		return (brain.targetLKP == null ? 0 : 0.8f) * 1 - brain.LKPAge * 0.05f;
	}

	public override void Enter() {
		ctx.motionController.Stop();
		brain.cover = new(ctx.transform.position, brain.targetLKP.Value, null, CoverParams.Default);
	}

	public override void Tick() {
		if (brain.cover.TryGetCover(out CoverTask cover)) {
			if (!combatRoutine) {
				ctx.motionController.Move(cover.point.position, Pace.Jog);
				if ((cover.point.position - ctx.transform.position).sqrMagnitude < 0.01f) {
					combatRoutine = true;
					t = 3;
				}
			} else {
				if (isPeeking) { ctx.handling.FireAt(brain.targetLKP.Value); }
				if (t >= 4) {
					if (isPeeking) {
						ctx.motionController.Move(cover.point.position, Pace.Walk);
						brain.expectsToSeeTarget = false;
						isPeeking = false;
					} else {
						ctx.motionController.Move(cover.ReturnFirePoint, Pace.Walk);
						brain.expectsToSeeTarget = true;
						isPeeking = true;
					}
					t = 0;
				}
				t += Time.deltaTime;
			}
		} else {
			brain.cover.Search();
		}
	}

	public override void Exit() {
		ctx.handling.ADS(false);
	}
}
