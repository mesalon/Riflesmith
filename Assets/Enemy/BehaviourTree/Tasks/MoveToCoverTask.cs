using UnityEngine;

public class MoveToCoverTask : Node { 
	private Enemy self;
	private Blackboard ctx;
	public MoveToCoverTask(Enemy self) {
		this.self = self;
		ctx = self.blackboard;
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		self.locomotion.Move(ctx.cover!.Value, ctx.cfg.locomotion.sprintSpeed);
		return NodeState.Running;
	}
}
