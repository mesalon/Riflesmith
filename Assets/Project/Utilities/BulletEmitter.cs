using UnityEngine;

public class BulletEmitter : MonoBehaviour {
	[SerializeField] ProjectileData p;
	[SerializeField] float rate;
	float t;
	void Update() {
		if (t >= rate) { 
			ProjectileManager.CreateProjectile(new Projectile(p, transform.position, transform.forward + Random.insideUnitSphere * 0.1f, p.maxSpeed)); 
			t = 0;
		}
		t += Time.deltaTime;
	}
}
