using UnityEngine;

public class Stock : Part {
	public override void OnAssemble(Receiver receiver) {
		print("Stock attached");
	}

	public override void Reset() { }
}
