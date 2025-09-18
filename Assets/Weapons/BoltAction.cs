using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public class BoltAction : MonoBehaviour, IInteractable {
    public Hand Interactor { get; set; }
    public bool PreventInteraction { get; set; }
    Transform Hand => Interactor.transform;
    [SerializeField] FirearmReceiver receiver;
    [SerializeField] EventReference slideUp;
    [SerializeField] EventReference slideDown;
    [SerializeField] float rearZ;
    [SerializeField] float backZ;
    [SerializeField] float upRot;
    [SerializeField] float rotOffset;
    Vector3 pickedPoint;
    
    float startPoint;
    float fwdZ;
    float downRot;
    float lastZ;
    
    void Awake() {
        fwdZ = transform.localPosition.z;
        lastZ = fwdZ;
        downRot = Mathf.Repeat(downRot, 360);
        upRot = Mathf.Repeat(upRot, 360);
    }

    public void OnPicked() {
        pickedPoint = Hand.position;
        startPoint = transform.localPosition.z;
    }

    public void OnHold() {
        float relativeOffset = Vector3.Dot(Hand.position - pickedPoint, transform.forward); 
        SetSlideZ(startPoint + relativeOffset);

        Vector3 from = Vector3.ProjectOnPlane(receiver.transform.right, receiver.transform.forward);
        Vector3 to = Vector3.ProjectOnPlane(Hand.position - transform.position, transform.forward);
        SetSlideRot(Vector3.SignedAngle(from, to, receiver.transform.forward) + rotOffset); // todo: fix the weird clamp snapping that happens with this.
    }

    public void OnHoldFixed() {
        throw new NotImplementedException();
    }

    void Update() {
        receiver.hammerLocked = transform.localPosition.z != fwdZ;
        
        // Slide is back
        if (transform.localPosition.z.DidRecede(rearZ, lastZ)) {
            RuntimeManager.PlayOneShot(slideDown, transform.position);
            receiver.Eject();
            receiver.hammerState = true;
        }

        // Forward slide positions
        if (transform.localPosition.z.DidReach(fwdZ, lastZ)) {
            RuntimeManager.PlayOneShot(slideUp, transform.position);
        }
        if (transform.localPosition.z.DidPass(rearZ, lastZ)) { // Slide is forward
            if (receiver.magwell.Magazine && receiver.magwell.Magazine.ammo.TryPop(out ProjectileData round)) {
                receiver.chamber = round;
            }
        }

        lastZ = transform.localPosition.z;
    }

    public void OnDropped() { }

    void SetSlideZ(float z) {
        float back = transform.localRotation.eulerAngles.z == upRot ? backZ : fwdZ;
        transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, Mathf.Clamp(z, back, fwdZ));
    }

    void SetSlideRot(float rot) {
        float down = transform.localPosition.z == fwdZ ? downRot : upRot;
        transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, Mathf.Clamp(rot, down, upRot));
    }
}
