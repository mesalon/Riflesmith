using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Random = UnityEngine.Random;

public class FirearmReceiver : Firearm {
	public event Action OnFired;
	public Magwell magwell;
	public GrabInteractable grip;
	[SerializeField] ReceiverStats baseStats;

	[SerializeField] List<AttachmentMount> mounts;


	public Vector3 targetPos;
	public Vector3 targetRot;


	
	void Awake() {
		RefreshAttachments();
	}

	void Update() {
		// Recoil
		/*targetPos = Vector3.Lerp(targetPos, Vector3.zero, stats.rs.posRecovery * Time.deltaTime);
		grip.extraPos = Vector3.Slerp(grip.extraPos, targetPos, stats.rs.posSpeed * Time.deltaTime);
		targetRot = Vector3.Lerp(targetRot, Vector3.zero, stats.rs.rotRecovery * Time.deltaTime);
		grip.extraRot = Vector3.Slerp(grip.extraRot, targetRot, stats.rs.rotSpeed * Time.deltaTime);*/
	}
	
	public void Eject() {
		if (chamber) { // todo: make it eject round if unfired
			Casing casing = Instantiate(chamber.casing, chamberPoint.position, chamberPoint.rotation);
			Vector3 force = new Vector3(Random.Range(3, 5), Random.Range(3, 5), Random.Range(-1, 1));
			casing.rb.AddForce(chamberPoint.rotation * force, ForceMode.Impulse);
			casing.rb.AddTorque(force * 20, ForceMode.Impulse);	
			chamber = null;
		}
	}
	
	public void RefreshAttachments() {
		stats = baseStats;
		foreach (AttachmentMount m in mounts) {
			m.UpdateAttachment(this);
		}
	}

	public void Fire() { OnFired?.Invoke(); }
}
