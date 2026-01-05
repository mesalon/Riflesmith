using FMODUnity;
using UnityEngine;

[CreateAssetMenu()]
public class ProjectileData : ScriptableObject {
	public EventReference shotSound;	
	public Material tailMat, headMat;
	public Casing casing;
	public float minSpeed, maxSpeed;
	public float damage;
	public float force;
	public string caliber;
}
