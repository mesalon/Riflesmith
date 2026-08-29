using UnityEngine;

public class Grip : FixedPart {
	[SerializeField] FireControlGroup fcg;
	private GrabInteractable gi;

	void Awake() {
		gi = GetComponent<GrabInteractable>();
		gi.OnHold += OnHold;
	}

	public override void OnAssemble(Receiver receiver) {
		fcg = receiver.Find<FireControlGroup>();
	}

	private void OnHold() {
		//fcg.input = gi.Input;
	}
}
