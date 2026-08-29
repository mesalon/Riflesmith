using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
// todo: add easing to the player's leg springs
// todo: reverse order so that springing a jump prevents you from running, and not the other way around
// todo: don't make jump min a binary floor. Just smoothly affect the jump ability based on your head height

public class Player : Human, IVRAnchorProvider {
	private DeviceInput Input => VRPlayer.Input.head;
	public CharacterController cc;
	[SerializeField] Transform eyes;
	[SerializeField] Transform eyesForward;
	[SerializeField] Transform debugSkeleton;
	[SerializeField] LayerMask groundMask;
	[SerializeField] Rigidbody rb;
	[SerializeField] Renderer[] toHide;
	[SerializeField] CapsuleCollider bodyCollider;
	[SerializeField] float speed, runSpeed;
	[SerializeField] float turnSpeed, turnDeadzone;
	[SerializeField] float bodyOffset;
	[SerializeField] float maxLegLength, legForce, legDamper, legResistance;
	[SerializeField] float jumpForce, sprintJumpForce, jumpSpeed, jumpBrace, jumpInputMin, jumpInputMax, jumpHeightMin, sprintJumpSpeed, sprintJumpRecovery;
	[SerializeField] bool drawGizmos;
	[SerializeField] float footRadius;
	[SerializeField] List<Transform> targetBones;
	[SerializeField] List<Transform> actualBones;
	private float jumpInputAccumulation;
	private float sprintJumpT;
	private bool isSprintJumping;

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

	public Pose Anchor => new(eyes.position, transform.rotation);

	void Awake() {
		VRPlayer.anchorProvider = this;
	}

	void Update() {
		if (drawGizmos) Ext.DrawSkeleton(debugSkeleton, Color.white);

		Quaternion rot = Input.rotation * Quaternion.Inverse(eyesForward.localRotation);
		eyes.SetPose(new(0, Input.position.y, 0), rot, Space.Self);
	}

	void FixedUpdate() {
		//Vector3 headDelta = transform.rotation * (Input.position - VRPlayer.LastInput.head.position);
		//rb.MovePosition(rb.position + headDelta.FlattenY());
		rb.MoveRotation(Quaternion.Euler(0, VRPlayer.Input.RHand.stick.x.Deadzone(turnDeadzone) * turnSpeed, 0) * rb.rotation);

		bodyCollider.height = eyes.localPosition.y - bodyOffset;
		float midpoint = (eyes.localPosition.y + bodyOffset) / 2;
		bodyCollider.center = new(0, Mathf.Min(eyes.localPosition.y, midpoint), 0);
		bodyCollider.radius = footRadius * 1.5f;

		for (int i = 0; i < actualBones.Count; i++) {
			actualBones[i].localPosition = targetBones[i].localPosition; 
			actualBones[i].localRotation = targetBones[i].localRotation;
		}

		float length = Mathf.Clamp(eyes.localPosition.y, 0, maxLegLength) - footRadius;
		l = length;
		float physicalAccumulation = 0;
		if (Physics.SphereCast(eyes.position, footRadius, Vector3.down, out RaycastHit hit, length, groundMask)) {
			Debug.DrawRay(eyes.position, Vector3.down * hit.distance, Color.red);

			Quaternion forward = Quaternion.LookRotation(Vector3.ProjectOnPlane(eyesForward.rotation * Vector3.forward, Vector3.up));
			Vector3 input = forward * Vector3.ClampMagnitude(new Vector3(VRPlayer.Input.LHand.stick.x, 0, VRPlayer.Input.LHand.stick.y), 1);
			rb.AddForce(input * (VRPlayer.Input.LHand.stickButton ? runSpeed : speed), ForceMode.Acceleration);

			float jumpCrouch = jumpInputAccumulation * jumpBrace;
			float adjustedLegForce = legForce * (length - hit.distance - jumpCrouch) - legDamper * rb.linearVelocity.y;
			rb.AddForce(Vector3.up * Mathf.Max(0, adjustedLegForce), ForceMode.Acceleration);
			rb.AddForce(-rb.linearVelocity.FlattenY() * legResistance, ForceMode.Acceleration);

			physicalAccumulation = Mathf.Min(jumpInputAccumulation, length - hit.distance); // How much have we actually crouched down?
			if (rb.linearVelocity.FlattenY().magnitude < sprintJumpSpeed) {
				float stick = Mathf.Max(0, -VRPlayer.Input.RHand.stick.y);
				float raw = Mathf.InverseLerp(jumpInputMin, jumpInputMax, stick >= jumpInputMin ? stick : 0); // Deadzoned
				if (eyes.localPosition.y > jumpHeightMin) // Can't jump while sitting down and shit
					jumpInputAccumulation = Mathf.Max(jumpInputAccumulation, raw);
			} else {
				ConsumeJump(physicalAccumulation); // No sprinting with a primed jump
				if (VRPlayer.Input.RHand.stick.y < -jumpInputMin && !isSprintJumping && sprintJumpT >= sprintJumpRecovery) {
					rb.AddForce(Vector3.up * sprintJumpForce, ForceMode.Acceleration);
					isSprintJumping = true;
					sprintJumpT = 0;
				}
			}
			sprintJumpT += Time.deltaTime;
		} else if (isSprintJumping) {
			isSprintJumping = false;
		}

		if (VRPlayer.Input.RHand.stick.y > -jumpInputMin) {
			rb.AddForce(Vector3.up * ConsumeJump(physicalAccumulation) * jumpForce, ForceMode.Acceleration);
		}
	}

	float l;
	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(eyes.position + l * Vector3.down, footRadius);
	}

	private float ConsumeJump(float amount) {
		float applied = Mathf.Min(amount, jumpSpeed * Time.fixedDeltaTime);
		jumpInputAccumulation -= applied;
		return applied;
	}
}
