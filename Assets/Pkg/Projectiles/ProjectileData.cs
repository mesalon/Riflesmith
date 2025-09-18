using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile", menuName = "Items/New Projectile")]
public class ProjectileData : ScriptableObject {
	public Mesh tracer;
	public Material tracerMat;
	public float minSpeed, maxSpeed;
	public float damage;
	public float force;
	public float maxBarrelLength;

	public EventReference shotSound;	
	public string caliber;
	public Casing casing;
	public Mesh mesh;
	public Material mat;
	public Vector3 scale;

	public bool isExplosive;
	public float explosiveForce;
	public float explosiveDamage;
	public float explosiveRange;
	public GameObject boom;
}