using System.Collections.Generic;

public class Receiver : GrabInteractable {
	public List<Part> parts = new();

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
