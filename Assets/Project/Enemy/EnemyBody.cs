using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BodyCfg {
	public float health;
	public float strength;
	public float recoveryMin;
	public float recoveryMax;
	public float recoveryDelay;
	public float force;
	public float damper;
	public float speed;
}

public class EnemyBody {
	public float Health { get; private set; }
	public bool isUp { get; private set; } = true;
	private readonly Blackboard ctx;
	private BodyCfg cfg => ctx.cfg.body;
	private Vector3[] axes;
	private readonly float seed;
	private float bleeding;
	private float hitTime;
	private float strength;

	public EnemyBody(Blackboard ctx) {
		this.ctx = ctx;
		seed = ctx.transform.GetInstanceID();
		Health = cfg.health;
		strength = cfg.strength;
		axes = new Vector3[ctx.joints.Count];
		for (int i = 0; i < ctx.joints.Count; i++)
			axes[i] = Random.onUnitSphere;
		SetForce(1);
	}

	public void Tick() {
		float regenRate = Mathf.Lerp(cfg.recoveryMin, cfg.recoveryMax, hitTime - cfg.recoveryDelay / 10);
		strength = Mathf.Min(100, strength + regenRate * (Health / 100) * Time.deltaTime);
		bleeding = Mathf.Max(0, bleeding - bleeding * 0.1f * Time.deltaTime);
		SetForce(Mathf.Min(strength, Health) / 100);
		Health = Mathf.Clamp(Health - bleeding * Time.deltaTime, 0, 100);

		if (!isUp && strength >= 100) { // Get up
			bleeding = 0;
			SetRagdoll(false);
			Recenter();
		}

		if (isUp && strength <= 40) { SetRagdoll(true); }

		if (Health <= 0) { // You are dead.
			SetRagdoll(true);
			SetForce(0);
		}

		for (int i = 0; i < ctx.joints.Count; i++) { // todo, fix the animations and remove the random spinning
			ctx.joints[i].targetRotation = Quaternion.Euler(
					Mathf.PerlinNoise(Time.time * cfg.speed + i + seed, 0) * 360,
					Mathf.PerlinNoise(0, Time.time * cfg.speed + i + seed) * 360,
					Mathf.PerlinNoise(Time.time * cfg.speed + i * 10 + seed, Time.time * cfg.speed + i) * 360
					);
		}

		hitTime += Time.deltaTime;
	}

	public void Damage(float amount, float force) {
		Debug.Log("Hello");
		Health = Mathf.Max(0, Health - amount);
		strength = Mathf.Max(0, strength - force);
		bleeding += amount * 0.1f;
		hitTime = 0;
	}

	public void SetRagdoll(bool state) {
		isUp = !state;
		ctx.anim.enabled = !state;
		ctx.weapon.enabled = !state;
		foreach (ConfigurableJoint j in ctx.joints) {
			Rigidbody rag = j.GetComponent<Rigidbody>();
			rag.isKinematic = !state;
			if (!rag.isKinematic) {
				rag.angularVelocity = Vector3.zero;
				rag.linearVelocity = Vector3.zero;
			}
		}
	}

	public void SetForce(float scale) {
		JointDrive drive = ctx.joints[0].angularXDrive;
		drive.positionSpring = scale * cfg.force;
		drive.positionDamper = scale * cfg.damper;
		foreach (ConfigurableJoint j in ctx.joints) {
			j.angularXDrive = drive;
			j.angularYZDrive = drive;
		}
	}

	private void Recenter() {
		Vector3 original = ctx.coreRag.position;
		ctx.transform.position = ctx.coreRag.position;
		if (Physics.Raycast(ctx.coreRag.position, Vector3.down, out RaycastHit hitInfo, LayerMask.NameToLayer("Environment"))) {
			ctx.transform.position = new Vector3(ctx.transform.position.x, hitInfo.point.y, ctx.transform.position.z);
		}
		ctx.coreRag.position = Vector3.zero;
	}
}
