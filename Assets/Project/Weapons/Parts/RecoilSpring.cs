public class RecoilSpring : Part {
	private float cyclicRate;
	private Bolt bolt;

	public override void Reset() { }
	public override void OnAssemble(Receiver receiver) {
		bolt = receiver.Find<Bolt>();
		bolt.conf.hasSpring = true;
	}
}
