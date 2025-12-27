using UnityEngine;

public class EnemyHandling {
	private LocomotionCfg cfg => ctx.board.cfg.locomotion;
	private Blackboard board => ctx.board;
	private Vector3 TargetDir => (target - board.weapon.transform.position).normalized;
	private readonly Enemy ctx;
	private bool isAiming;
	private Quaternion weaponRotation;
	private Vector3 aimDir;
	private Vector3 target;

	public EnemyHandling(Enemy ctx) {
		this.ctx = ctx;
		weaponRotation = board.weapon.transform.rotation;
	}

	public void FireAt(Vector3 target) {
		Debug.Log("Firing!");
		ADS(true);
		if (this.target != target) {
			this.target = target;
			aimDir = (TargetDir + Random.insideUnitSphere * board.aimError).normalized;
		}
		if (Vector3.Angle(board.weapon.transform.forward, aimDir) < 5 && !Physics.Linecast(board.weapon.muzzle.position, target, out var _, ProjectileManager.I.mask)) {
			if (t > rest) {
				board.weapon.FireOnce();
				aimDir = (TargetDir + Random.insideUnitSphere * board.aimError).normalized;
				//board.confidence += 0.025f;
				t -= 0.25f + 0.15f * (0.5f - Random.value);
				burst++;
				if (burst == 3) { 
					t = burst = 0; 
					rest = 0.75f + (0.5f - Random.value) * 0.75f;
				}
			}
		}
		t += Time.deltaTime;
	}

	float t;
	int burst;
	float rest;
	public void Tick() {
		Debug.DrawRay(board.weapon.muzzle.position, TargetDir * 50, Color.red);
		Debug.DrawRay(board.weapon.muzzle.position, aimDir * 50, Color.green);
		Debug.DrawRay(board.weapon.muzzle.position, board.weapon.transform.forward * 50, Color.purple);
		Transform weaponTarget = isAiming ? board.weaponAimPose : board.weaponRestPose;
		board.weapon.transform.localPosition = Vector3.Lerp(board.weapon.transform.localPosition, weaponTarget.localPosition, cfg.aimSpeed * Time.deltaTime);
		if (isAiming) {
			Quaternion globalLookRot = Quaternion.LookRotation(aimDir != Vector3.zero ? aimDir : ctx.transform.forward);
			weaponRotation = Quaternion.Slerp(weaponRotation, globalLookRot, cfg.aimSpeed * Time.deltaTime);
			ctx.locomotion.Focus(target);
		} else {
			weaponRotation = Quaternion.Slerp(weaponRotation, weaponTarget.rotation, cfg.aimSpeed * Time.deltaTime);
		}
		board.weapon.transform.rotation = weaponRotation;
	}

	public void ADS(bool state) { isAiming = state; }
}
