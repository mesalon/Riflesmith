using FMODUnity;
using UnityEngine;

public class SimpleFirearm : GrabInteractable {
	public Transform muzzle;
	public int rounds;
	[HideInInspector] public bool triggerState;
	[SerializeField] float cyclicRate;
	[SerializeField] int capacity;
	[SerializeField] EventReference shot;
	[SerializeField] ProjectileData projectile;
	[SerializeField] ParticleSystem muzzleFlash;
	[SerializeField] Light muzzleLight;
	private float fireTime;
	private float lightT;

	private void Awake() {
		rounds = capacity;
	}

	private void Update() {
		if (lightT >= 0.05f) { muzzleLight.enabled = false; }
		if (triggerState && fireTime > 1 / (cyclicRate / 60) && rounds > 0) {
			muzzleFlash.Emit(1);
			muzzleLight.enabled = true;
			lightT = 0;
			RuntimeManager.PlayOneShot(shot, muzzle.position);
			ProjectileManager.CreateProjectile(new(projectile, muzzle.position, muzzle.forward));

			fireTime = 0;
			rounds--;
		}
		fireTime += Time.deltaTime;
		lightT += Time.deltaTime;
		triggerState = false; // Trigger must be pulled continuously.
	}
}
