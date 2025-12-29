using UnityEngine;

public class Projectile {
	private Vector3 position;
	private Vector3 lastPosition;
	Vector3 velocity;

	public ProjectileData data;

	public Projectile(ProjectileData data, Vector3 position, Vector3 direction) {
		this.data = data;
		this.position = position;
		velocity = direction.normalized * data.maxSpeed;
	}

	public void Tick(ProjectileManager p, out bool destroyProjectile) {
		destroyProjectile = false;
		lastPosition = position;
		velocity += Physics.gravity * Time.deltaTime;
		position += velocity * Time.deltaTime;

		if (p.showDebugTracers) {
			float time = p.debugTracerTime == 0 ? Time.deltaTime : p.debugTracerTime;
			Color col = Physics.Linecast(lastPosition, position, out _) ? Color.red : p.debugTracerColor;
			Debug.DrawLine(lastPosition, position, col, time);
		}

		Vector3 end = position - lastPosition;
		RaycastHit[] hits = Physics.RaycastAll(lastPosition, end, end.magnitude, p.mask);
		
		foreach (RaycastHit hit in hits) {
			if (hit.transform.TryGetComponent(out IDamageable dmg)) { dmg.Damage(data.damage); }
			if (hit.transform.TryGetComponent(out Rigidbody rb)) { rb.AddForceAtPosition(velocity.normalized * data.force, hit.point); }
			if (hit.collider.sharedMaterial) {
				p.CreateFX(hit.collider.sharedMaterial.name switch {
						_ => p.genericFx,
						}, hit.point, hit.normal);
			}
			destroyProjectile = true;
			break;
		}
	}
	
	public void Draw(ProjectileManager p) {
		Mesh quad = new() {
			vertices = new Vector3[] { new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(-0.5f, 0.5f, 0), new(0.5f, 0.5f, 0) },
			uv = new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) },
			triangles = new int[] { 0, 2, 1, 2, 3, 1 },
		};
		Quaternion lookRot = Quaternion.LookRotation(velocity);
		Vector3 scale = p.tailMat.GetVector("_Scale");
		Graphics.DrawMesh(quad, Matrix4x4.TRS(position + (lookRot * Vector3.forward * scale.y / 2), lookRot * Quaternion.Euler(90, 0, 0), Vector3.one), p.tailMat, 0);
		Graphics.DrawMesh(quad, Matrix4x4.TRS(position + (lookRot * Vector3.forward * scale.y), Quaternion.identity, Vector3.one * p.headMat.GetFloat("_Size")), p.headMat, 0);
	}
}
