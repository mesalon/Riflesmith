using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LiteJointPlayer : MonoBehaviour, IVRAnchorProvider  {
	private Rigidbody rb;
	DeviceInput Input => VRPlayer.Input.head;
	[SerializeField] ConfigurableJoint headJoint;
	[SerializeField] Transform head;
	[SerializeField] float legForce, legDampingRatio, turnSpeed;
	[SerializeField] List<Renderer> toHide;
	[SerializeField] float assDrag;
	public Quaternion playerRotation;
	private VRInput lastInput;
	private Vector3 lastHeadVel;

	public Pose Anchor => new(head.position, playerRotation);

	void OnEnable() {
		RenderPipelineManager.beginCameraRendering += HandlePreCull;
		RenderPipelineManager.endCameraRendering += HandlePostRender;
	}
	void OnDisable() {
		RenderPipelineManager.beginCameraRendering -= HandlePreCull;
		RenderPipelineManager.endCameraRendering -= HandlePostRender;
	}
	void HandlePreCull(ScriptableRenderContext ctx, Camera cam) { if (cam == VRPlayer.camera) SetHidden(false); }
	void HandlePostRender(ScriptableRenderContext ctx, Camera cam) { if (cam == VRPlayer.camera) SetHidden(true); }
	void SetHidden(bool visible) { foreach (var r in toHide) r.enabled = visible; }

	void Awake() {
		rb = GetComponent<Rigidbody>();
		playerRotation = Quaternion.identity;
		VRPlayer.anchorProvider = this;
	}


	void Update() {
		playerRotation *= Quaternion.Euler(0, VRPlayer.Input.RHand.stick.x.Deadzone(0.1f) * turnSpeed, 0) * rb.rotation;
	}

	void FixedUpdate() {
		headJoint.targetPosition = new(0, Input.position.y, 0);
		headJoint.targetRotation = Input.rotation;

		Vector3 headVel = (Input.position - lastInput.head.position) / Time.fixedDeltaTime;
		Vector3 headAcceleration = (headVel - lastHeadVel).FlattenY() / Time.fixedDeltaTime;
		Vector3 friction = Ext.CritDamp(legForce, legDampingRatio) * (headVel - rb.linearVelocity).FlattenY();

		Quaternion forward = Quaternion.LookRotation(Vector3.ProjectOnPlane(playerRotation * Input.rotation * transform.forward, Vector3.up));
		Vector3 movement = forward * Vector3.ClampMagnitude(new Vector3(VRPlayer.Input.LHand.stick.x, 0, VRPlayer.Input.LHand.stick.y), 1) * legForce;

		rb.AddForce((movement + headAcceleration + friction) * rb.mass);

		lastHeadVel = headVel;
		lastInput = VRPlayer.Input;
	}
}
