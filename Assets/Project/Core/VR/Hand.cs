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
	public bool lockit;
	public float grabPosSpeed, grabRotSpeed;
	[HideInInspector] public ConfigurableJoint grabJoint;
	[SerializeField] Side side;
	[SerializeField] Transform forward;
	[SerializeField] Transform controllerVisual;
	[SerializeField] JointDriveData normalDrive, climbDrive, grabDrive;
	[SerializeField] LayerMask grabLayer;
	[SerializeField] Transform[] bones;
	[SerializeField] Collider[] associatedColliders;
	[SerializeField] JointDriveData heldDrive;
	[SerializeField] Muscle upperMuscle, lowerMuscle, handMuscle;
	[SerializeField] float reach;
	[SerializeField] float gripPoint, gripLeniency;
	private float gripTime;
	private JointDriveData currentDrive;

	void Awake() {
		currentDrive = normalDrive;
		upperMuscle.Init();
		lowerMuscle.Init();
		handMuscle.Init();
	}

	void Update() {
		Ext.DrawSkeleton(upperMuscle.joint.transform, Color.cyan);
		VRGizmos.Axis(transform.position, transform.rotation, 0.05f);

		Vector3 pos = Input.position; // Keep the hand positions relative to the head until they have a real input
		if (VRPlayer.Input.head.gotFirstInput && !Input.gotFirstInput) { pos += VRPlayer.Input.head.position.FlattenY(); }
		transform.SetPose(Input.position, Input.rotation, Space.Self);
		controllerVisual.SetPose(pos, Input.rotation, Space.Self);
	}


	void FixedUpdate() {
		upperMuscle.Drive(true);
		lowerMuscle.Drive();
		handMuscle.Drive();
		
		if (Input.grip >= gripPoint) {
			if (!grabJoint) {
				gripTime += Time.deltaTime;
				if (gripTime < gripLeniency) { 
					Collider[] overlap = Physics.OverlapSphere(palm.position, reach, grabLayer);
					if (overlap.Length > 0) {
						foreach (Collider col in overlap) {
							if (col.transform.root.TryGetComponent(out Rigidbody objectRb)) {
								Ext.IgnoreCollisionsBetween(associatedColliders, objectRb.GetComponentsInChildren<Collider>(), true);
								grabJoint = objectRb.gameObject.AddComponent<ConfigurableJoint>();
								grabJoint.connectedBody = rb;
								grabJoint.autoConfigureConnectedAnchor = false;
								grabJoint.anchor = objectRb.transform.InverseTransformPoint(rb.position);
								grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = heldDrive;
								grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
								grabJoint.slerpDrive = heldDrive;
								break;
							}
						}
					}
				}
			} 		
		} else if (grabJoint && !lockit) {
			Ext.IgnoreCollisionsBetween(associatedColliders, grabJoint.transform.GetComponentsInChildren<Collider>(), false); // todo: this fucking sucks
			Destroy(grabJoint);
		} else { 
			gripTime = 0;
		}
	}
}

public enum Side { Left, Right }
[System.Serializable] public struct Muscle {
	public ConfigurableJoint joint;
	[SerializeField] Transform target;
	[SerializeField] Transform bone;
	private Quaternion initRot;

	public void Init() => initRot = Quaternion.Inverse(joint.transform.localRotation);
	public void Drive(bool positionToo = false) {
		if (positionToo) {
			joint.targetPosition = initRot * target.root.InverseTransformPoint(target.position);
			joint.targetRotation = initRot * Quaternion.Inverse(target.root.rotation) * target.rotation;
		} else {
			joint.targetRotation = initRot * target.localRotation;
		}
		bone.SetPose(joint.transform.position, joint.transform.rotation);
	}
}
