using UnityEngine;
using Pathfinding;

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
	private Vector3 lastPos;
	private Vector3 destination;
	private Vector3? lookTarget;
	private Path path;
	private int cornerIdx;
	private float verticalVelocity;
	private float speed;
	private bool isCrouching;
	private bool isAiming;
	private Transform head;
	private Vector2 ikAim;
	private Vector3 velocity;
	private Vector3 velocityRef;
	private float startMoveTime;

	public EnemyLocomotion(Blackboard ctx) {
		this.ctx = ctx;
		speed = cfg.walkSpeed;
	}

	public void Tick() {
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = GameManager.Camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray.origin, ray.direction * 1000, out RaycastHit hit)) {
				Move(hit.point, cfg.jogSpeed);
			}
		}

		if (!ctx.cc.isGrounded) { verticalVelocity += Physics.gravity.y * Time.deltaTime; } 
		else if (verticalVelocity < 0) { verticalVelocity = -2f; }
		ctx.cc.Move(new(0, verticalVelocity, 0));

		if (path != null) {
			if (cornerIdx < path.vectorPath.Count) {
				Vector3 dir = (path.vectorPath[cornerIdx] - ctx.transform.position).FlattenY().normalized;
				ctx.cc.Move(dir * (speed * Time.deltaTime));
				Vector3 pathDirection = (path.vectorPath[cornerIdx] - path.vectorPath[cornerIdx - 1]).FlattenY().normalized;
				float dot = Vector3.Dot((ctx.transform.position - path.vectorPath[cornerIdx]).FlattenY().normalized, pathDirection);
				if (dot >= 0) { cornerIdx++; }

				if (lookTarget == null) { 
					Vector3 aimDir = dir;
					lookTarget = ctx.eyes.position + aimDir * cfg.minAimDistance;
				}
			}
			Quaternion rot = Quaternion.LookRotation((lookTarget.Value - ctx.transform.position).FlattenY().normalized);
			ctx.transform.rotation = Quaternion.Lerp(ctx.transform.rotation, rot, cfg.turnSpeed * Time.deltaTime);
			ctx.ikTarget.position = Vector3.Lerp(ctx.ikTarget.position, lookTarget.Value, cfg.lookSpeed * Time.deltaTime);
			lookTarget = null; // Consume focus. It has to be set every frame.

			ctx.gunRestRig.weight = Mathf.Lerp(ctx.gunRestRig.weight, isAiming ? 0 : 1, Time.deltaTime * cfg.aimSpeed);
			ctx.anim.SetFloat("MoveX", Mathf.Clamp(Vector3.Dot(ctx.transform.right, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
			ctx.anim.SetFloat("MoveY", Mathf.Clamp(Vector3.Dot(ctx.transform.forward, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
			ctx.anim.SetBool("Crouching", isCrouching); 
			lastPos = ctx.transform.position;
		}
	}

	public void Move(Vector3 destination, float speed, Vector3? lookTarget = null) {
		if (this.destination != destination) {
			this.destination = destination;
			ctx.seeker.StartPath(ctx.transform.position, destination, OnPathComplete);
			this.lookTarget = lookTarget;
			this.speed = speed;
			cornerIdx = 1;
		}
	}

	private void OnPathComplete(Path p) {
		if (!p.error) {
			path = p;
		} else {
			Debug.Log($"Error occured during OnPathComplete ({p.error})");
		}
	}

	private void Face(Vector3 target, float speed) {
	}

	public void ADS(bool state) {
		isAiming = state;
	}
}
