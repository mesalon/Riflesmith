using UnityEngine;

public class PatrolTask : Node {
	private readonly Blackboard ctx;
	private readonly EnemyLocomotion locomotion;
	private Vector3[] patrol;
	private int current;

	public PatrolTask(Enemy ctx) {
		this.ctx = ctx.blackboard;
		locomotion = ctx.locomotion;
		patrol = PatrolGenerator.I.GetPatrolPath(ctx.transform.position);
		locomotion.Move(patrol[current], this.ctx.cfg.locomotion.walkSpeed);
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		//for (int i = 0; i < patrol.Length; i++) { Ext.Label(patrol[i] + Vector3.up * 0.5f, $"Point {i}"); }
		Ext.DrawPath(patrol);
		if ((patrol[current] - ctx.transform.position).sqrMagnitude < 4) {
			Debug.Log($"Bump to {current}");
			current++; 
			locomotion.Move(patrol[current], ctx.cfg.locomotion.walkSpeed);
		}
		Debug.DrawRay(ctx.transform.position, patrol[current] - ctx.transform.position, Color.red); 
		Debug.Log($"{(patrol[current] - ctx.transform.position).sqrMagnitude}");
		return NodeState.Running;
	}
}
