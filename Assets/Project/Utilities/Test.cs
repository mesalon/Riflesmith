using UnityEngine;

public class Test : MonoBehaviour, IDamageable {
	public void Damage(float amount) {
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = false;
	}
}
