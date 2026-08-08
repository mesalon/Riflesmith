using UnityEngine;

public class GasBlock : FixedPart {
	private Bolt bolt;
	private float gas;
	[SerializeField] float gasTake;

	public override void OnAssemble(Receiver receiver) {
		bolt = receiver.Find<Bolt>();
	}

	public void Receive(float energy) {
		gas += gasTake * energy;
	}
}
