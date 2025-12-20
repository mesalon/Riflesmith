using UnityEngine;

public class ShootAction : AIAction {
	private readonly Enemy ctx;
	private CoverQuery query;
	private Vector3 cover;

	public ShootAction(Enemy ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return ctx.blackboard.targetLKP != null && ctx.blackboard.seenTime < 10 ? 0.6f : 0;
	}

	public override void Enter() {
		ctx.locomotion.Stop();
		query = new(ctx.transform.position, ctx.blackboard.targetLKP.Value, ctx.blackboard.seeker, CoverParams.Default);
	}

	public override void Tick() {
		if (query != null) {
			query.FindCover();
			if (query.GetBestPoint(1, 1, out cover)) {
				ctx.locomotion.Move(cover, Pace.Run);
				query = null;
			}
		} else {
			if (ctx.locomotion.Arrived) {
				// Fight from cover
				Debug.Log("Fighting!");
				ctx.handling.FireAt(ctx.blackboard.targetLKP.Value);
			}
		}
	}
	public override void Exit() {
	}
}
