using FMODUnity;
using UnityEngine;

public class EnemyFirearm : GrabInteractable {
	public Transform muzzle;
	public int rounds;
	[HideInInspector] public bool triggerState;
	[SerializeField] private float cyclicRate;
	[SerializeField] private int capacity;
	[SerializeField] private EventReference shot;
	[SerializeField] ProjectileData projectile;
	[SerializeField] ParticleSystem muzzleFlash;
	[SerializeField] Light muzzleLight;
	private float fireTime;
	private bool releaseImmediately;
	private float lightT;

	private void Awake() {
		rounds = capacity;
	}

	private void Update() {
		if (lightT >= 0.05f) { muzzleLight.enabled = false; }
		if (triggerState && fireTime > 1 / (cyclicRate / 60) && rounds > 0) {
			if (releaseImmediately) {
				triggerState = false;
				releaseImmediately = false;
			}
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
	}
	public void FireOnce() {
		triggerState = true;
		releaseImmediately = true;
	}
}
