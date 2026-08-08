// todo: hand stretching
using UnityEngine;

public class Hand : MonoBehaviour {
	public ConfigurableJoint joint => handMuscle.joint;
	public DeviceInput Input => (side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand).RelativeTo(HeadOffset);
	public DeviceInput LastInput => (side == Side.Left ? VRPlayer.LastInput.LHand : VRPlayer.LastInput.RHand).RelativeTo(HeadOffset);
	private Vector3 HeadOffset => VRPlayer.Input.head.position.FlattenY();

	[SerializeField] HandPoseObject neutralPose, gripPose, triggerPose;
	public Transform palm;
	public Rigidbody rb;
	public Hand other;
	public bool doToggleGrab;
	public float grabPosSpeed, grabRotSpeed;
	public JointDriveData heldDrive;
	[HideInInspector] public ConfigurableJoint grabJoint;
	public Transform grabPoint;
	public Side side;
	[SerializeField] bool drawGizmos;
	[SerializeField] Transform controllerVisual;
	[SerializeField] JointDriveData normalDrive, climbDrive, grabDrive;
	[SerializeField] LayerMask grabLayer;
	[SerializeField] Collider[] associatedColliders;
	[SerializeField] Muscle upperMuscle, lowerMuscle, handMuscle;
	[SerializeField] float reach;
	[SerializeField] float gripPoint;
	private JointDriveData currentDrive;
	public Interactable held;
	private bool gripState;

	void Awake() {
		currentDrive = normalDrive;
		upperMuscle.Init();
		lowerMuscle.Init();
		handMuscle.Init();
	}

	void Update() {
		if (drawGizmos) {
			Ext.DrawSkeleton(upperMuscle.joint.transform, Color.cyan);
			VRGizmos.Axis(transform.position, transform.rotation, 0.05f);
		}

		Vector3 pos = Input.position; // Keep the hand positions relative to the head until they have a real input
		if (VRPlayer.Input.head.gotFirstInput && !Input.gotFirstInput) { pos += VRPlayer.Input.head.position.FlattenY(); }
		transform.SetPose(Input.position, Input.rotation, Space.Self);
		controllerVisual.SetPose(pos, Input.rotation, Space.Self);

		if (held) held.OnHold();
	}


	private void Drop() {
		if (grabJoint) {
			Destroy(grabJoint);
			Ext.IgnoreCollisionsBetween(associatedColliders, grabJoint.transform.GetComponentsInChildren<Collider>(), false); // todo: this fucking sucks
			if (held) {
				held.OnDropped();
				held.hand = null;
				held = null;
			}
		}
	}

	void FixedUpdate() {
		upperMuscle.Drive(true);
		lowerMuscle.Drive();
		handMuscle.Drive();
		if (doToggleGrab) {
			if (Input.grip.DidPass(gripPoint, LastInput.grip)) { gripState = !gripState; }
		} else { gripState = Input.grip >= gripPoint; }
		if (gripState) {
			if (!grabJoint) {
				Collider[] overlap = Physics.OverlapSphere(palm.position, reach, grabLayer);

				Interactable interactable = null;
				Rigidbody objectRb = null;
				foreach (Collider col in overlap) {
					if (col.TryGetComponent(out interactable)) { 
						objectRb = interactable.rb; 
						break;
					}
					if (!objectRb && col.attachedRigidbody) { objectRb = col.attachedRigidbody; }
				}
				if (objectRb) {
					Ext.IgnoreCollisionsBetween(associatedColliders, objectRb.GetComponentsInChildren<Collider>(), true);
					grabJoint = objectRb.gameObject.AddComponent<ConfigurableJoint>();
					grabJoint.connectedBody = rb;
					grabJoint.autoConfigureConnectedAnchor = false;
					grabJoint.anchor = objectRb.transform.InverseTransformPoint(rb.position);
					grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = heldDrive;
					grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
					grabJoint.slerpDrive = heldDrive;
				}
				if (interactable && !interactable.hand) {
					//if (interactable.hand) { interactable.hand.Drop(); } todo: fix
					held = interactable;
					held.hand = this;
					held.OnPicked();
				}
			}
		} else {
			Drop();
		}
		if (held) held.OnHoldFixed();
	}
}

public enum Side { Left, Right }
[System.Serializable] public struct Muscle {
	public ConfigurableJoint joint;
	[SerializeField] Transform target;
	[SerializeField] Transform bone;
	[SerializeField] float dampingRatio;
	private Quaternion initRot;

	public void Init() => initRot = Quaternion.Inverse(joint.transform.localRotation);
	public void Drive(bool positionToo = false) {
		JointDrive drive = joint.slerpDrive;
		drive.positionDamper = Mathf.Sqrt(joint.slerpDrive.positionSpring) * dampingRatio;
		joint.slerpDrive = drive;
		if (positionToo) {
			joint.targetPosition = initRot * target.root.InverseTransformPoint(target.position);
			joint.targetRotation = initRot * Quaternion.Inverse(target.root.rotation) * target.rotation;
		} else {
			joint.targetRotation = initRot * target.localRotation;
		}
		bone.SetPose(joint.transform.position, joint.transform.rotation);
	}
}
