using System.Collections.Generic;
using UnityEngine;

public class Receiver : MonoBehaviour {
	public List<Part> parts = new();

	public void Reassemble() {
		foreach (Part part in parts) { part.OnReset(); }
		foreach (Part part in parts) { part.OnAssemble(this); }
	}

	public T Find<T>() {
		foreach (Part part in parts) {
			if (part is T found) { return found; }
		}
		return default;
	}

}
