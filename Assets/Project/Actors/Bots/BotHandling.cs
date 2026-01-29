using UnityEngine;

[System.Serializable] public struct HandlingCfg {
	public float aimError, fixAmount, fixVariance;
	public float aimSpeed;
}

public class BotHandling {
	public SimpleFirearm weapon;
	private HandlingCfg cfg => ctx.cfg.handling;
	private Vector3 TargetDir => (target - weapon.transform.position).normalized;
	private readonly Bot ctx;
	private bool isAiming;
	private Quaternion weaponRotation;
	private Vector3 aimDir;
	private Vector3 target;

	public BotHandling(Bot ctx) {
		this.ctx = ctx;
		//weaponRotation = weapon.transform.rotation;
	}

	public void FireAt(Vector3 target) {
		Debug.Log("Firing!");
		ADS(true);
		if (this.target != target) {
			this.target = target;
			aimDir = (TargetDir + Random.insideUnitSphere * cfg.aimError).normalized;
		}
		if (Vector3.Angle(weapon.transform.forward, aimDir) < 5 && !Physics.Linecast(weapon.muzzle.position, target, out var _, ProjectileManager.I.mask)) {
			if (t > rest) {
				weapon.triggerState = true;
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
		ctx.brain.isArmed = weapon;
		if (!weapon) return;

		Debug.DrawRay(weapon.muzzle.position, TargetDir * 50, Color.red);
		Debug.DrawRay(weapon.muzzle.position, aimDir * 50, Color.green);
		Debug.DrawRay(weapon.muzzle.position, weapon.transform.forward * 50, Color.purple);
		Transform weaponTarget = isAiming ? ctx.weaponAimPose : ctx.weaponRestPose;
		weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, weaponTarget.localPosition, cfg.aimSpeed * Time.deltaTime);
		if (isAiming) {
			Quaternion globalLookRot = Quaternion.LookRotation(aimDir != Vector3.zero ? aimDir : ctx.transform.forward);
			weaponRotation = Quaternion.Slerp(weaponRotation, globalLookRot, cfg.aimSpeed * Time.deltaTime);
			ctx.motionController.Focus(target);
		} else {
			weaponRotation = Quaternion.Slerp(weaponRotation, weaponTarget.rotation, cfg.aimSpeed * Time.deltaTime);
		}
		weapon.transform.rotation = weaponRotation;
	}

	public void ADS(bool state) { isAiming = state; }
}
