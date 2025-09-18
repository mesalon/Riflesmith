public class DoNothingTask : Node {
    private EnemyAI ctx;

    public DoNothingTask(EnemyAI ctx) {
        this.ctx = ctx;
    }

    private float t;
    public override NodeState Evaluate(out Node active) {
        active = this;
        return NodeState.Running;
    }
}