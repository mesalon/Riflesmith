public class RecoilSpring : FixedPart {
	private float cyclicRate;
	private Bolt bolt;

	public override void OnReset() {
		bolt = null;
	}
	public override void OnAssemble(Receiver receiver) {
		bolt = receiver.Find<Bolt>();
		bolt.conf.hasSpring = true;
	}
}
