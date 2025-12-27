using UnityEngine;

public class PatrolAction : AIAction {
	private Blackboard board => ctx.board;
	private readonly Enemy ctx;
	private readonly EnemyLocomotion locomotion;
	private Vector3[] patrol;
	private int current;

	public PatrolAction(Enemy ctx) {
		this.ctx = ctx;
		locomotion = ctx.locomotion;
		patrol = PatrolGenerator.I.GetPatrolPath(ctx.transform.position);
	}

	public override float GetScore() {
		return 0.5f;
	}

	public override void Enter() {
		locomotion.Move(patrol[current], Pace.Walk);
	}
	public override void Tick() {
		//Ext.DrawPath(patrol);
		if ((patrol[current] - ctx.transform.position).sqrMagnitude < 4) {
			current++; 
			locomotion.Move(patrol[current], Pace.Walk);
		}
	}
	public override void Exit() { }
}
