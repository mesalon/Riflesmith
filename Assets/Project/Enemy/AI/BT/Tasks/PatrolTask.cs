using UnityEngine;

public class PatrolTask : Node {
	private readonly Blackboard ctx;
	private readonly EnemyLocomotion locomotion;
	private Vector3[] patrol;
	private int point;

	public PatrolTask(Enemy ctx) {
		this.ctx = ctx.blackboard;
		locomotion = ctx.locomotion;
		patrol = PatrolGenerator.I.GetPatrolPath(ctx.transform.position);
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		Debug.Log($"moving: {patrol[point]}");
		Ext.DrawPath(patrol);
		Ext.Label(patrol[point], "Alleged destination");
		if (locomotion.Move(patrol[point], ctx.cfg.locomotion.walkSpeed)) { point++; }
		return NodeState.Running;
	}
}
