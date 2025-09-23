using System.Collections.Generic;

public class SelectorNode : Node {
	public SelectorNode(List<Node> children) : base(children) { }
	private Node activeNode;
	
	public override NodeState Evaluate(out Node active) {
		active = null;
		foreach (Node node in children) {
			switch (node.Evaluate(out Node activeChild)) {
				case NodeState.Failure:
					continue;
				case NodeState.Success:
					active = activeChild;
					return NodeState.Success;
				case NodeState.Running:
					active = activeChild;
					return NodeState.Running;
				default:
					continue;
			}
		}
		activeNode = active;
		return NodeState.Failure;
	}

	public override void DrawGizmos() {
		activeNode?.DrawGizmos();
	}
}