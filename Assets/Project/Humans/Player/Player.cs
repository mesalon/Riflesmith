using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;


public class Player : MonoBehaviour {
	public CharacterController cc;
	[SerializeField] Transform head;
	[SerializeField] Hand LHand, RHand;

	private Controls controls;
	void Start() {
		controls = new();
		//controls.Player.Enable();
	}

	public void OnEnable() {
		Application.onBeforeRender += UpdateHead;
		controls ??= new Controls();
		controls.Enable();
	}

	public void OnDisable() {
		Application.onBeforeRender -= UpdateHead;
		controls.Disable();
	}

	void Update() {
		print(controls.LHand.Position.ReadValue<Vector3>());
		print(controls.LHand.Rotation.ReadValue<Vector3>());
		print(controls.LHand.Grip.ReadValue<float>());
		print(controls.LHand.Trigger.ReadValue<float>());
		print(controls.LHand.PrimaryButton.ReadValue<bool>());
		print(controls.LHand.SecondaryButton.ReadValue<bool>());
		LHand.transform.SetPose(new(), true);
		RHand.transform.SetPose(new(), true);

		// Recenter
		if (Input.GetKeyDown(KeyCode.Space)) {
			Vector3 adjustment = new Vector3(head.position.x, 0, head.position.z) -
				new Vector3(transform.position.x, 0, transform.position.z);
			transform.position += adjustment;
			head.localPosition -= transform.InverseTransformVector(adjustment);
			LHand.transform.localPosition -= transform.InverseTransformVector(adjustment);
			RHand.transform.localPosition -= transform.InverseTransformVector(adjustment);
		}
	}

	void FixedUpdate() {
		cc.height = head.localPosition.y;
		cc.center = new(head.localPosition.x, cc.height / 2, head.localPosition.z);
	}

	[BeforeRenderOrder(-30000)]
	public void UpdateHead() {
		head.localPosition = default;
		head.localRotation = default;
	}
}
