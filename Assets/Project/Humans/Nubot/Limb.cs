using UnityEngine;

public class Limb : MonoBehaviour, IDamageable {
	public Nubot self;
	public float damageMultiplier = 1;

	public Rigidbody rb;
	public void Damage(float amount) { 
		self.body.Damage(amount * damageMultiplier); 
	}
}
