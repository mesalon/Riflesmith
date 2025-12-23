using UnityEngine;

public class MoveUpAction : AIAction {
	private readonly Enemy ctx;
	private CoverQuery query;
	private Vector3 cover;
	private bool inCover;
	private bool peeking;
	float t;

	public MoveUpAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return (ctx.blackboard.targetLKP == null ? 0 : 1) * ctx.blackboard.confidence;
	}

	public override void Enter() {
		ctx.locomotion.Stop();
		query = new(ctx.transform.position, ctx.blackboard.targetLKP.Value, ctx.blackboard.seeker, CoverParams.Default);
	}

	public override void Tick() {
		if (inCover) {
			if (peeking) {
				ctx.handling.FireAt(ctx.blackboard.targetLKP.Value);
				if (t >= 4) {
					t = 0;
					peeking = false;
					ctx.locomotion.Move(cover, Pace.Walk);
				}
			} else {
				if (t >= 4) {
					ctx.locomotion.Move(cover + ctx.transform.rotation * Vector3.right * 0.4f, Pace.Walk);
					peeking = true;
					t = 0;
				}
			}
			t += Time.deltaTime;
		} else {
			if (query != null) {
				query.FindCover();
				if (query.GetBestPoint(1, 1, out cover)) {
					ctx.locomotion.Move(cover, Pace.Run);
					query = null;
				}
			} else if (ctx.locomotion.Arrived) { 
				inCover = true; 
				t = 3;
			}
		}
	}

	public override void Exit() {
		ctx.handling.ADS(false);
	}
}

