using FMODUnity;
using UnityEngine;

public class SimpleFirearm : MonoBehaviour {
	public Transform muzzle;
	[HideInInspector] public int rounds;
	[HideInInspector] public bool triggerState;
	[SerializeField] float cyclicRate;
	[SerializeField] int capacity;
	[SerializeField] EventReference shot;
	[SerializeField] ProjectileData projectile;
	public Transform grip, foregrip;
	private float fireTime;

	private void Awake() {
		rounds = capacity;
	}

	private void Update() {
		if (triggerState && fireTime > 1 / (cyclicRate / 60) && rounds > 0) {
			RuntimeManager.PlayOneShot(shot, muzzle.position);
			ProjectileManager.CreateProjectile(new(projectile, muzzle.position, muzzle.forward, projectile.maxSpeed));
			fireTime = 0;
			rounds--;
		}
		fireTime += Time.deltaTime;
		triggerState = false; // Trigger must be pulled continuously.
	}
}
