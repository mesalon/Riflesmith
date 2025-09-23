public class DoNothingTask : Node {
    private Enemy ctx;

    public DoNothingTask(Enemy ctx) {
        this.ctx = ctx;
    }

    private float t;
    public override NodeState Evaluate(out Node active) {
        active = this;
        return NodeState.Running;
    }
}
