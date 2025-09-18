using System;
using FMODUnity;
using UnityEngine;

public class GrabInteractable : MonoBehaviour, IInteractable {
    public PlayerInput Input => Interactor ? Interactor.Input : new();
    public PlayerInput LastInput => Interactor ? Interactor.LastInput : new();
    
    public Hand Interactor { get; set; }
    public bool PreventInteraction { get; set; }
    protected Transform Hand => Interactor.holdPoint;
    public event Action onPicked;
    public event Action onHold;
    public event Action onDropped;
    [HideInInspector] public bool doPosing = true;
    [HideInInspector] public Vector3 extraPos;
    [HideInInspector] public Vector3 extraRot;
    [HideInInspector] public Rigidbody rb;
    protected virtual Pose TargetPose => new(Hand.position, Hand.rotation);
    [Header("Interactable Settings")]
    public Transform root;
    public Transform grabPoint;
    [SerializeField] bool doSnapping;
    [SerializeField] private int priority;
    [SerializeField] EventReference grabSound;

    protected void Awake() {
        if (grabSound.Guid.IsNull) { grabSound = EventReference.Find("event:/Foley"); }
        if(!rb) { rb = root ? root.GetComponent<Rigidbody>() : GetComponent<Rigidbody>(); }

        if (!grabPoint) {
            grabPoint = new GameObject("Auto Grab Point").transform; 
            grabPoint.SetParent(transform, false);
        }
        if (!root) { root = transform; }
    }

    public virtual void OnPicked() {
        RuntimeManager.PlayOneShot(grabSound, grabPoint.position);
        onPicked?.Invoke();
    }

    public virtual void OnHold() { 
        onHold?.Invoke();
    }

    public virtual void OnHoldFixed() {
        rb.centerOfMass = root.InverseTransformPoint(grabPoint.position);
        Vector3 targetPosition = TargetPose.position;
        Quaternion targetRotation = TargetPose.rotation;
        
        if (!doSnapping) {
            targetPosition += Hand.rotation * grabPoint.localPosition;
            targetRotation *= Quaternion.Inverse(root.rotation) * grabPoint.rotation;
        }
        
        if (Interactor.other.held is GrabInteractable s && s.transform.root == transform.root && priority > s.priority) {
            Vector3 gripOffset = s.grabPoint.position - grabPoint.position;
            Vector3 handsDir = s.Hand.position - Hand.position;
            Vector3 adjustedDir = handsDir - gripOffset;
            targetRotation = Quaternion.LookRotation(adjustedDir, Vector3.Cross(adjustedDir, Hand.right));
        }
        
        rb.linearVelocity = (targetPosition - grabPoint.position) / Time.fixedDeltaTime;
        Quaternion diff = targetRotation * Quaternion.Inverse(transform.rotation);
        diff.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) {
            angle = 360f - angle; // Take the shorter path
            axis = -axis;         // Reverse the axis
        }
        rb.angularVelocity = axis * (angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    public virtual void OnDropped() {
        onDropped?.Invoke();
    }
}