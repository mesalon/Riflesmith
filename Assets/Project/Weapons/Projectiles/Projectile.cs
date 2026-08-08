using UnityEngine;

public struct Projectile {
	private readonly static Mesh quad = new() {
		vertices = new Vector3[] { new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(-0.5f, 0.5f, 0), new(0.5f, 0.5f, 0) },
		uv = new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) },
		triangles = new int[] { 0, 2, 1, 2, 3, 1 },
	};
	public CartridgeData data;
	public Vector3 position;
	public Vector3 velocity;
	private Vector3 lastPosition;

	public void Tick(ProjectileManager pm, out bool doTerminate) {
		doTerminate = false;
		lastPosition = position;
		velocity += Physics.gravity * Time.deltaTime;
		position += velocity * Time.deltaTime;

		if (pm.showDebugTracers) {
			float time = pm.debugTracerTime == 0 ? Time.deltaTime : pm.debugTracerTime;
			Color col = Physics.Linecast(lastPosition, position, out _) ? Color.red : pm.debugTracerColor;
			Debug.DrawLine(lastPosition, position, col, time);
		}

		Vector3 end = position - lastPosition;
		RaycastHit[] hits = Physics.RaycastAll(lastPosition, end, end.magnitude, pm.mask);

		foreach (RaycastHit hit in hits) {
			if (hit.transform.TryGetComponent(out IDamageable dmg)) { dmg.Damage(data.damage); }
			if (hit.transform.TryGetComponent(out Rigidbody rb)) { rb.AddForceAtPosition(velocity * data.bulletMass, hit.point); }
			if (hit.collider.sharedMaterial) {
				pm.CreateFX(hit.collider.sharedMaterial.name switch {
						_ => pm.genericFx,
						}, hit.point, hit.normal);
			}
			doTerminate = true;
			break;
		}

		Quaternion lookRot = Quaternion.LookRotation(velocity);
		Vector3 scale = data.tailMat.GetVector("_Scale");
		Graphics.DrawMesh(quad, Matrix4x4.TRS(position + (lookRot * Vector3.forward * scale.y / 2), lookRot * Quaternion.Euler(90, 0, 0), Vector3.one), data.tailMat, 0);
		Graphics.DrawMesh(quad, Matrix4x4.TRS(position + (lookRot * Vector3.forward * scale.y), Quaternion.identity, Vector3.one * data.headMat.GetFloat("_Size")), data.headMat, 0);
	}
}
