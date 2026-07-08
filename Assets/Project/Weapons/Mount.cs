using System.Collections.Generic;
using UnityEngine;

public class Mount : MonoBehaviour {
	public string mountType;
	[SerializeField] Receiver receiver;
	[SerializeField] Part attached;
	[SerializeField] Mount parent;
	[SerializeField] List<Mount> mounts;

	public void Attach(Part part) {
		receiver.parts.Add(part);
		part.mount = this;
		attached = part;
		receiver.Reassemble();
	}

	public void Detach(Part part) {
		receiver.parts.Remove(part);
		part.mount = null;
		attached = null;
		receiver.Reassemble();
	}
}
