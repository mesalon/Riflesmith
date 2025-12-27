using UnityEngine;

[System.Serializable]
public struct BodyCfg {
	public float health;
	public float strength;
	public float recovery;
	public float recoveryDelay;
	public float ragdollStrength;
	public float ragdollDamper;
	public float ragdollSpeed;

	public static readonly BodyCfg Default = new() {
		health = 100,
		strength = 100,
		recovery = 10,
		recoveryDelay = 8,
	};
}

public class EnemyBody {
	public float Health { get; private set; }
	public bool isUp { get; private set; } = true;
	private Blackboard board => ctx.board;
	private BodyCfg cfg => board.cfg.body;
	private readonly Enemy ctx;
	private Vector3[] axes;
	private readonly float seed;
	private float bleeding;
	private float hitTime;
	private float strength;

	public EnemyBody(Enemy ctx) {
		this.ctx = ctx;
		seed = ctx.transform.GetInstanceID();
		Health = cfg.health;
		strength = cfg.strength;
		axes = new Vector3[board.joints.Count];
		for (int i = 0; i < board.joints.Count; i++)
			axes[i] = Random.onUnitSphere;
		SetForce(1);
	}

	public void Tick() {
		float regenRate = Mathf.Lerp(0, cfg.recovery, hitTime - cfg.recoveryDelay / 10);
		strength = Mathf.Min(100, strength + regenRate * (Health / 100) * Time.deltaTime);
		bleeding = Mathf.Max(0, bleeding - bleeding * 0.1f * Time.deltaTime);
		SetForce(Mathf.Min(strength, Health) / 100);
		Health = Mathf.Max(Health - bleeding * Time.deltaTime, 0);

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

		for (int i = 0; i < board.joints.Count; i++) { // todo, fix the animations and remove the random spinning
			board.joints[i].targetRotation = Quaternion.Euler(
					Mathf.PerlinNoise(Time.time * cfg.ragdollSpeed + i + seed, 0) * 360,
					Mathf.PerlinNoise(0, Time.time * cfg.ragdollSpeed + i + seed) * 360,
					Mathf.PerlinNoise(Time.time * cfg.ragdollSpeed + i * 10 + seed, Time.time * cfg.ragdollSpeed + i) * 360
					);
		}

		hitTime += Time.deltaTime;
	}

	public void Damage(float amount, float force) {
		Health = Mathf.Max(0, Health - amount);
		strength = Mathf.Max(0, strength - force);
		bleeding += amount * 0.1f;
		hitTime = 0;
	}

	public void SetRagdoll(bool state) {
		isUp = !state;
		board.anim.enabled = !state;
		board.weapon.enabled = !state;
		foreach (ConfigurableJoint j in board.joints) {
			Rigidbody rag = j.GetComponent<Rigidbody>();
			rag.isKinematic = !state;
			if (!rag.isKinematic) {
				rag.angularVelocity = Vector3.zero;
				rag.linearVelocity = Vector3.zero;
			}
		}
	}

	public void SetForce(float scale) {
		JointDrive drive = board.joints[0].angularXDrive;
		drive.positionSpring = scale * cfg.ragdollStrength;
		drive.positionDamper = scale * cfg.ragdollDamper;
		foreach (ConfigurableJoint j in board.joints) {
			j.angularXDrive = drive;
			j.angularYZDrive = drive;
		}
	}

	private void Recenter() {
		Vector3 original = board.coreRag.position;
		ctx.transform.position = board.coreRag.position;
		if (Physics.Raycast(board.coreRag.position, Vector3.down, out RaycastHit hitInfo, LayerMask.NameToLayer("Environment"))) {
			ctx.transform.position = new Vector3(ctx.transform.position.x, hitInfo.point.y, ctx.transform.position.z);
		}
		board.coreRag.position = Vector3.zero;
	}
}
