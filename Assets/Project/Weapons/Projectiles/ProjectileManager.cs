using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour {
	private static readonly List<Projectile> projectiles = new();

	public LayerMask mask;
	public GameObject genericFx;
	public CartridgeData genericCartridge;
	private static CartridgeData generic;

	[Header("Debug Tracers")]
	public bool showDebugTracers;
	public float debugTracerTime;
	public Color debugTracerColor = Color.white;

	void Awake() { generic = genericCartridge; }

	void Update() {
		for(int i = projectiles.Count - 1; i >= 0; i--) {
			Projectile p = projectiles[i];
			p.Tick(this, out bool destroyProjectile);
			projectiles[i] = p;
			if(destroyProjectile) {
				projectiles.RemoveAt(i);
			}
		}
	}

	public static void CreateProjectile(Projectile projectile) { projectiles.Add(projectile); }
	public static void CreateGenericProjectile(Vector3 position, Vector3 velocity) {
		CreateProjectile(new() { data = generic, position = position, velocity = velocity });
	}

	public void CreateFX(GameObject fx, Vector3 pos, Vector3 normal) {
		Instantiate(fx, pos, Quaternion.LookRotation(normal));
	}
}
