using UnityEngine;

public class BulletEmitter : MonoBehaviour {
	[SerializeField] ProjectileData p;
	float t;
	void Update() {
		if (t >= 0.2f) { 
			ProjectileManager.CreateProjectile(new Projectile(p, transform.position, transform.forward + Random.insideUnitSphere * 0.1f)); 
			t = 0;
		}
		t += Time.deltaTime;
	}
}
