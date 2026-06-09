using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour {
	private static List<Projectile> projectiles = new();
	public static ProjectileManager I { get; private set; }

	public LayerMask mask;
	public Material tailMat, headMat;
	public GameObject genericFx;

	[Header("Debug Tracers")]
	public bool showDebugTracers;
	public float debugTracerTime;
	public Color debugTracerColor = Color.white;
	
	private void Awake() { I = this; }
	private void OnDestroy() { I = null; }
	
	private void Update() {
		for(int i = projectiles.Count - 1; i >= 0; i--) {
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
