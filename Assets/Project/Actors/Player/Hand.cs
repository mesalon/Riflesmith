using UnityEngine.InputSystem;
using UnityEngine;

public class Hand : Interactor {
	public float Grip => gripAction.ReadValue<float>();
	public float Trigger => triggerAction.ReadValue<float>();
	public float NearButton => nearButtonAction.ReadValue<float>();
	public float FarButton => farButtonAction.ReadValue<float>();
	public Vector2 Stick => stickAction.ReadValue<Vector2>();
	public InputAction gripAction;
	public InputAction triggerAction;
	public InputAction nearButtonAction;
	public InputAction farButtonAction;
	public InputAction stickAction;
	[SerializeField] GameObject vis;
	[SerializeField] Transform[] bones;
	[SerializeField] HandPose idlePose, grippingPose;
	[SerializeField] bool hideOnGrab;
	private Quaternion[] poseBuffer;

	/* todo: needed?
	private void OnEnable() {
		gripAction.performed += _ => GripHand();
		gripAction.canceled += _ => Drop();
	}

	private void OnDisable() {
		gripAction.performed -= _ => GripHand();
		gripAction.canceled -= _ => Drop();
	}
	*/

	public void GripHand() {
		Collider[] overlap = Physics.OverlapSphere(holdPoint.position, 0.01f);
		foreach (Collider col in overlap) {
			if (held == null && col.TryGetComponent(out IInteractable interactable)) { Pick(interactable); }
		}
	}

	public override void Update() {
		base.Update();
		HandPose.Lerp(idlePose, grippingPose, Grip, poseBuffer);
		for (int i = 0; i < bones.Length; i++) { bones[i].localRotation = poseBuffer[i]; }
	}

	public override void Pick(IInteractable interactable) {
		base.Pick(interactable);
		vis.SetActive(!hideOnGrab);
	}

	public override void Drop() {
		base.Drop();
		vis.SetActive(true);
	}
}

