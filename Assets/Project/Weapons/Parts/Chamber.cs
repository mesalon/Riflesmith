using FMODUnity;
using UnityEngine;

public class Chamber : Part {
	public Cartridge cartridge;
	private GasBlock gas;

	public override void Reset() { }
	public override void OnAssemble(Receiver receiver) {
		gas = receiver.Find<GasBlock>();
	}

	public void Strike() {
		//muzzleFlash.Emit(1);
		//muzzleLight.enabled = true;
		ProjectileManager.CreateProjectile(new(cartridge.data, transform.position, transform.forward));
		RuntimeManager.PlayOneShot(cartridge.data.shotSound, transform.position);
		cartridge.isFired = true;
		gas?.Receive();
	}

	public void Eject(Vector3 at) {
		// todo
		cartridge = default;
	}
}
