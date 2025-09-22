public class IsPlayerVisible : Node {
	private Enemy self;
	private Blackboard ctx;
	public IsPlayerVisible(Enemy self) {
		this.self = self;
		ctx = self.blackboard;
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		return ctx.target ? NodeState.Success : NodeState.Failure;
	}
}
