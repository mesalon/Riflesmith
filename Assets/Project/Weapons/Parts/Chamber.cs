using FMODUnity;
using UnityEngine;

public class Chamber : BasicPart {
	public Cartridge cartridge;
	private GasBlock gas;
	[SerializeField] float energyConversion;
	[SerializeField] Transform roundT;
	public override void OnAssemble(Receiver receiver) {
		gas = receiver.Find<GasBlock>();
	}

	public void Strike() {
		if (cartridge.data) {
			CartridgeData data = cartridge.data;
			float energy = data.energy * energyConversion;
			float speed = Mathf.Sqrt(2 * energy / data.bulletMass);
			if (gas) gas.Receive(energy);
			ProjectileManager.CreateProjectile(new() { data = data, position = transform.position, velocity = transform.forward * speed });
			RuntimeManager.PlayOneShot(cartridge.data.shotSound, transform.position);
			cartridge.isFired = true;
		}
	}

	void Update() {
		Ext.Label(transform.position, cartridge.data ? $"Chamber: {cartridge.data}" : "");
		if (cartridge.data) cartridge.Render(roundT.localToWorldMatrix);
	}
}
