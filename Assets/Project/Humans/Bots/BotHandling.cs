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
	private bool isEquipped, isAiming;
	private Quaternion weaponRotation;
	private Vector3 aimDir;
	private Vector3 target;
	private float t;
	private int burst;
	private float rest;

	public BotHandling(Bot ctx) {
		this.ctx = ctx;
		ctx.equipWeapon.Events.SetCallback(0, () => {
				AttachToHand();
				});
		ctx.equipWeapon.Events.OnEnd = () => { // Todo: make these transitions nicer
			weapon.transform.SetParent(ctx.weaponContainer); 
			ctx.upperLayer.StartFade(0);
			ctx.ik.solver.IKPositionWeight = 1;
			UpdateWeapon();
			isEquipped = true;
		};

		ctx.dequipWeapon.Events.SetCallback(0, Holster);
	}

	public void FireAt(Vector3 target) {
		ADS(true);
		if (this.target != target) {
			this.target = target;
			aimDir = (TargetDir + Random.insideUnitSphere * cfg.aimError).normalized;
		}
		if (Vector3.Angle(weapon.transform.forward, aimDir) < 5 && !Physics.Raycast(weapon.muzzle.position, weapon.muzzle.forward, out var _, ProjectileManager.I.mask)) {
			if (t > rest) {
				weapon.triggerState = true;
				aimDir = (TargetDir + Random.insideUnitSphere * cfg.aimError).normalized;
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

	public void Tick() {
		if (!weapon) {
			Collider[] overlap = Physics.OverlapSphere(ctx.self.Center, 1f); // todo: optimize this

			foreach (Collider col in overlap) {
				if (col.TryGetComponent(out SimpleFirearm gun)) { 
					weapon = gun; 
					//weapon.SetDormant(true);
					ctx.ik.solver.leftHandEffector.target = weapon.foregrip;
					ctx.ik.solver.rightHandEffector.target = weapon.grip;
					ctx.brain.isArmed = true;
					Holster();
					Equip();
				}
			}
		} else if (isEquipped) {
			UpdateWeapon();
			Debug.DrawRay(weapon.muzzle.position, TargetDir * 50, Color.red);
			Debug.DrawRay(weapon.muzzle.position, aimDir * 50, Color.green);
			Debug.DrawRay(weapon.muzzle.position, weapon.transform.forward * 50, Color.purple);
		}
		//isAiming = false; // Must be constantly updated.
	}

	private void UpdateWeapon() {
		Transform weaponTarget;
		if (isAiming) {
			weaponTarget = ctx.weaponAimPose;
			Quaternion globalLookRot = Quaternion.LookRotation(aimDir != Vector3.zero ? aimDir : ctx.transform.forward);
			weaponRotation = Quaternion.Slerp(weaponRotation, globalLookRot, cfg.aimSpeed * Time.deltaTime);
			ctx.motion.Focus(target);
		} else {
			weaponTarget = ctx.weaponRestPose;
			weaponRotation = Quaternion.Slerp(weaponRotation, weaponTarget.rotation, cfg.aimSpeed * Time.deltaTime);
		}
		weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, weaponTarget.localPosition, cfg.aimSpeed * Time.deltaTime);
		weapon.transform.rotation = weaponRotation;
	}

	private void Holster() {
		weapon.transform.SetParent(ctx.weaponContainer);
		weapon.transform.SetPose(ctx.weaponHolster);
	}

	private void AttachToHand() {
		weapon.transform.SetParent(ctx.ik.solver.rightHandEffector.bone);
		Quaternion rotOffset = Quaternion.Inverse(weapon.transform.rotation) * weapon.grip.rotation;
		weapon.transform.localRotation = Quaternion.Inverse(rotOffset);
		Vector3 posOffset = weapon.transform.InverseTransformPoint(weapon.grip.position);
		weapon.transform.localPosition = -(weapon.transform.localRotation * posOffset);
	}

	public void Dequip() {
		if (isEquipped) {
			isEquipped = false;
			ctx.upperLayer.Play(ctx.dequipWeapon);
			ctx.ik.solver.IKPositionWeight = 0;
			AttachToHand();
		} 
	}

	public void Equip() {
		if (!isEquipped) {
			ctx.upperLayer.Play(ctx.equipWeapon);
		}
	}

	public void ADS(bool state) { isAiming = state; }
}
