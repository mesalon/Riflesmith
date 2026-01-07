using UnityEngine;

public class Player : Actor {
	private Rig rig;
	public Controls controls;

	private void Awake() {
		controls = new();
		rig = new(this);
	}

	private void OnEnable() {
		controls.Enable();
		Application.onBeforeRender += rig.UpdateHead;
	}

	private void OnDisable() {
		controls.Disable();
		Application.onBeforeRender -= rig.UpdateHead;
	}

	private void Update() {
		locomotion.Move(rig.LHand.Stick, 5);
	}

	public override void Damage(float amount) {
		health = Mathf.Max(0, health - amount);
	}
}
