using UnityEngine;

public class ShootAction : AIAction {
	private Blackboard board => ctx.board;
	private readonly Enemy ctx;
	private bool combatRoutine;
	private bool isPeeking;
	float t;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return (board.targetLKP == null ? 0 : 0.8f) * 1 - board.LKPAge * 0.05f;
	}

	public override void Enter() {
		ctx.locomotion.Stop();
		board.cover = new(ctx.transform.position, board.targetLKP.Value, board.seeker, CoverParams.Default);
	}

	public override void Tick() {
		if (board.cover.TryGetCover(out CoverTask cover)) {
			if (!combatRoutine) {
				ctx.locomotion.Move(cover.point.position, Pace.Jog);
				if ((cover.point.position - ctx.transform.position).sqrMagnitude < 0.01f) {
					combatRoutine = true;
					t = 3;
				}
			} else {
				if (isPeeking) { ctx.handling.FireAt(board.targetLKP.Value); }
				if (t >= 4) {
					if (isPeeking) {
						ctx.locomotion.Move(cover.point.position, Pace.Walk);
						board.expectsToSeeTarget = false;
						isPeeking = false;
					} else {
						ctx.locomotion.Move(cover.ReturnFirePoint, Pace.Walk);
						board.expectsToSeeTarget = true;
						isPeeking = true;
					}
					t = 0;
				}
				t += Time.deltaTime;
			}
		} else {
			board.cover.Search();
		}
	}

	public override void Exit() {
		ctx.handling.ADS(false);
	}
}
