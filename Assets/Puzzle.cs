using System;
using UnityEngine;

public class Puzzle : MonoBehaviour, IInteractable {
    [SerializeField] private Transform knob;
    [SerializeField] private float min, max;
    public bool isSolved;
    public Hand Interactor { get; set; }
    public bool PreventInteraction { get; set; }
    Vector3 pickedPoint;

    public void OnPicked() { pickedPoint = Interactor.transform.position; }

    public void OnHold() {
        knob.localPosition = new(knob.localPosition.x, Mathf.Clamp(Vector3.Dot(Interactor.transform.position - pickedPoint, transform.up), min, max), knob.localPosition.z);
    }

    public void OnHoldFixed() {
        throw new NotImplementedException();
    }

    public void OnDropped() { }

    private void Update() {
        if (knob.localPosition.y == max) {
            isSolved = true;
            Interactor?.Drop();
            PreventInteraction = true;
        }
    }
}