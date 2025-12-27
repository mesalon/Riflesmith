using UnityEngine;

public class ShootAction : AIAction {
	private readonly Enemy ctx;
	private bool InCover {
		get {
			Vector3? cover = Cover;
			if (cover == null) return false;
			return (cover.Value - ctx.transform.position).sqrMagnitude < 1;
		}
	}
	private Vector3? Cover {
		get {
			if (ctx.blackboard.cover != null && ctx.blackboard.cover.GetBestPoint(1, 1, out CoverTask task)) {
				return task.cover.position;
			}
			return null;
		}
	}
	private bool peeking;
	float t;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return (ctx.blackboard.targetLKP == null ? 0 : 1) * 1 - ctx.blackboard.LKPAge * 0.05f;
	}

	public override void Enter() {
		ctx.locomotion.Stop();
		ctx.blackboard.cover = new(ctx.transform.position, ctx.blackboard.targetLKP.Value, ctx.blackboard.seeker, CoverParams.Default);
	}

	public override void Tick() {
		ctx.blackboard.cover?.ShowDebug();
		Vector3? cover = Cover;
		Ext.Label(ctx.transform.position, $"{(cover != null ? (cover.Value - ctx.transform.position).sqrMagnitude : "No cover")}");
		if (inCover) {
			if (peeking) {
				ctx.handling.FireAt(ctx.blackboard.targetLKP.Value);
				if (t >= 4) {
					t = 0;
					peeking = false;
					ctx.locomotion.Move(, Pace.Walk);
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
			if (ctx.blackboard.cover != null) {
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
