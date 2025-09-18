using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Todo: Consolodate code with gearslot
// Todo: replace this shitty system with a change check
// Todo: Standardize using non alloc for casts or fuck off with it
public class Backpack : MonoBehaviour {
	[SerializeField] BoxCollider check;
	Dictionary<GrabInteractable, Action> lastOverlap = new(), items = new();
	List<GrabInteractable> overlap = new();
	Collider[] olapBuffer = new Collider[512];
	
	void Update() {
		int hits = Physics.OverlapBoxNonAlloc(check.transform.TransformPoint(check.center), Vector3.Scale(check.transform.lossyScale, check.size) / 2, olapBuffer, check.transform.rotation);
		for (int i = 0; i < hits; i++) { if (olapBuffer[i].TryGetComponent(out GrabInteractable grab) && grab.gameObject != gameObject) overlap.Add(grab); }
		if(hits == olapBuffer.Length) { Debug.LogError("Backpack buffer out of space!"); } // todo: Dynamic allocation
		overlap.Clear();
		
		foreach (GrabInteractable grab in lastOverlap.Keys.ToList()) { // Are you still there?
			if (!overlap.Contains(grab)) {
				grab.onDropped -= lastOverlap[grab];
				lastOverlap.Remove(grab);
			}
		}

		foreach (GrabInteractable grab in overlap) {
			if (!lastOverlap.ContainsKey(grab)) { // Are you new?
				Action droppedAction = () => PutInBackpack(grab);
				grab.onDropped += droppedAction;
				lastOverlap.Add(grab, droppedAction);
			}
		}
	}

	void PutInBackpack(GrabInteractable item) {
		item.root.SetParent(transform);
		item.rb.isKinematic = true;
		Action pickedAction = () => TakeFromBackpack(item);
		item.onPicked += pickedAction;
		items.Add(item, pickedAction);
	}

	void TakeFromBackpack(GrabInteractable item) {
		item.root.SetParent(null);
		item.rb.isKinematic = false;
		item.onPicked -= items[item];
		items.Remove(item);
	}
}

/*
// Todo: Consolodate code with gearslot
// Todo: replace this shitty system with a change check
public class Backpack : MonoBehaviour {
	[SerializeField] BoxCollider check;
	List<GrabInteractable> lastOverlap = new();
	List<GrabInteractable> overlap = new();
	Collider[] olapBuffer = new Collider[512];
	
	void Update() {
		overlap.Clear();
		int hits = Physics.OverlapBoxNonAlloc(check.transform.TransformPoint(check.center), Vector3.Scale(check.transform.lossyScale, check.size) / 2, olapBuffer, check.transform.rotation);
		for (int i = 0; i < hits; i++) { if (olapBuffer[i].TryGetComponent(out GrabInteractable grab) && grab.gameObject != gameObject) overlap.Add(grab); }
		if(hits == olapBuffer.Length) { Debug.LogError("Backpack buffer out of space!"); } // todo: Dynamic allocation

		foreach (GrabInteractable grab in lastOverlap) {
			if()
		}
		
		lastOverlap = overlap;
	}

	void PutInBackpack(GrabInteractable item) {
		item.root.SetParent(transform);
		item.rb.isKinematic = true;
		Action pickedAction = () => TakeFromBackpack(item);
		item.onPicked += pickedAction;
		items.Add(item, pickedAction);
	}

	void TakeFromBackpack(GrabInteractable item) {
		item.root.SetParent(null);
		item.rb.isKinematic = false;
		item.onPicked -= items[item];
		items.Remove(item);
	}
}
*/

