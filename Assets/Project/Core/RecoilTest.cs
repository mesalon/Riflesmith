using UnityEngine;

public class RecoilTest : MonoBehaviour {
	[SerializeField] Vector3 impulse;

	[SerializeField] SpringSystem x, y, z;

	private void Update() {
		if (Input.GetMouseButtonDown(0)) { 
			x.velocity += impulse.x; 
			y.velocity += impulse.y; 
			z.velocity += impulse.z; 
		}
		x.Update();
		y.Update();
		z.Update();
		transform.localRotation = Quaternion.Euler(new(x.position, y.position, z.position));
	}
}

[System.Serializable]
public class SpringSystem {
	private float omega => 2f * Mathf.PI * frequency;
	private float kDamping => 2f * damping / omega;
	private float kMass => 1f / (omega * omega);

	[HideInInspector] public float position;
	[HideInInspector] public float velocity;
	[SerializeField] float frequency = 10; 
	[SerializeField] float damping = 0.5f;  

	public void Update() {
		float stableMass = Mathf.Max(kMass, // Are you needed?
				(Time.deltaTime * Time.deltaTime / 2f) + (Time.deltaTime * kDamping / 2f), 
				Time.deltaTime * kDamping); 
		float force = -position - (kDamping * velocity);
		velocity += force / kMass * Time.deltaTime;
		position += velocity * Time.deltaTime;
	}
}
