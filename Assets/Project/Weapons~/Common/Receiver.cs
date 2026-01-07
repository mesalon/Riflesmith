using UnityEngine;
using FMODUnity;
using System;

namespace Firearm {
	public abstract class Receiver : MonoBehaviour {
		public float CyclicInterval => 1 / (stats.cyclicRate / 60);
		public ReceiverStats stats;
		[HideInInspector] public ProjectileData chamber;
		[HideInInspector] public bool hammerState;
		[HideInInspector] public bool hammerLocked;
		[SerializeField] EventReference hammerUp;
		[SerializeField] EventReference hammerDown;
		[SerializeField] EventReference triggerUp;
		[SerializeField] EventReference triggerDown;

		[SerializeField] List<AttachmentMount> mounts;
		[SerializeField] ParticleSystem muzzleFlash;
		[SerializeField] Light muzzleLight;
		[SerializeField] Transform chamberPoint;
		[SerializeField] ReceiverStats baseStats;
		private bool disconnectorState;
		private float trigger, lastTrigger;
		private float fireTime;

		void Awake() {
			RefreshAttachments();
		}

		public virtual void Fire() {
		}

		public virtual void Update() {
			if (fireTime >= 0.05f) { muzzleLight.enabled = false; }
			if (trigger.DidPass(stats.triggerThresh, lastTrigger)) { RuntimeManager.PlayOneShot(triggerDown, transform.position); }
			if (trigger.DidFail(stats.triggerThresh, lastTrigger)) { RuntimeManager.PlayOneShot(triggerUp, transform.position); }
			if (trigger > stats.triggerThresh && !hammerLocked && hammerState && fireTime >= CyclicInterval && !disconnectorState) {
				fireTime = 0;
				RuntimeManager.PlayOneShot(hammerUp, transform.position);
				disconnectorState = !stats.isFullAuto;
				hammerState = false;
				if (chamber) {
					Fire();
				}
			}

			if (trigger < Mathf.Min(0.1f, stats.triggerThresh)) { disconnectorState = false; }
			fireTime += Time.deltaTime;
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
	}

	[Serializable] public struct ReceiverStats { 
		public float cyclicRate;
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
}
