using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

[ExecuteAlways] public class PoseCreator : MonoBehaviour {
	public Transform[] bones;
	[SerializeField] HandPoseObject preview;
	private HandPose reference;
	private HandPose modified;
	private Transform returnTo;
	private Pose[] init;

	void Awake() {
		init = CopyFromBones();
		modified = new(bones.Length);
	}

	public Pose[] CopyFromBones() {
		Pose[] poses = new Pose[bones.Length];
		for (int i = 0; i < bones.Length; i++) { poses[i] = new(bones[i].localPosition, bones[i].localRotation); }
		return poses;
	}

	void Update() {
		for (int i = 0; i < bones.Length; i++) {
			if (bones[i].localPosition != init[i].position || bones[i].localRotation != init[i].rotation) {
				modified.mask[i] = true;
				modified.poses[i] = new(bones[i].localPosition, bones[i].localRotation);
			}
		}
	}

	void OnDrawGizmos() {
		for (int i = 0; i < modified.poses.Length; i++) {
			Gizmos.color = Color.white;
			if (i > 0 && modified.mask[i-1]) Gizmos.color = Color.green;
			Gizmos.DrawLine(bones[i].position, bones[i].parent.position);
		}
	}

	public void InitializeFrom(HandPoseObject hp, Transform target = null) {
		if (target) { 
			transform.SetPose(target.position, target.rotation); 
			returnTo = target;
		}

		reference = hp;
		HandPose.Apply(reference, bones);
		Update();
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(PoseCreator))]
	public class PoseCreatorEditor : Editor {
		public override VisualElement CreateInspectorGUI() {
			VisualElement root = new();
			PoseCreator p = (PoseCreator)target;
			PropertyField previewField = new(serializedObject.FindProperty("preview"));
			previewField.RegisterValueChangeCallback(evt => {
				var hpo = (HandPoseObject)evt.changedProperty.objectReferenceValue;
					if (hpo) {
						HandPose.Apply(hpo.data, p.bones);
						p.init = p.CopyFromBones();
					}
			});
			root.Add(previewField);

			if (p.modified.poses != null) {
				for (int i = 0; i < p.modified.poses.Length; i++) {
					int index = i;
					string Label() => $"{p.bones[index].name} ({index}): {(p.modified.mask[index] ? $"{p.modified.poses[index].position} | {p.modified.poses[index].rotation.eulerAngles}" : "none")}";
					Button button = null;
						button = new(() => {
							p.modified.poses[index] = default;
							p.modified.mask[index] = false;
							p.bones[index].SetPose(p.init[index].position, p.init[index].rotation, Space.Self);
							button.text = Label();
						}) { text = Label() };
					button.style.unityTextAlign = TextAnchor.MiddleLeft;
					root.Add(button);
				}
			} else {
				root.Add(new Label("Not initialized"));
			}
			root.Add(new Button(() => {
				for (int i = 0; i < p.bones.Length; i++) {
					p.modified.mask[i] = true;
					p.modified.poses[i] = new(p.bones[i].localPosition, p.bones[i].localRotation);
				}
				Save(p);
			}) { text = "Capture all" });
			root.Add(new Button(() => Save(p)) { text = "Save" });
			return root;
		}

		private void Save(PoseCreator p) {
			p.reference.poses = p.modified.poses;
			p.reference.mask = p.modified.mask;
			print($"Saving: Reference is {p.reference}. Modified poses is {p.modified} with {p.modified.poses.Length}");
			Selection.activeObject = p.returnTo;
			DestroyImmediate(p.gameObject);
		}
	}
#endif
}
