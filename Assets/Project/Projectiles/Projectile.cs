using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile {
	private Vector3 position;
	private Vector3 lastPosition;
	Vector3 velocity;

	public ProjectileData data;

	public Projectile(ProjectileData data, Vector3 position, Vector3 direction, float? barrelLength = null) {
		this.data = data;
		this.position = position;
		float t = barrelLength != null ? Mathf.InverseLerp(0, data.maxBarrelLength, (float)barrelLength) : 0.5f;
		velocity = direction.normalized * Mathf.Lerp(data.minSpeed, data.maxSpeed, t);
	}

	public void Tick(ProjectileManager p, out bool destroyProjectile) {
		destroyProjectile = false;
		lastPosition = position;
		// Apply forces
		velocity += Physics.gravity * Time.deltaTime;
		position += velocity * Time.deltaTime;

		// Debug tracers
		if(p.showDebugTracers) {
			float time = p.debugTracerTime == 0 ? Time.deltaTime : p.debugTracerTime; // Time should be for one tick if set to zero
			Color col = Physics.Linecast(lastPosition, position, out _) ? Color.red : p.debugTracerColor; // Make the tracer red if it hits
			Debug.DrawLine(lastPosition, position, col, time);
		}

		Vector3 end = position - lastPosition;
		RaycastHit[] hits = Physics.RaycastAll(lastPosition, end, end.magnitude, p.mask);
		Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
		List<GearItem> gearsHit = new();
		float damage = data.damage;
		
		foreach (RaycastHit hit in hits) {
			// if (hit.transform.TryGetComponent(out Player player)) { todo: uncomment and fix
			// 	player.Damage(data.damage);
			// }
			// GearItem gear;
			// if ((gear = hit.transform.GetComponentInParent<GearItem>()) != null && !gearsHit.Contains(gear)) {
			//  damage *= gear.damageMultiplier;
			// 	gearsHit.Add(gear);
			// 	//Debug.Log($"{hit.transform.name} was hit at a normal of {Vector3.Dot(end.normalized, hit.normal)}. New damage is {damage}");
			// }
			if (hit.transform.TryGetComponent(out Limb limb)) { limb.enemy.body.Damage(damage * limb.damageMultiplier, damage * limb.damageMultiplier * 2); }
			if (hit.transform.TryGetComponent(out Rigidbody rb)) { rb.AddForceAtPosition(velocity.normalized * data.force, hit.point); }
			break;
		}

		if (hits.Length > 0) {
			if (data.isExplosive) {
				p.CreateFX(data.boom, hits[0].point, hits[0].normal);
				foreach (Collider col in Physics.OverlapSphere(hits[0].point, data.explosiveRange)) {
					if (col.TryGetComponent(out Rigidbody rb)) { rb.AddForceAtPosition(hits[0].normal * data.explosiveForce, hits[0].point); }

					if (col.TryGetComponent(out Limb limb)) {
						limb.enemy.body.Damage(data.explosiveDamage * Mathf.Lerp(data.explosiveDamage, 0, Vector3.Distance(hits[0].point, limb.transform.position) / data.explosiveRange), 500);
						Debug.Log($"{limb.name}: {Mathf.Lerp(data.explosiveDamage, 0, Vector3.Distance(hits[0].point, limb.transform.position) / data.explosiveRange)}");
					}
					
				}
			}
			p.CreateFX(p.concreteFX, hits[0].point, hits[0].normal);
			destroyProjectile = true;
		}
	}
	
	public void Draw(ProjectileManager p) {
		Mesh quad = new() {
			vertices = new Vector3[] { new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(-0.5f, 0.5f, 0), new(0.5f, 0.5f, 0)   },
			uv = new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) },
			triangles = new int[] { 0, 2, 1, 2, 3, 1 },
		};
		Graphics.DrawMesh(quad, Matrix4x4.TRS(position, Quaternion.LookRotation(velocity), new(5, 0.2f, 1)), p.tailMat, 0);
		Graphics.DrawMesh(quad, Matrix4x4.TRS(position + velocity.normalized * 2.5f, Quaternion.LookRotation(velocity), Vector3.one * 0.2f), p.headMat, 0);
	}
}
