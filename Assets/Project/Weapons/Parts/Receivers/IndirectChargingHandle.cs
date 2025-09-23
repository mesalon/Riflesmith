using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndirectChargingHandle : MonoBehaviour, IInteractable {
	Transform Hand => Interactor.transform;
	public Hand Interactor { get; set; }
	public bool PreventInteraction { get; set; }
	[SerializeField] Slide slide;
	
	Vector3 pickedPoint;
	float startPoint;
	float fwdZ;
	
	void Awake() {
		fwdZ = transform.localPosition.z;
	}
	
	public void OnPicked() {
		pickedPoint = Hand.position;
		startPoint = transform.localPosition.z;
	}

	public void OnHold() {
		float relativeOffset = Vector3.Dot(Hand.position - pickedPoint, transform.forward);
		SetSlide(startPoint + relativeOffset);
		slide.fwdZ = transform.localPosition.z;
	}

	public void OnHoldFixed() {
		
	}

	void Update() {
		// Charging handle cannot be behind the bolt
		if (!Interactor) {
			slide.fwdZ = 0;
			if (slide.transform.localPosition.z > transform.localPosition.z) {
				SetSlide(slide.transform.localPosition.z);
			}
		}
	}
	
	public void OnDropped() { }
	
	void SetSlide(float z) {
		transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, Mathf.Clamp(z, slide.backZ, fwdZ));
	}
}