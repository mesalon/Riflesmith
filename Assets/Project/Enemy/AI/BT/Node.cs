using System.Collections.Generic;

public enum NodeState { Running, Success, Failure }
public class Node {
	public Node parent;
	public List<Node> children = new();
	
	public Node() { }
	
	public Node(List<Node> children) {
		foreach(Node node in children) { Attach(node); }
	}
	
	public virtual NodeState Evaluate(out Node active) {
		active = null; 
		return NodeState.Failure; 
	}
	
	public void Attach(Node node) {
		node.parent = this;
		children.Add(node);
	}

	public virtual void DrawGizmos() { }
}
