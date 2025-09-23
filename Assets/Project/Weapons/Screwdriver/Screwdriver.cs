using System;
using UnityEngine;

public class Screwdriver : GrabInteractable {
	[SerializeField] Transform tip;
	[SerializeField] float reach;
	void Update() {
		if (Input.trigger.DidPass(0.5f, LastInput.trigger)) {
			foreach (Collider col in Physics.OverlapSphere(tip.position, reach)) {
				if (col.TryGetComponent(out AttachmentMount detected) && detected.attachment) {
					detected.attachment.Detach();
				}
			}
		}
	}
}
