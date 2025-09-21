using UnityEngine;
using UnityEngine.AI;

[System.Serializable] public struct LocomotionCfg {
	public LayerMask hitMask;
	public float walkSpeed;
	public float runSpeed;
	public float sens;
	public float aimSpeed;
	public bool verticalLook;
	public float smoothing;
	public float destinationTolerance;
	public float lookSpeed;
	public float minAimDistance;
}

public class EnemyLocomotion {
	private readonly Blackboard ctx;
	private readonly LocomotionCfg cfg;
	private NavMeshPath path;
	private Transform head;
	private Vector3 destination;
	private Vector2 ikAim;
	private Vector3 aimTarget;
	private Vector3 lastPos;
	private Vector3 velocity;
	private Vector3 velocityRef;
	public bool didArrive;
	private bool isCrouching;
	private bool isAiming;
	private float startMoveTime;
	private bool isLookOverridden;

	public EnemyLocomotion(Blackboard ctx) {
		this.ctx = ctx;
		cfg = ctx.cfg.locomotion;
		destination = ctx.transform.position;
		path = new();
	}

	public void Tick() {
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = GameManager.Camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray.origin, ray.direction * 1000, out RaycastHit hit)) {
				MoveTo(hit.point);
				Debug.Log("hi");
			}
		}

		NavMesh.CalculatePath(ctx.transform.position, destination,  NavMesh.AllAreas, path);
		if (path.corners.Length > 0) {
			int cornerIdx = 0;
			for (int i = 1; i < path.corners.Length; i++) {
				if ((path.corners[i] - ctx.transform.position).sqrMagnitude > cfg.destinationTolerance) {
					cornerIdx = i;
					break;
				}
			}

			Vector3 destination = Physics.Raycast(path.corners[cornerIdx], Vector3.down, out RaycastHit hit) ? hit.point : path.corners[cornerIdx];
			Vector3 moveDir = destination - ctx.transform.position;
			Move(moveDir, cfg.walkSpeed);
			if(!isLookOverridden && (moveDir.x > 0.05f || moveDir.y > 0.05f)) LookAt(moveDir);
			didArrive = (destination - ctx.transform.position).sqrMagnitude < cfg.destinationTolerance;
		}

		Vector3 aimDir = aimTarget - ctx.eyes.position;
		Vector3 correctedTarget = aimDir.sqrMagnitude < cfg.minAimDistance * cfg.minAimDistance ? // Adjust for min distance
			ctx.eyes.position + aimDir.normalized * cfg.minAimDistance : aimTarget;
		ctx.ikTarget.position = Vector3.Lerp(ctx.ikTarget.position, correctedTarget, cfg.lookSpeed);
		ctx.transform.rotation = Quaternion.Lerp(ctx.transform.rotation, Quaternion.Euler(0, Mathf.Atan2(aimDir.x, aimDir.z) * Mathf.Rad2Deg, 0), cfg.lookSpeed * Time.deltaTime);

		ctx.gunRestRig.weight = Mathf.Lerp(ctx.gunRestRig.weight, isAiming ? 0 : 1, Time.deltaTime * cfg.aimSpeed);
		ctx.anim.SetFloat("MoveX", Mathf.Clamp(Vector3.Dot(ctx.transform.right, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
		ctx.anim.SetFloat("MoveY", Mathf.Clamp(Vector3.Dot(ctx.transform.forward, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
		ctx.anim.SetBool("Crouching", isCrouching); 
		lastPos = ctx.transform.position;
		isLookOverridden = false;
	}

	private void Move(Vector3 direction, float speed, Vector3? lookTarget = null) {
		Vector3 dir = new(direction.x, 0, direction.z);
		ctx.cc.Move(dir.normalized * (speed * Time.deltaTime));
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
