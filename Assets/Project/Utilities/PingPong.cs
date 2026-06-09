using UnityEngine;

public class PingPong : MonoBehaviour {
	[SerializeField] float size = 5;
	Vector3 pos;
	void Awake() {
		pos = transform.position;
	}

	void Update() {
		transform.position = pos + Random.insideUnitSphere * size;
	}
}
