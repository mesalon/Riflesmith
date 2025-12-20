using UnityEngine;

public class FindCover : Node {
	private Blackboard ctx;
	private CoverQuery query;

	public FindCover(Enemy self) { 
		ctx = self.blackboard;
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		//if (query == null) query = new CoverQuery(ctx.transform.position, ctx.target.transform.position, ctx.target.rig.head.position.y, ctx.cfg.cover);
		query.FindCover();
		if (query.GetBestPoint(1, 1, out Vector3 cover)) {
			ctx.cover = cover;
			return NodeState.Success;
		}
		return NodeState.Running;
	}
}
