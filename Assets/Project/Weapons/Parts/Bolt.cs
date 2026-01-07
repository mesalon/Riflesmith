using UnityEngine;
using System;

public class Bolt : Part {
	[Serializable] public struct Config {
		public float cyclicRate;
		public bool hasSpring;
	}
	public Config conf;
	[SerializeField] Config baseConf;

	public bool boltLockActive;
	[SerializeField] Transform ejectPoint;

	private Chamber chamber;
	private FireControlGroup fcg;
	private IAmmoSource ammo;
	private bool boltLocked;
	
	public override void Reset() {
		conf = baseConf;
	}
	public override void OnAssemble(Receiver receiver) {
		ammo = receiver.Find<IAmmoSource>();
		chamber = receiver.Find<Chamber>();
		fcg = receiver.Find<FireControlGroup>();
	}

	public void DeliverForce() {
		if(Interactor) Interactor.Drop();
		chamber.Eject(ejectPoint.position);
		fcg.ResetHammer();
		if (conf.hasSpring) {
			chamber.cartridge = ammo.Strip();
			boltLocked = boltLockActive;
		}
	}
}
