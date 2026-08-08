using UnityEngine;

public class TwoHandGrab : Interactable {
	private Hand Other => hand ? hand.other : null;


	public override void OnHoldFixed() {
		base.OnHoldFixed();
	}

}
