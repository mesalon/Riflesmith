using UnityEngine;

public class Bolt : BasicPart {
	[SerializeField] Chamber chamber;
	[SerializeField] FireControlGroup fcg;
	[SerializeField] AmmoSource ammo;
	[SerializeField] ConfigurableJoint joint;
	[SerializeField] Transform roundT;
	[SerializeField] Transform rearT, stripPointT, ejectPointT;
	[SerializeField] Vector3 ejectorDirection;
	[SerializeField] float ejectorForce;
	private Rigidbody rb;
	private Cartridge cartridge;
	private float stripPoint, ejectPoint, rearPoint, forePoint;
	private float state;
	private bool isManaged;

	public override void OnAssemble(Receiver receiver) {
		ammo = receiver.Find<AmmoSource>();
		chamber = receiver.Find<Chamber>();
		fcg = receiver.Find<FireControlGroup>();
		joint = GetComponent<ConfigurableJoint>();
		joint.connectedBody = receiver.GetComponent<Rigidbody>();
	}

	new void Awake() {
		base.Awake();
		forePoint = transform.localPosition.z;
		rearPoint = rearT.localPosition.z;
		stripPoint = stripPointT.localPosition.z;
		ejectPoint = ejectPointT.localPosition.z;
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate() {
		if (isManaged) {
			// state sets the physics todo
			joint.zMotion = ConfigurableJointMotion.Locked;
		} else {
			// physics sets the state
			SetInternal(transform.localPosition.z);
		}
	}

	void Update() {
		if (cartridge.data) cartridge.Render(roundT.localToWorldMatrix);
		Ext.Label(roundT.position, cartridge.data ? $"Bolt: {cartridge.data}" : "");
	}

	public void Set(float value) {
		isManaged = true;
		SetInternal(value);
	}

	private void SetInternal(float valueUnclamped) {
		float value = Mathf.Clamp(valueUnclamped, rearPoint, forePoint);
		if (value.DidReach(forePoint, state)) {
			print($"Seat");
			if (!chamber.cartridge.data) {
				chamber.cartridge = cartridge;
				cartridge = default;
			}
		}
		if (value.DidPassBack(forePoint, state)) {
			print("Extract");
			if (chamber.cartridge.data) {
				cartridge = chamber.cartridge;
				chamber.cartridge = default;
			}
		}

		if (value.DidReach(stripPoint, state)) {
			print($"Strip");
			if (ammo != null && !cartridge.data) cartridge = ammo.Strip();
		}

		if (value.DidReachBack(ejectPoint, state)) {
			print($"Eject");
			fcg.hammerState = true;
			if (cartridge.data) {
				CartridgeObject cartridgeObject = Instantiate(cartridge.data.visual, roundT.position, roundT.rotation);
				cartridgeObject.data = cartridge;
				float speed = rb.linearVelocity.magnitude;
				cartridgeObject.rb.AddForce(rb.rotation * ejectorDirection.normalized * ejectorForce * speed);
				cartridge = default;
			}
		}
		state = value;
	}
}
