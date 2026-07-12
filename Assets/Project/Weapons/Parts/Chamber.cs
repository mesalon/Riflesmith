using FMODUnity;
using UnityEngine;

public class Chamber : FixedPart {
	public Cartridge cartridge;
	private Barrel barrel;
	private GasBlock gas;

	public override void OnReset() {
		gas = null;
		barrel = null;
	}
	public override void OnAssemble(Receiver receiver) {
		gas = receiver.Find<GasBlock>();
		barrel = receiver.Find<Barrel>();
	}

	public void Strike() {
		//muzzleFlash.Emit(1);
		//muzzleLight.enabled = true;
		float speed = barrel ? Mathf.Lerp(cartridge.data.minSpeed, cartridge.data.maxSpeed, barrel.length) : 5;
		Projectile p = new(cartridge.data, transform.position, transform.forward, speed);
		ProjectileManager.CreateProjectile(p);
		RuntimeManager.PlayOneShot(cartridge.data.shotSound, transform.position);
		cartridge.isFired = true;
		gas?.Receive();
	}

	public void Eject(Vector3 at) {
		// todo
		cartridge = default;
	}
}
