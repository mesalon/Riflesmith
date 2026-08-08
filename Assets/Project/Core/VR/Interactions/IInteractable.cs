using UnityEngine;
using System;

public abstract class Interactable : MonoBehaviour {
	public Rigidbody rb;
	public Hand hand;
	public Action OnPickedE, OnHoldE, OnHoldFixedE, OnDroppedE;
	public virtual void OnPicked() { OnPickedE?.Invoke(); }
	public virtual void OnHold() { OnHoldE?.Invoke(); }
	public virtual void OnHoldFixed() { OnHoldFixedE?.Invoke(); }
	public virtual void OnDropped() { OnDroppedE?.Invoke(); }

	private void Awake() {
		if (!rb) rb = GetComponent<Rigidbody>(); 
	}
}
