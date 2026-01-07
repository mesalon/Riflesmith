using System.Collections.Generic;
using UnityEngine;

public class Receiver : MonoBehaviour {
	public List<Mount> mounts;
	public List<Part> parts;

	public void Reassemble() {
		foreach (Part part in parts) { part.Reset(); }
		foreach (Part part in parts) { part.OnAssemble(this); }
	}

	public T Find<T>() {
		foreach (Part part in parts) {
			if (part is T found) { return found; }
		}
		return default;
	}
}
