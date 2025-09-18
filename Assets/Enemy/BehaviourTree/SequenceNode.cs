using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : Node {
	public SequenceNode(List<Node> children) : base(children) { }
	private Node activeNode;

	public override NodeState Evaluate(out Node active) {
		bool anyChildIsRunning = false;
		active = null;
		foreach (Node node in children) {
			switch (node.Evaluate(out Node activeChild)) {
				case NodeState.Failure:
					return NodeState.Failure;
				case NodeState.Success:
					active = activeChild;
					continue;
				case NodeState.Running:
					active = activeChild;
					anyChildIsRunning = true;
					continue;
				default:
					return NodeState.Success;
			}
		}
		return anyChildIsRunning ? NodeState.Running : NodeState.Success;
	}

	public override void DrawGizmos() {
		activeNode?.DrawGizmos();
	}
}