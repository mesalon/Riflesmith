using UnityEngine;

public class BulletEmitter : MonoBehaviour {
	[SerializeField] float rate;
	float t;
	void Update() {
		if (t >= rate) { 
			ProjectileManager.CreateGenericProjectile(transform.position, transform.forward + Random.insideUnitSphere * 0.1f);
			t = 0;
		}
		t += Time.deltaTime;
	}
}
