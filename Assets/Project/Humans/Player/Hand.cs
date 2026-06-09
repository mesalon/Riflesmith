// todo: hand stretching
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class Hand : Interactor {
	private Vector3 HeadOffset => VRPlayer.Input.head.position.FlattenY();
	public DeviceInput Input => (side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand).RelativeTo(HeadOffset);
	public DeviceInput LastInput => (side == Side.Left ? VRPlayer.LastInput.LHand : VRPlayer.LastInput.RHand).RelativeTo(HeadOffset);
	public Transform[] bones;
	[SerializeField] Side side;
	[SerializeField] Transform shoulder;
	[SerializeField] HandPose idlePose, fistPose, grabPose;
	[SerializeField] Transform forward;
	[SerializeField] Transform controller, target;
	[SerializeField] JointDriveData normalDrive, climbDrive, grabDrive;
	[SerializeField] float poseSpeed;
	[SerializeField] float reach;
	[SerializeField] float gripPoint, gripLeniency;
	[SerializeField] LayerMask grabLayer;
	[SerializeField] List<Collider> toIgnoreWhenGrabbing;
	private float gripTime;
	private JointDriveData currentDrive;
	private Quaternion[] currentPose;
	private FixedJoint grabJoint;

	void Awake() {
		currentDrive = normalDrive;
		currentPose = idlePose.poses;
	}

	void Update() {
		Vector3 pos = Input.position; // Keep the hand positions relative to the head until they have a real input
		if (VRPlayer.Input.head.gotFirstInput && !Input.gotFirstInput) { pos += VRPlayer.Input.head.position.FlattenY(); }
		Quaternion truerot = Input.rotation * Quaternion.Inverse(forward.localRotation);
		target.SetPose(pos - truerot * forward.localPosition, truerot, Space.Self);
		controller.SetPose(pos, Input.rotation, Space.Self);
	}
	new void FixedUpdate() {
		base.FixedUpdate();

		if (Input.grip > gripPoint) {
			gripTime += Time.deltaTime;
			if (gripTime < gripLeniency) { Grip(); }
		} else {
			LetGo();
			gripTime = 0;
		}

		Quaternion[] newPose = grabJoint ? grabPose.poses : LerpPose(idlePose.poses, fistPose.poses, Input.grip);
		currentPose = LerpPose(currentPose, newPose, 1 - Mathf.Exp(-poseSpeed * Time.deltaTime));
		ApplyPose(currentPose);

		float toHand = (transform.position - shoulder.position).magnitude;
	}

	private void SetDrive(JointDriveData drive) {}

	public void Grip() {
		if (!grabJoint) {
			Collider[] overlap = Physics.OverlapSphere(grabPoint.position, reach, grabLayer);
			if (overlap.Length > 0) {
				overlap.PrintAll();
				Rigidbody rb = null;
				foreach (Collider col in overlap) { col.transform.root.TryGetComponent(out rb); break; }
				grabJoint = rb.gameObject.AddComponent<FixedJoint>();
				if (rb) { 
					Ext.IgnoreCollisionsBetween(toIgnoreWhenGrabbing, rb.GetComponentsInChildren<Collider>(), true); // todo: this fucking sucks
					grabJoint.connectedBody = rb; 
					currentDrive = grabDrive;
				} else { 
					currentDrive = climbDrive; 
				}
			}
		}
	}

	private void LetGo() {
		if (grabJoint) {
			if (grabJoint.connectedBody) {
				Ext.IgnoreCollisionsBetween(toIgnoreWhenGrabbing, grabJoint.connectedBody.GetComponentsInChildren<Collider>(), false);
			}
			Destroy(grabJoint);
		}
		currentDrive = normalDrive;
		Drop();
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
