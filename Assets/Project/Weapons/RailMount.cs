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

	public Vector3 GetAttachPoint(Vector3 center) {
		Vector3 a = start.localPosition;
		Vector3 b = end.localPosition;
		Vector3 c = transform.InverseTransformPoint(center);

		Vector3 ab = b - a;
		float len = ab.magnitude;
		Vector3 dir = ab / len;
		float t = Vector3.Dot(c - a, dir);
		t = Mathf.Clamp(t, 0f, len);
		return a + dir * t;
	}

	public override void Register() {
		if (receiver) {
			foreach (RailPart part in attached) {
				if (!receiver.parts.Contains(part)) receiver.parts.Add(part);
				foreach (Mount m in part.children) {
					m.receiver = receiver;
					m.Register();
				}
			}
		}
	}

	public override void Deregister() {
		if (receiver) {
			foreach (RailPart part in attached) {
				receiver.parts.Remove(part);
				foreach (Mount m in part.children) {
					m.Deregister();
					m.receiver = null;
				}
			}
		}
	}
}
