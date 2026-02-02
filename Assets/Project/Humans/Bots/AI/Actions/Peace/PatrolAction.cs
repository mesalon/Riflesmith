using UnityEngine;

public class PatrolAction : AIAction {
	private readonly Bot ctx;
	private Vector3[] patrol;
	private int current;

	public PatrolAction(Bot ctx) {
		this.ctx = ctx;
		patrol = PatrolGenerator.I.GetPatrolPath(ctx.transform.position);
	}

	public override float GetScore() {
		return 0.0f;
	}

	public override void Enter() {
		ctx.motionController.Move(patrol[current], Pace.Walk);
	}
	public override void Tick() {
		//Ext.DrawPath(patrol);
		if ((patrol[current] - ctx.transform.position).sqrMagnitude < 4) {
			current++; 
			ctx.motionController.Move(patrol[current], Pace.Walk);
		}
	}
	public override void Exit() { }
}
