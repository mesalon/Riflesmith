using UnityEngine;

[System.Serializable]
public struct BodyCfg {
	public float strength;
	public float recovery;
	public float recoveryDelay;
	public float ragdollStrength;
	public float ragdollDamper;
	public float ragdollSpeed;

	public static readonly BodyCfg Default = new() {
		strength = 100,
		recovery = 10,
		recoveryDelay = 8,
	};
}

public class EnemyBody {
	public bool isUp { get; private set; } = true;
	public float strength;
	private BodyCfg cfg => ctx.cfg.body;
	private readonly Enemy ctx;
	private Vector3[] axes;
	private float hitTime;
	private readonly float seed;

	public EnemyBody(Enemy ctx) {
		this.ctx = ctx;
		seed = ctx.transform.GetInstanceID();
		strength = cfg.strength;
		axes = new Vector3[ctx.joints.Count];
		for (int i = 0; i < ctx.joints.Count; i++)
			axes[i] = Random.onUnitSphere;
		SetForce(1);
	}

	public void Tick() {
		float regenRate = Mathf.Lerp(0, cfg.recovery, hitTime - cfg.recoveryDelay / 10);
		strength = Mathf.Min(100, strength + regenRate * (ctx.health / 100) * Time.deltaTime);
		SetForce(Mathf.Min(strength, ctx.health) / 100);

		if (!isUp && strength >= 100) { // Get up
			SetRagdoll(false);
			Recenter();
		}

		if (isUp && strength <= 40) { SetRagdoll(true); }

		if (ctx.health <= 0) {
			SetRagdoll(true);
			SetForce(0);
		}

		for (int i = 0; i < ctx.joints.Count; i++) { // todo, fix the animations and remove the random spinning
			ctx.joints[i].targetRotation = Quaternion.Euler(
					Mathf.PerlinNoise(Time.time * cfg.ragdollSpeed + i + seed, 0) * 360,
					Mathf.PerlinNoise(0, Time.time * cfg.ragdollSpeed + i + seed) * 360,
					Mathf.PerlinNoise(Time.time * cfg.ragdollSpeed + i * 10 + seed, Time.time * cfg.ragdollSpeed + i) * 360
					);
		}

		hitTime += Time.deltaTime;
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
		drive.positionSpring = scale * cfg.ragdollStrength;
		drive.positionDamper = scale * cfg.ragdollDamper;
		foreach (ConfigurableJoint j in ctx.joints) {
			j.angularXDrive = drive;
			j.angularYZDrive = drive;
		}
	}

	private void Recenter() {
		ctx.transform.position = ctx.coreRag.position;
		if (Physics.Raycast(ctx.coreRag.position, Vector3.down, out RaycastHit hitInfo, LayerMask.NameToLayer("Environment"))) {
			ctx.transform.position = new Vector3(ctx.transform.position.x, hitInfo.point.y, ctx.transform.position.z);
		}
		ctx.coreRag.position = Vector3.zero;
	}
}
