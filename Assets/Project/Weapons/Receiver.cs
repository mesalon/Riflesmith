using System.Collections.Generic;
using UnityEngine;

public class Receiver : MonoBehaviour {
	public List<Part> parts = new();

	void Update() {
		foreach (Part part in parts) {
			part.input = default;
			part.input = new DeviceInput() {
				trigger = Input.GetKey(KeyCode.T) ? 1 : 0,
				grip = Input.GetKey(KeyCode.G) ? 1 : 0,
			};
		}
	}

	public void Reassemble() {
		print($"Reassembling receiver with parts: {string.Join(", ", parts)}");
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
