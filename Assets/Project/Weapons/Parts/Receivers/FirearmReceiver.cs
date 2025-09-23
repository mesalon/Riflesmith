using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Random = UnityEngine.Random;

public class FirearmReceiver : MonoBehaviour {
	public float CyclicInterval => 1 / (stats.cyclicRate / 60);
	public event Action OnFired;
	public Magwell magwell;
	public GrabInteractable grip;
	public ProjectileData simpleFirearmProjectile;
	[SerializeField] ReceiverStats baseStats;
	 public ReceiverStats stats;

	[HideInInspector] public ProjectileData chamber;
	[HideInInspector] public bool hammerState;
	[HideInInspector] public bool hammerLocked;
	[SerializeField] Transform chamberPoint;
	[SerializeField] List<AttachmentMount> mounts;
	bool disconnectorState;
	float fireTimer;

	[SerializeField] EventReference hammerUp;
	[SerializeField] EventReference hammerDown;
	[SerializeField] EventReference triggerUp;
	[SerializeField] EventReference triggerDown;

	public Vector3 targetPos;
	public Vector3 targetRot;


	
	void Awake() {
		RefreshAttachments();
	}

	void Update() {
		if (grip.Input.trigger.DidPass(stats.triggerThresh, grip.LastInput.trigger)) { RuntimeManager.PlayOneShot(triggerDown, transform.position); }
		if (grip.Input.trigger.DidFail(stats.triggerThresh, grip.LastInput.trigger)) { RuntimeManager.PlayOneShot(triggerUp, transform.position); }
		//if(grip.Input.trigger > stats.triggerThresh) print($"!hammerLocked: {!hammerLocked} && hammerState: {hammerState} && fireTimer >= _stats.cyclicInterval: {fireTimer >= stats.cyclicInterval} && !disconnectorState: {!disconnectorState}");
		if (simpleFirearmProjectile) {
			hammerState = true;
		}
		if (grip.Input.trigger > stats.triggerThresh && !hammerLocked && hammerState && fireTimer >= CyclicInterval && !disconnectorState) {
			fireTimer = 0;
			RuntimeManager.PlayOneShot(hammerUp, transform.position);
			disconnectorState = !stats.isFullAuto;
			hammerState = false;
			if (chamber || simpleFirearmProjectile) {
				Fire();
			}
		}

		if (grip.Input.trigger < Mathf.Min(0.1f, stats.triggerThresh)) { disconnectorState = false; }

		// Recoil
		/*targetPos = Vector3.Lerp(targetPos, Vector3.zero, stats.rs.posRecovery * Time.deltaTime);
		grip.extraPos = Vector3.Slerp(grip.extraPos, targetPos, stats.rs.posSpeed * Time.deltaTime);
		targetRot = Vector3.Lerp(targetRot, Vector3.zero, stats.rs.rotRecovery * Time.deltaTime);
		grip.extraRot = Vector3.Slerp(grip.extraRot, targetRot, stats.rs.rotSpeed * Time.deltaTime);*/
		
		fireTimer += Time.deltaTime;
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

	public void Fire() {
		ProjectileManager.CreateProjectile(new(chamber ?? simpleFirearmProjectile, chamberPoint.position, chamberPoint.forward, stats.barrelLength));
		RuntimeManager.PlayOneShot((chamber ?? simpleFirearmProjectile).shotSound, chamberPoint.position);
		targetPos += new Vector3(Random.Range(-stats.rs.pos.x, stats.rs.pos.x), stats.rs.pos.y, -stats.rs.pos.z) / 100;
		targetRot += new Vector3(stats.rs.rot.x, Random.Range(-stats.rs.rot.y, stats.rs.rot.y), Random.Range(-stats.rs.rot.z, stats.rs.rot.z));
				
		OnFired?.Invoke();
	}
}

[Serializable] public struct ReceiverStats { 
	public float cyclicRate;
	public float triggerThresh;
	public bool isFullAuto;
	public bool gasBlowback;
	public float springSpeed;
	public float barrelLength;
	
	[Serializable] public class RecoilSettings {
		public float posSpeed, rotSpeed;
		public float posRecovery, rotRecovery;
		public float stability;
		public Vector3 pos, rot;
	} public RecoilSettings rs;
}