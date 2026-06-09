using UnityEngine;

public abstract class Interactor : MonoBehaviour {
	public Transform grabPoint;
	public Rigidbody rb;
	public IInteractable held;

	public virtual void Update() {
		if (held != null) { held.OnHold(); } 
	}
	public virtual void FixedUpdate() {
		if (held != null) { held.OnHoldFixed(); }
	}
	public virtual void Pick(IInteractable interactable) {
		if (!interactable.PreventInteraction) {
			if (interactable.Interactor) interactable.Interactor.Drop(); // Release if something else is holding it
			held = interactable;
			held.Interactor = this;
			held.OnPicked();
		}
	}
	public virtual void Drop() {
		if (held != null) {
			held.OnDropped();
			held.Interactor = null;
			held = null;
		}
	}
}
