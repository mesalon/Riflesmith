using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GearSlot : MonoBehaviour {
	[SerializeField] BoxCollider check;
	[SerializeField] Transform pose;
	Dictionary<GrabInteractable, Action> lastOverlap = new();
	List<GrabInteractable> overlap = new();
	Collider[] olapBuffer = new Collider[512];
	(GrabInteractable, Action) item;

	void Awake() {
		if (!pose) {
			pose = transform;
		}
	}

	void Update() {
		if (!item.Item1) {
			overlap.Clear();
			int hits = Physics.OverlapBoxNonAlloc(check.transform.TransformPoint(check.center), Vector3.Scale(check.transform.lossyScale, check.size) / 2, olapBuffer, check.transform.rotation);
			for (int i = 0; i < hits; i++) { if (olapBuffer[i].TryGetComponent(out GrabInteractable grab) && grab.gameObject != gameObject) overlap.Add(grab); }
			if (hits == olapBuffer.Length) { Debug.LogError("Gear slot buffer out of space!"); } // todo: Dynamic allocation

			foreach (GrabInteractable grab in lastOverlap.Keys.ToList()) { // Are you still there?
				if (!overlap.Contains(grab)) {
					grab.onDropped -= lastOverlap[grab];
					lastOverlap.Remove(grab);
				}
			}

			foreach (GrabInteractable grab in overlap) {
				if (!lastOverlap.ContainsKey(grab)) { // Are you new?
					Action droppedAction = () => Put(grab);
					grab.onDropped += droppedAction;
					lastOverlap.Add(grab, droppedAction);
				}
			}
		}
	}

	void Put(GrabInteractable item) {
		item.root.SetParent(transform);
		item.root.SetPose(pose.position, pose.rotation);
		item.rb.isKinematic = true;
		
		Action pickedAction = () => Remove(item);
		item.onPicked += pickedAction;
		this.item = (item, pickedAction);
	}

	void Remove(GrabInteractable item) {
		item.root.SetParent(null);
		item.rb.isKinematic = false;
		item.onPicked -= this.item.Item2;
		this.item = (null, null);
	}
}