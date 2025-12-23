using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour {
	private static List<Projectile> projectiles = new();
	public static ProjectileManager I { get; private set; }

	public LayerMask mask;
	[Header("Debug Tracers")]
	public bool showDebugTracers;
	public float debugTracerTime;
	public Color debugTracerColor = Color.white;
	public Material tailMat, headMat;
	public GameObject concreteFX;
	public GameObject woodFX;
	public GameObject metalFX;
	public GameObject dirtFX;
	
	private void Awake() {
		if (I == null) {
			I = this;
			DontDestroyOnLoad(gameObject);
		}
		else { Destroy(gameObject); }
	}
	
	private void Update() {
		for(int i = projectiles.Count - 1; i >= 0; i--) { // Count backwards as to not make the gods angry
			projectiles[i].Draw(this);
			projectiles[i].Tick(this, out bool destroyProjectile);
			if(destroyProjectile) {
				projectiles.RemoveAt(i);
			}
		}
	}

	public static void CreateProjectile(Projectile projectile, int amount = 1) {
		for(int i = 0; i < amount; i++) {
			projectiles.Add(projectile);
		}
	}

	public void CreateFX(GameObject fx, Vector3 pos, Vector3 normal) {
		Instantiate(fx, pos, Quaternion.LookRotation(normal));
	}
}
