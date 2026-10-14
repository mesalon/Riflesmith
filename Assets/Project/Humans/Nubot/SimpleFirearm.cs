using FMODUnity;
using UnityEngine;

public class SimpleFirearm : MonoBehaviour {
	public Transform grip, foregrip, stock;
	public Transform muzzle;
	public Rigidbody rb;
	[HideInInspector] public int rounds;
	[HideInInspector] public bool triggerState;
	[SerializeField] float cyclicRate;
	[SerializeField] int capacity;
	[SerializeField] EventReference shot;
	[SerializeField] GrabInteractable grab;
	[SerializeField] ParticleSystem muzzleFlash;
	private float fireTime;

	private void Awake() {
		rounds = capacity;
	}

	private void Update() {
		triggerState = grab.input.trigger > 0.8f;
		Debug.DrawRay(muzzle.position, muzzle.forward * 100, Color.red);
		if (triggerState && fireTime > 1 / (cyclicRate / 60) && rounds > 0) {
			RuntimeManager.PlayOneShot(shot, muzzle.position);
			ProjectileManager.CreateGenericProjectile(muzzle.position, muzzle.forward * 600);
			muzzleFlash.Emit(20);
			fireTime = 0;
			rounds--;
		}
		fireTime += Time.deltaTime;
		triggerState = false; // Trigger must be pulled continuously.
	}
}
