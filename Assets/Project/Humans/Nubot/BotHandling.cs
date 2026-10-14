using UnityEngine;
using RootMotion.FinalIK;
using Animancer;

[System.Serializable] public class BotHandling {
	[SerializeField, OnObjectChanged(nameof(OnWeaponChanged))] SimpleFirearm weapon;
	[SerializeField] Transform holster;
	[SerializeField] LookAtIK lookIK;
	[SerializeField] FullBodyBipedIK IK;
	[SerializeField] AvatarMask upperMask;
	[SerializeField] ClipTransition equipWeapon, dequipWeapon;
	[SerializeField] Transform weaponContainer;
	[SerializeField] Transform weaponAimPose, weaponRestPose;
	[SerializeField] Transform grabL, grabR;
	[SerializeField] Vector3 leftElbowBend, rightElbowBend;
	[SerializeField] float aimSpeed;
	private bool isAiming;
	private Vector3 look = Vector3.forward;
	private bool isWeaponEquipped;
	private Nubot ctx;
	private AnimancerLayer upperAnim;

	public void Init(Nubot ctx) {
		this.ctx = ctx;
		upperAnim = ctx.anim.Layers[1];
		upperAnim.Mask = upperMask;

		equipWeapon.Events.SetCallbacks("Grab Weapon", () => { AttachWeaponToHand(); });
		equipWeapon.Events.OnEnd = () => {
			weapon.transform.SetParent(weaponContainer); 
			upperAnim.StartFade(0);
			isWeaponEquipped = true;
		};
		dequipWeapon.Events.OnEnd = () => {
			upperAnim.StartFade(0);
			HolsterWeapon();
		};
	}

	public void Tick() {
		lookIK.solver.IKPosition = ctx.vision.eyes.position + look * 100;
		if (Input.GetKeyDown(KeyCode.X)) { Dequip(); }
		if (Input.GetKeyDown(KeyCode.C)) { Equip(); }
		if (Input.GetMouseButtonDown(1)) { isAiming = !isAiming; }
		if (isWeaponEquipped) {
			Ext.DrawSkeleton(ctx.transform, Color.white);
			Vector3 targetPos = isAiming ? weaponAimPose.localPosition : weaponRestPose.localPosition;
			Quaternion targetRot = isAiming ? weaponAimPose.localRotation : weaponRestPose.localRotation;
			weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, targetPos, aimSpeed * Time.deltaTime);
			weapon.transform.localRotation = Quaternion.Slerp(weapon.transform.localRotation, targetRot, aimSpeed * Time.deltaTime);

			IK.solver.rightHandEffector.rotation = weapon.grip.rotation * Quaternion.Inverse(grabR.localRotation);
			IK.solver.rightHandEffector.position = weapon.grip.position - IK.solver.rightHandEffector.rotation * grabR.localPosition;
			IK.solver.leftHandEffector.rotation = weapon.foregrip.rotation * Quaternion.Inverse(grabL.localRotation);
			IK.solver.leftHandEffector.position = weapon.foregrip.position - IK.solver.leftHandEffector.rotation * grabL.localPosition;
			IK.solver.leftArmChain.bendConstraint.direction = leftElbowBend;
			IK.solver.rightArmChain.bendConstraint.direction = rightElbowBend;
			IK.solver.IKPositionWeight = 1;
		} else {
			IK.solver.IKPositionWeight = 0;
		}
	}

	public void LateTick() {
		lookIK.solver.Update();
		IK.solver.Update();
	}
	public void Look(Vector3 direction) {
		look = direction;
	}

	private void AttachWeaponToHand() {
		Transform bone = IK.solver.rightHandEffector.bone;
		weapon.transform.SetParent(bone);
		weapon.transform.localRotation = grabR.localRotation * Quaternion.Inverse(weapon.grip.localRotation);
		weapon.transform.localPosition = grabR.localPosition - weapon.transform.localRotation * weapon.grip.localPosition;
	}
	private void HolsterWeapon() {
		weapon.transform.SetParent(holster);
		weapon.transform.Reset();
	}

	public void Equip() {
		if (weapon && !upperAnim.IsPlayingClip(equipWeapon.Clip) && !isWeaponEquipped) {
			upperAnim.Play(equipWeapon);
		}
	}
	public void Dequip() {
		if (weapon && !upperAnim.IsPlayingClip(dequipWeapon.Clip) && isWeaponEquipped) {
			AttachWeaponToHand();
			isWeaponEquipped = false;
			upperAnim.Play(dequipWeapon);
		}
	}

	public void PickWeapon(SimpleFirearm f) {
		f.rb.isKinematic = true;
		HolsterWeapon();
	}
	public void DropWeapon(SimpleFirearm f) {
		f.transform.SetParent(null);
		f.rb.isKinematic = false;
	}
	void OnWeaponChanged(SimpleFirearm oldValue, SimpleFirearm newValue) {
		if (oldValue) { DropWeapon(oldValue); }
		if (newValue) { PickWeapon(newValue); }
	}

	private void HandWeapon() {
	}
}
