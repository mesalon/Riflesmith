public class GasBlock : Part {
	private Bolt bolt;

	public override void Reset() { 
		bolt = null;
	}
	public override void OnAssemble(Receiver receiver) {
		bolt = receiver.Find<Bolt>();
	}

	public void Receive() {
		bolt.DeliverForce();
	}
}
