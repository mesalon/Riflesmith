using UnityEngine;

public class Cartridge : GrabInteractable {
	[SerializeField] ProjectileData data;

	private void Update() {
		Collider[] overlap = Physics.OverlapSphere(transform.position, 0.01f);
		foreach (Collider col in overlap) {
			if (Interactor && col.TryGetComponent(out Magazine mag) && mag.TryInsert(data)) {
				Interactor.Drop();
				Destroy(root.gameObject);
			}
		}
	}
}
