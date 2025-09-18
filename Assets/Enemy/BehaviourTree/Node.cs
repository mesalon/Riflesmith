using System.Collections.Generic;
using UnityEngine;

public enum NodeState {
	Running, Success, Failure
}
public class Node {
	public Node parent;
	public List<Node> children = new();
	protected Tree ownerTree;
	
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
	
	public void SetOwnerTree(Tree tree) {
		ownerTree = tree;
		foreach (Node child in children) {
			child.SetOwnerTree(tree);
		}
	}

	public virtual void DrawGizmos() { }
}