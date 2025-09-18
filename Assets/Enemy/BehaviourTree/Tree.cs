using UnityEngine;

public abstract class Tree : MonoBehaviour {
	private Node root;
	protected void Start() {
		root = SetupTree();
	}

	protected void Update() {
		if (root != null) {
			root.Evaluate(out Node active);
		}
	}
	
	protected void OnDrawGizmos() {
		root?.DrawGizmos();
	}
	
	protected abstract Node SetupTree();
}
