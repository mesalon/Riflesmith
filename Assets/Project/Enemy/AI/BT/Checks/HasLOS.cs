using UnityEngine;

public class HasLOS : Node {
    private Blackboard ctx;

    public HasLOS(Enemy ctx) {
        this.ctx = ctx.blackboard;
    }

    private float t;
    public override NodeState Evaluate(out Node active) {
        active = this;
				return ctx.target && Physics.Raycast(ctx.eyes.position, ctx.target.rig.head.position, out var _) ? NodeState.Success : NodeState.Failure;
    }
}
