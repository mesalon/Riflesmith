using System;
using UnityEngine;
using UnityEngine.AI;
using FMODUnity;
using UnityEngine.Serialization;

public class EnemyLocomotion : MonoBehaviour {
    public bool didArrive;
    [SerializeField] private Animator anim;
    [SerializeField] private CharacterController cc;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float sens;
    [SerializeField] UnityEngine.Animations.Rigging.Rig gunRestRig;
    [SerializeField] private float aimSpeed;
    [SerializeField] private bool verticalLook;
    [SerializeField] private Transform head;
    [SerializeField] private Transform ikTarget;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Vector3 destination;
    [SerializeField] float smoothing = 0.5f;
    [SerializeField] private float destinationTolerance = 0.1f;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float minAimDistance;
    private Vector2 ikAim;
    private Vector3 aimTarget;
    private bool isCrouching;
    private bool isAiming;
    private Vector3 lastPos;
    public NavMeshPath path;
    private Vector3 velocity;
    private float startMoveTime;
    private Vector3 velocityRef;
    private bool isLookOverridden;
    private EnemyAI ai;

    private void Awake() {
        ai = GetComponent<EnemyAI>();
        destination = transform.position;
				path = new();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        NavMesh.CalculatePath(transform.position, destination,  NavMesh.AllAreas, path);
        if (path.corners.Length > 0) {
            int cornerIdx = 0;
            for (int i = 1; i < path.corners.Length; i++) {
                if ((path.corners[i] - transform.position).sqrMagnitude > destinationTolerance) {
                    cornerIdx = i;
                    break;
                }
            }

            Vector3 destination = Physics.Raycast(path.corners[cornerIdx], Vector3.down, out RaycastHit hit) ? hit.point : path.corners[cornerIdx];
            Vector3 moveDir = destination - transform.position;
            Move(moveDir, walkSpeed);
            if(!isLookOverridden && (moveDir.x > 0.05f || moveDir.y > 0.05f)) LookAt(moveDir);
            didArrive = (destination - transform.position).sqrMagnitude < destinationTolerance;
        }

        Vector3 aimDir = aimTarget - ai.eyes.position;
        Vector3 correctedTarget = aimDir.sqrMagnitude < minAimDistance * minAimDistance ? // Adjust for min distance
            ai.eyes.position + aimDir.normalized * minAimDistance : aimTarget;
        ikTarget.position = Vector3.Lerp(ikTarget.position, correctedTarget, lookSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, Mathf.Atan2(aimDir.x, aimDir.z) * Mathf.Rad2Deg, 0), lookSpeed * Time.deltaTime);
        
        gunRestRig.weight = Mathf.Lerp(gunRestRig.weight, isAiming ? 0 : 1, Time.deltaTime * aimSpeed);
        anim.SetFloat("MoveX", Mathf.Clamp(Vector3.Dot(transform.right, transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
        anim.SetFloat("MoveY", Mathf.Clamp(Vector3.Dot(transform.forward, transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
        anim.SetBool("Crouching", isCrouching); 
        lastPos = transform.position;
        isLookOverridden = false;
    }

    private void OnDrawGizmosSelected() {
        if(!Application.isPlaying) return;
        foreach (Vector3 v in path.corners) {
            Gizmos.DrawWireSphere(v, 0.05f);
        }
    }

    private void Move(Vector3 direction, float speed, Vector3? lookTarget = null) {
        Vector3 dir = new(direction.x, 0, direction.z);
        cc.Move(dir.normalized * (speed * Time.deltaTime));
    }

    public void MoveTo(Vector3 destination) {
        this.destination = destination;
    }

    public void LookAt(Vector3 position) {
        aimTarget = position;
        isLookOverridden = true;
    }
    
    public void ADS(bool state) {
        isAiming = state;
    }
}
