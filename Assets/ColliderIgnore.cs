using UnityEngine;

public class ColliderIgnore : MonoBehaviour {
	[SerializeField] Collider self;
	[SerializeField] Collider toIgnore;

	private void Awake() {
		Physics.IgnoreCollision(self, toIgnore);
	}
}
