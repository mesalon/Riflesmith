using UnityEngine;

public class PatrolAction : AIAction {
	private readonly Blackboard ctx;
	private readonly EnemyLocomotion locomotion;
	private Vector3[] patrol;
	private int current;

	public PatrolAction(Enemy ctx) {
		this.ctx = ctx.blackboard;
		locomotion = ctx.locomotion;
		patrol = PatrolGenerator.I.GetPatrolPath(ctx.transform.position);
		locomotion.Move(patrol[current], this.ctx.cfg.locomotion.walkSpeed);
	}

	public override float GetScore() {
		return 0.5f;
	}

	public override void Execute() {
		Ext.DrawPath(patrol);
		if ((patrol[current] - ctx.transform.position).sqrMagnitude < 4) {
			current++; 
			locomotion.Move(patrol[current], ctx.cfg.locomotion.walkSpeed);
		}
		Debug.DrawRay(ctx.transform.position, patrol[current] - ctx.transform.position, Color.red); 
	}
}
