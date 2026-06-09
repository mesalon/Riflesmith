using UnityEngine;
using UnityEngine.Events;

public class GenericButton : MonoBehaviour {
	[SerializeField] UnityEvent action;
	[SerializeField] bool once;
	private float startPos;
	private ConfigurableJoint joint;
	private bool activated;

	void Awake() {
		joint = GetComponent<ConfigurableJoint>();
		startPos = transform.position.y;
	}

	void FixedUpdate() {
		if (startPos - transform.position.y > joint.linearLimit.limit && !activated) {
			action.Invoke();
			activated = true;
		} else if (!once) {
			activated = false;
		}
	}
}
