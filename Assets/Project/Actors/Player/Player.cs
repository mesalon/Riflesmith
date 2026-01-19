using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] Human self;
	private Rig rig;

	private void OnEnable() {
		Application.onBeforeRender += UpdateHead;
	}

	private void OnDisable() {
		Application.onBeforeRender -= UpdateHead;
	}

	public void Awake() {
	}

	private void Update() {
	}

	public void UpdateHead() {
	}
}
