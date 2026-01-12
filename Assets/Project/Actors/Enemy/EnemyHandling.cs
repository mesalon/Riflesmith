using UnityEngine;

[System.Serializable] public struct HandlingCfg {
	public float aimError, fixAmount, fixVariance;
	public float aimSpeed;
}

public class EnemyHandling {
	private HandlingCfg cfg => ctx.cfg.handling;
	private Vector3 TargetDir => (target - ctx.weapon.transform.position).normalized;
	private readonly Enemy ctx;
	private bool isAiming;
	private Quaternion weaponRotation;
	private Vector3 aimDir;
	private Vector3 target;

	public EnemyHandling(Enemy ctx) {
		this.ctx = ctx;
		weaponRotation = ctx.weapon.transform.rotation;
	}

	public void FireAt(Vector3 target) {
		Debug.Log("Firing!");
		ADS(true);
		if (this.target != target) {
			this.target = target;
			aimDir = (TargetDir + Random.insideUnitSphere * cfg.aimError).normalized;
		}
		if (Vector3.Angle(ctx.weapon.transform.forward, aimDir) < 5 && !Physics.Linecast(ctx.weapon.muzzle.position, target, out var _, ProjectileManager.I.mask)) {
			if (t > rest) {
				ctx.weapon.triggerState = true;
				aimDir = (TargetDir + Random.insideUnitSphere * cfg.aimError).normalized;
				//ctx.confidence += 0.025f;
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
		Debug.DrawRay(ctx.weapon.muzzle.position, TargetDir * 50, Color.red);
		Debug.DrawRay(ctx.weapon.muzzle.position, aimDir * 50, Color.green);
		Debug.DrawRay(ctx.weapon.muzzle.position, ctx.weapon.transform.forward * 50, Color.purple);
		Transform weaponTarget = isAiming ? ctx.weaponAimPose : ctx.weaponRestPose;
		ctx.weapon.transform.localPosition = Vector3.Lerp(ctx.weapon.transform.localPosition, weaponTarget.localPosition, cfg.aimSpeed * Time.deltaTime);
		if (isAiming) {
			Quaternion globalLookRot = Quaternion.LookRotation(aimDir != Vector3.zero ? aimDir : ctx.transform.forward);
			weaponRotation = Quaternion.Slerp(weaponRotation, globalLookRot, cfg.aimSpeed * Time.deltaTime);
			ctx.motionController.Focus(target);
		} else {
			weaponRotation = Quaternion.Slerp(weaponRotation, weaponTarget.rotation, cfg.aimSpeed * Time.deltaTime);
		}
		ctx.weapon.transform.rotation = weaponRotation;
	}

	public void ADS(bool state) { isAiming = state; }
}
