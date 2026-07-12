using System.Collections.Generic;
using UnityEngine;

public class RailMount : Mount {
	public List<RailPart> attached = new();
	public Transform start, end;

	public bool TryAttach(RailPart part, out Vector3 pos) {
		pos = Vector3.zero;
		if (part.mountType == mountType) {
			pos = GetAttachPoint(part.transform.position);
			return true;
		} 
		return false;
	}

	public override void Register() {
		print($"Registering {this} to {mount}, receiver is {Receiver}");
		if (!Receiver.parts.Contains(this)) Receiver.parts.Add(this);
		foreach (Mount m in children) {
			m.receiver = Receiver;

			if (m.attached) { m.attached.Register(); }
		}
	}

	private void Deregister() {
		print($"Deregistering {this} from {mount}, receiver is {Receiver}");
		if (mount) {
			Receiver.parts.Remove(this);
			foreach (Mount m in children) {
				if (m.attached) { m.attached.Deregister(); }
				m.receiver = null;
			}
		}
	}

	public override Vector3 GetAttachPoint(Vector3 center) {
		Vector3 a = end.position - center;
		Vector3 b = end.position - start.position;
		return Vector3.Lerp(start.position, end.position, Vector3.Dot(b, a));
	}
}
