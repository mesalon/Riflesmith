public class GasBlock : FixedPart {
	private Bolt bolt;

	public override void OnReset() { 
		bolt = null;
	}
	public override void OnAssemble(Receiver receiver) {
		bolt = receiver.Find<Bolt>();
	}

	public void Receive() {
		bolt.DeliverForce();
	}
}
