using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;
using System;
using FMODUnity;
using UnityEditor.Rendering;
using Random = UnityEngine.Random;

public class GunWizard : EditorWindow {
	[SerializeField] GameObject body;
	[SerializeField] GameObject magazine;
	[SerializeField] EventReference foley;
	[SerializeField] EventReference attach, unattach;
	[SerializeField] List<GameObject> attachments;
	[SerializeField] SerializedObject sObject;
	[SerializeField] private VisualTreeAsset tree;

	[SerializeField] SerializedObject firearmObject;
	[SerializeField] SerializedProperty chamberPointProp;

	[MenuItem("Tools/Gun Wizard")]
	public static void ShowExample() {
		GetWindow<GunWizard>().titleContent = new("Gun Wizard");
	}

	private void OnEnable() { sObject = new(this); }
	private void OnDisable() { sObject?.Dispose(); }

	public void CreateGUI() {
		tree.CloneTree(rootVisualElement);
		rootVisualElement.Bind(sObject);
		List<AttachmentMount> mounts = new();
		
		rootVisualElement.Q<Button>("init").clicked += () => {
			mounts.Clear();
			if (rootVisualElement.Q<DropdownField>("mode").value == "Firearm") { // Create firearm with attachments
				FirearmReceiver receiver = (FirearmReceiver)new GameObject(body.name).AddComponent(typeof(FirearmReceiver));
				receiver.transform.position = body.transform.position;
				SerializedObject serializedGun = new(receiver);

				Transform chamber = new GameObject("Chamber").transform;
				chamber.SetParent(receiver.transform);
				serializedGun.FindProperty("chamberPoint").objectReferenceValue = chamber;

				SerializedProperty mountsProperty = serializedGun.FindProperty("mounts");
				// Add attachments
				foreach (GameObject go in attachments) { InitAttachment(receiver.transform, go.transform.position, go.name + " Mount", mounts, mountsProperty, receiver); }
				serializedGun.ApplyModifiedProperties();

				// Add magwell
				InitAttachment(receiver.transform, magazine.transform.position, "Magwell", mounts, mountsProperty, receiver);
			}
			else { // Add attachment to attachments
				Attachment attachment = body.GetComponent<Attachment>();
				Debug.Log(attachment);
				SerializedObject serialized = new(attachment);
				Debug.Log(serialized);

				foreach (GameObject go in attachments) { InitAttachment(body.transform, go.transform.position, go.name + " Mount", mounts, serialized.FindProperty("mounts")); }
				serialized.ApplyModifiedProperties();
			}
		};
		
		rootVisualElement.Q<Button>("build").clicked += () => {
			for (int i = 0; i < attachments.Count; i++) {
				GameObject go = attachments[i];
				Type type = mounts[i] is Magazine ? typeof(Magazine) : typeof(Attachment);
				Component at = new GameObject(go.name).AddComponent(type);
				at.transform.position = go.transform.position;
				at.gameObject.AddComponent(typeof(Rigidbody));
				SerializedObject serializedAt = new(at);

				Transform mountPoint = new GameObject("Mount").transform;

				mountPoint.transform.position = mounts[i].transform.position;
				mountPoint.transform.SetParent(at.transform, true);
				serializedAt.FindProperty("mountPoint").objectReferenceValue = mountPoint;
				serializedAt.FindProperty("detector").objectReferenceValue = AddBox(mountPoint.gameObject);
				serializedAt.FindProperty("type").stringValue = new SerializedObject(mounts[i]).FindProperty("type").stringValue;
				serializedAt.ApplyModifiedProperties();
			}
		};
	}

	BoxCollider AddBox(GameObject go) {
		BoxCollider box = (BoxCollider)go.AddComponent(typeof(BoxCollider));
		box.size = Vector3.one * 0.02f;
		box.isTrigger = true;
		return box;
	}
	
	void InitAttachment(Transform parent, Vector3 position, string name, List<AttachmentMount> mountList, SerializedProperty mountsProperty, FirearmReceiver receiver = null, Attachment attachment = null) {
		MonoBehaviour theNotNullOne = receiver as MonoBehaviour ?? attachment;
		AttachmentMount mount = (AttachmentMount)new GameObject(name).AddComponent(typeof(AttachmentMount));
		mount.transform.SetParent(parent);
		mount.transform.position = position;
		AddBox(mount.gameObject);
					
		SerializedObject serializedMount = new(mount);
		serializedMount.FindProperty(receiver ? "receiver" : "parent").objectReferenceValue = theNotNullOne;
		serializedMount.FindProperty("type").stringValue = Random.value.ToString();
		/*serializedMount.FindProperty("foley").FindPropertyRelative("Guid").managedReferenceValue = foley.Guid;
		serializedMount.FindProperty("attach").FindPropertyRelative("Guid").managedReferenceValue = attach.Guid;
		serializedMount.FindProperty("unattach").FindPropertyRelative("Guid").managedReferenceValue = unattach.Guid;*/
		serializedMount.ApplyModifiedProperties();
		mountsProperty.AddToPropertyList(mount);
		mountList.Add(mount);
	}
}