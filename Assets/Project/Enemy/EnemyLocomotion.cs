using UnityEngine;
using UnityEngine.AI;

[System.Serializable] public struct LocomotionCfg {
	public float walkSpeed;
	public float jogSpeed;
	public float sprintSpeed;
	public float turnSpeed;
	public float aimSpeed;
	public float lookSpeed;
	public float minAimDistance;

	public static readonly LocomotionCfg Default = new() {
		walkSpeed = 1.25f,
		jogSpeed = 2,
		sprintSpeed = 6,
		turnSpeed = 4,
		aimSpeed = 2,
		lookSpeed = 4,
		minAimDistance = 2,
	};
}

public class EnemyLocomotion {
	// External calls simply tell the AI to move towards a position with some level of urgency
	// They can also specify a look target, and whether to aim there with the gun
	private readonly Blackboard ctx;
	private LocomotionCfg cfg => ctx.cfg.locomotion;
	private NavMeshPath path;
	private Transform head;
	private Vector3 destination;
	private Vector3 navDestination;
	private Vector2 ikAim;
	private Vector3 lastPos;
	private Vector3 velocity;
	private Vector3 velocityRef;
	private bool isCrouching;
	private bool isAiming;
	private float startMoveTime;
	private int cornerIdx;
	private float verticalVelocity;

	public EnemyLocomotion(Blackboard ctx) {
		this.ctx = ctx;
		destination = ctx.transform.position;
		path = new();
	}

	Vector3 dest;
	public void Tick() {
		bool isGrounded = ctx.cc.isGrounded;
		if (!isGrounded) { verticalVelocity += Physics.gravity.y * Time.deltaTime; } 
		else if (verticalVelocity < 0) { verticalVelocity = -2f; }
		ctx.cc.Move(new(0, verticalVelocity, 0));

		if (Input.GetMouseButtonDown(0)) {
			Ray ray = GameManager.Camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray.origin, ray.direction * 1000, out RaycastHit hit)) {
				dest = hit.point;
			}
		}
		if (ctx.focus) ctx.aimFocus = ctx.focus.position;
		if (ctx.dingle) dest = ctx.dingle.position;
		if (dest != default) Move(dest, cfg.walkSpeed);

		Ext.DrawPath(path.corners);

		Vector3 aimTarget;
		if (ctx.aimFocus is Vector3 focus) { 
			aimTarget = focus;
			ctx.aimFocus = null; // Consume focus. It has to be set every frame.
		} 
		else { 
			Vector3 aimDir = (destination + ctx.eyes.position.y * Vector3.up - ctx.eyes.position).normalized; 
			aimTarget = ctx.eyes.position + aimDir * cfg.minAimDistance;
		}
		Face(aimTarget, cfg.turnSpeed);
		Ext.DrawCube(aimTarget, Quaternion.identity, Vector3.one * 0.05f, Color.red);
		ctx.ikTarget.position = Vector3.Lerp(ctx.ikTarget.position, aimTarget, cfg.lookSpeed);

		ctx.gunRestRig.weight = Mathf.Lerp(ctx.gunRestRig.weight, isAiming ? 0 : 1, Time.deltaTime * cfg.aimSpeed);
		ctx.anim.SetFloat("MoveX", Mathf.Clamp(Vector3.Dot(ctx.transform.right, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
		ctx.anim.SetFloat("MoveY", Mathf.Clamp(Vector3.Dot(ctx.transform.forward, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
		ctx.anim.SetBool("Crouching", isCrouching); 
		lastPos = ctx.transform.position;

	}

	public bool Move(Vector3 destination, float speed) {
		if (this.destination != destination) {
			this.destination = destination;
			if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 20, NavMesh.AllAreas)) { navDestination = hit.position; }
			cornerIdx = 1;
			NavMesh.CalculatePath(ctx.transform.position, navDestination,  NavMesh.AllAreas, path);
			Debug.Log("Recalculating");
		}

		for (int i = 0; i < path.corners.Length; i++) { Ext.Label(path.corners[i], $"Corner {i}"); }
		Debug.Log($"Total: {path.corners.Length}");
		if (cornerIdx < path.corners.Length) {
			Ext.Label(path.corners[cornerIdx] + Vector3.up * 0.2f, "current dest");
			Vector3 dir = (path.corners[cornerIdx] - ctx.transform.position).FlattenY().normalized;
			if (ctx.aimFocus == null) {
				Quaternion rot = Quaternion.LookRotation(dir);
				ctx.transform.rotation = Quaternion.Lerp(ctx.transform.rotation, rot, cfg.turnSpeed * Time.deltaTime);
			}
			ctx.cc.Move(dir * (speed * Time.deltaTime));

			Vector3 pathDirection = path.corners[cornerIdx] - path.corners[cornerIdx - 1];
			if (Vector3.Dot((ctx.transform.position - path.corners[cornerIdx]).normalized, pathDirection) >= 0) { cornerIdx++; }
			return false;
		}
		return true;
	}

	private void Face(Vector3 target, float speed) {
		Quaternion rot = Quaternion.LookRotation((target - ctx.transform.position).FlattenY().normalized);
		ctx.transform.rotation = Quaternion.Lerp(ctx.transform.rotation, rot, speed * Time.deltaTime);
	}

	public void ADS(bool state) {
		isAiming = state;
	}
}
