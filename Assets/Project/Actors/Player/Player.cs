using UnityEngine;

public class Player : MonoBehaviour {
	 public CharacterController cc;
	[SerializeField] Transform lookSource;
	 Rig rig;


	public void Awake() {
		rig = new(this);
	}
	private void OnEnable() { Application.onBeforeRender += rig.UpdateHead; }
	private void OnDisable() { Application.onBeforeRender -= rig.UpdateHead; }

	private void Update() {
	}

	void UpdateHead() {}
}
