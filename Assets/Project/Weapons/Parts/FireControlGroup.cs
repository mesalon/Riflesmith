using UnityEngine;
using System;

public class FireControlGroup : Part {
	[Serializable] public struct Config {
		public float triggerThreshold;
		public bool isFullAuto;
	}
	public Config conf;
	[SerializeField] Config baseConf;
	private float triggerState;
	private bool hammerState;
	private bool hammerLocked;
	private bool disconnectorState;
	private Chamber chamber;

	public override void Reset() {
		conf = baseConf;
	}
	public override void OnAssemble(Receiver receiver) {
		chamber = receiver.Find<Chamber>();
	}

	public void ResetHammer() {
		hammerState = true;
	}

	private void Update() {
		if (triggerState > conf.triggerThreshold && hammerState && !hammerLocked && !disconnectorState) {
			disconnectorState = !conf.isFullAuto;
			chamber.Strike();
			hammerState = false;
		}
	}
}
