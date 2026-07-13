public class FixedMount : Mount {
	public FixedPart attached;

	public override void Register() {
		if (receiver) {
			if (!receiver.parts.Contains(attached)) receiver.parts.Add(attached);
			foreach (Mount m in attached.children) {
				m.receiver = receiver;
				m.Register();
			}
		}
	}

	public override void Deregister() {
		if (attached && receiver) {
			receiver.parts.Remove(attached);
			foreach (Mount m in attached.children) {
				m.Deregister();
				m.receiver = null;
			}
		}
	}
}
