public class HasAmmo : Node {
    private Blackboard ctx;

    public HasAmmo(Enemy ctx) {
        this.ctx = ctx.blackboard;
    }

    private float t;
    public override NodeState Evaluate(out Node active) {
        active = this;
				return ctx.weapon.rounds > 0 ? NodeState.Success : NodeState.Failure;
    }
}
