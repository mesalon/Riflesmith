using System;
using UnityEngine;

public enum Side { Left, Right }
public class Hand : MonoBehaviour {
    public Hand other;
    public PlayerInput Input;
    public PlayerInput LastInput;

    public Transform holdPoint;
    public IInteractable held;
    [SerializeField] Side side;
    [SerializeField] GameObject vis;
    [SerializeField] float grabThresh = 0.5f;
    [SerializeField] bool hideOnGrab;

    public void Tick() {
        if (Input.grip.DidPass(grabThresh, LastInput.grip)) {
            Collider[] overlap = Physics.OverlapSphere(holdPoint.position, 0.01f);
            foreach (Collider col in overlap) {
                if (held == null && col.TryGetComponent(out IInteractable interactable)) { Pick(interactable); }
            }
        }
        else if (held != null && Input.grip > grabThresh) { held.OnHold(); } 
        else if (held != null && Input.grip.DidFail(grabThresh, LastInput.grip)) { Drop(); }
    }

    public void FixedTick() {
        if (held != null) { held.OnHoldFixed(); }
    }

    public void Drop() {
        if (held != null) {
            vis.SetActive(true);
            held.OnDropped();
            held.Interactor = null;
            held = null;
        }
    }

    public void Pick(IInteractable interactable) {
        if (!interactable.PreventInteraction) {
            if (interactable.Interactor) interactable.Interactor.Drop(); // Release if other controller is holding it
            held = interactable;
            held.Interactor = this;
            held.OnPicked();
            vis.SetActive(!hideOnGrab);
        }
    }
}

public struct PlayerInput {
    public Vector2 stick;
    public bool stickButton;
    public float trigger;
    public float grip;
    public bool farButton;
    public bool nearButton;
    
}