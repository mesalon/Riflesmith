using UnityEngine;

public class CartridgeObject : MonoBehaviour {
	[System.NonSerialized] public Cartridge data;
	public Rigidbody rb;
	public CartridgeVisual unfired, fired;
	[SerializeField] Collider col;
	private bool lastIsFired;
	private int unphaseFrame;

	void OnValidate() {
		unfired.Cache();
		fired.Cache();
	}

	void Start() {
		col.isTrigger = true;
		unphaseFrame = Time.frameCount + 1;
	}

	void Update() {
		if (col.isTrigger && Time.frameCount >= unphaseFrame) { col.isTrigger = false; }
		Debug.DrawRay(transform.position, rb.linearVelocity, Color.blue);
		if (data.isFired && !lastIsFired) {
			fired.gameObject.SetActive(true);
			unfired.gameObject.SetActive(false);
		}
		lastIsFired = data.isFired;
	}

	void OnCollisionEnter(Collision col) {
		if (col.body && col.body.TryGetComponent(out Magazine mag)) {
			if (data.data && mag.TryLoad(data)) { 
				data.data = null;
				Destroy(gameObject); 
			}
		}
	}
}

[System.Serializable] public struct CartridgeVisual {
	public GameObject gameObject;
	[HideInInspector] public Material mat;
	[HideInInspector] public Mesh mesh;

	public void Cache() {
		mat  = gameObject ? gameObject.GetComponent<MeshRenderer>()?.sharedMaterial : null;
		mesh = gameObject ? gameObject.GetComponent<MeshFilter>()?.sharedMesh : null;
	}
}
