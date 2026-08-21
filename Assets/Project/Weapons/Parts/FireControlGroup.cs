using UnityEngine;

public class FireControlGroup : BasicPart {
	public DeviceInput input;
	[SerializeField] Chamber chamber;
	[SerializeField] float triggerThreshold;
	[SerializeField] bool isFullAuto;
	public bool hammerState;
	public bool hammerLocked;
	private bool disconnectorState;

	public override void OnAssemble(Receiver receiver) {
		chamber = receiver.Find<Chamber>();
	}

	private void Update() {
		print($"FCG: Trigger: {input.trigger}. Hammer: {hammerState}. Lock: {hammerLocked}. Disconnector: {disconnectorState}");
		if (input.trigger > triggerThreshold && hammerState && !hammerLocked && !disconnectorState) {
			disconnectorState = !isFullAuto;
			chamber.Strike();
			hammerState = false;
		}
		if (input.trigger <= triggerThreshold) { disconnectorState = false; }
	}
}
