using UnityEngine;
using FMODUnity;

// todo: transition this from directly using transform to an internal value with transform following it
public class Slide : MonoBehaviour, IInteractable {
    [SerializeField] bool preventInteraction;
    public bool PreventInteraction {
        get;
        set;
    }

    public Hand Interactor { get; set; }
    bool slideLockActive => doesLockBack && receiver.magwell.Magazine && receiver.magwell.Magazine.ammo.Count == 0; // An empty magazine is inside
    Transform Hand => Interactor.transform;
    public float backZ;
    [HideInInspector] public float fwdZ;
    [HideInInspector] public bool slideLocked;
    [SerializeField] FirearmReceiver receiver;
    [SerializeField] EventReference slideUp;
    [SerializeField] EventReference slideDown;
    [SerializeField] bool doesLockBack;
    [SerializeField] float lockPos;
    Vector3 pickedPoint;
    float internalPosition;
    float animTime;
    float startPoint;
    float lastZ;
    
    void Awake() {
        fwdZ = internalPosition = transform.localPosition.z; 
        receiver.OnFired += Cycle;
        animTime = receiver.CyclicInterval;
    }

    public void OnPicked() {
        pickedPoint = Hand.position;
        startPoint = internalPosition;
    }

    public void OnHold() {
        float relativeOffset = Vector3.Dot(Hand.position - pickedPoint, transform.forward);
        SetSlide(startPoint + relativeOffset);
    }

    public void OnHoldFixed() {
        
    }

    void Update() {
        transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, animTime < receiver.CyclicInterval ?
            Mathf.Lerp(fwdZ, backZ, GameManager.I.settings.blowbackCurve.Evaluate(animTime / receiver.CyclicInterval)) : internalPosition);
        
        receiver.hammerLocked = internalPosition != fwdZ;
        if (!Interactor) {
            SetSlide(internalPosition + receiver.stats.springSpeed * Time.deltaTime);
        }
        
        // Slide is back
        if (internalPosition.DidRecede(lockPos, lastZ)) {
            RuntimeManager.PlayOneShot(slideDown, transform.position);
            receiver.Eject();
            receiver.hammerState = true;
        }
        if (internalPosition.DidFail(lockPos, lastZ)) { slideLocked = slideLockActive; }

        // Forward slide positions
        if (internalPosition.DidReach(0, lastZ)) {
            RuntimeManager.PlayOneShot(slideUp, transform.position);
        }
        if (internalPosition.DidPass(lockPos, lastZ)) { // Slide going forward
            if (receiver.magwell.Magazine && receiver.magwell.Magazine.ammo.TryPop(out ProjectileData round)) {
                receiver.chamber = round;
            }
        }
        
        if (receiver.grip.Input.farButton) { slideLocked = false; }

        animTime += Time.deltaTime;
        lastZ = internalPosition;
    }

    public void OnDropped() { }
 
    public void SetSlide(float z) {
        internalPosition = Mathf.Clamp(z, backZ, slideLocked ? lockPos : fwdZ);
    }
    
    void Cycle() {
        if(Interactor) Interactor.Drop();
        if (receiver.stats.gasBlowback && receiver.stats.springSpeed > 0) {
            animTime = 0;
            receiver.hammerState = true;
            receiver.Eject();
            slideLocked = slideLockActive;
            SetSlide(fwdZ);
            if (receiver.magwell.Magazine && receiver.magwell.Magazine.ammo.TryPop(out ProjectileData round)) {
                receiver.chamber = round;
            }
        } else if (receiver.stats.gasBlowback) { // todo: animate this and generally just fking overhaul it goddamn
            receiver.hammerState = true;
            receiver.Eject();
            SetSlide(backZ);
        }
    }
}