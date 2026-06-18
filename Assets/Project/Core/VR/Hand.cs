// todo: hand stretching
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class Hand : MonoBehaviour {
	public ConfigurableJoint joint => handMuscle.joint;
	public DeviceInput Input => (side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand).RelativeTo(HeadOffset);
	public DeviceInput LastInput => (side == Side.Left ? VRPlayer.LastInput.LHand : VRPlayer.LastInput.RHand).RelativeTo(HeadOffset);
	private Vector3 HeadOffset => VRPlayer.Input.head.position.FlattenY();

	public IInteractable held;
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
		Ext.Label(Vector3.zero, "Hi!");
		VRGizmos.Axis(transform.position, transform.rotation, 0.05f);

		Vector3 pos = Input.position; // Keep the hand positions relative to the head until they have a real input
		if (VRPlayer.Input.head.gotFirstInput && !Input.gotFirstInput) { pos += VRPlayer.Input.head.position.FlattenY(); }
		transform.SetPose(Input.position, Input.rotation, Space.Self);
		controllerVisual.SetPose(pos, Input.rotation, Space.Self);

		held?.OnHold();
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
							if (col.transform.root.TryGetComponent(out Rigidbody grabbable)) {
								Ext.IgnoreCollisionsBetween(associatedColliders, grabbable.GetComponentsInChildren<Collider>(), true);
								grabJoint = grabbable.gameObject.AddComponent<ConfigurableJoint>();
								grabJoint.connectedBody = rb;
								grabJoint.autoConfigureConnectedAnchor = false;
								grabJoint.anchor = grabbable.transform.InverseTransformPoint(rb.position);
								grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = heldDrive;
								grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
								grabJoint.slerpDrive = heldDrive;
								break;
							} 
						}
						foreach (Collider col in overlap) { 
							if (col.transform.TryGetComponent(out IInteractable i)) {
								Pick(i);
								break;
							} 
						}
					}
				}
			} 		
		} else if (grabJoint) {
			if(lockit) return;
			Drop();
			Ext.IgnoreCollisionsBetween(associatedColliders, grabJoint.transform.GetComponentsInChildren<Collider>(), false); // todo: this fucking sucks
			Destroy(grabJoint);
		} else { 
			gripTime = 0;
		}


		held?.OnHoldFixed();
	}

	public void Pick(IInteractable i) {
		if (!i.PreventInteraction) {
			if (i.Interactor) i.Interactor.Drop(); // Release if something else is holding it
			held = i;
			held.Interactor = this;
			held.OnPicked();
		}
	}

	public void Drop() {
		if (held != null) {
			held.OnDropped();
			held.Interactor = null;
			held = null;
		}
	}

	private void ApplyPose(Quaternion[] pose, bool mirrored = false) {
		for (int i = 0; i < bones.Length; i++) { 
			bones[i].localRotation = mirrored ? new Quaternion(-pose[i].x, pose[i].y, pose[i].z, -pose[i].w): pose[i]; 
		}
	}

	private static Quaternion[] LerpPose(Quaternion[] a, Quaternion[] b, float t) {
		if (a.Length != b.Length) {
			Debug.LogError("Length mismatch for hand pose! Stinky! This should never happen.");
			return a;
		}
		var blend = new Quaternion[a.Length];
		for (int i = 0; i < a.Length; i++) { blend[i] = Quaternion.Lerp(a[i], b[i], t); }
		return blend;
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(Hand)), CanEditMultipleObjects]
	public class HandEditor : Editor {
		public override VisualElement CreateInspectorGUI() {
			VisualElement container = new();
			InspectorElement.FillDefaultInspector(container, serializedObject, this);

			ObjectField slot = new("Pose") {
				objectType = typeof(HandPose),
				allowSceneObjects = false,
			};

			VisualElement row = new();
			row.style.flexDirection = FlexDirection.Row;
			row.style.marginTop = 4;
			Button createNew = new(() => {
					var pose = CreateInstance<HandPose>();
					pose.poses = ((Hand)target).Capture();
					AssetDatabase.CreateAsset(pose, "Assets/Project/Humans/Player/NewPose.asset");
					AssetDatabase.SaveAssets();
					if (slot.value == null) slot.value = pose;
					}) { text = "Create new" };
			Button update = new(() => {
					if (slot.value != null) {
					((HandPose)slot.value).poses = ((Hand)target).Capture();
					EditorUtility.SetDirty(slot.value);
					AssetDatabase.SaveAssets();
					}
					}) { text = "Update" };
			Button apply = new(() => {
					if (slot.value != null) ((Hand)target).ApplyPose(((HandPose)slot.value).poses);
					EditorUtility.SetDirty(target);
					}) { text = "Apply to hand" };
			update.style.flexGrow = createNew.style.flexGrow = apply.style.flexGrow = 1;
			row.Add(update);
			row.Add(createNew);
			row.Add(apply);

			Foldout foldout = new() { text = "Pose Editor" };
			foldout.style.marginTop = 8;
			foldout.Add(slot);
			foldout.Add(row);
			container.Add(foldout);

			return container;
		}
	}

	private Quaternion[] Capture() {
		Quaternion[] rots = new Quaternion[bones.Length];
		for (int i = 0; i < bones.Length; i++) {
			rots[i] = bones[i].localRotation;
		}
		return rots;
	}
#endif
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
