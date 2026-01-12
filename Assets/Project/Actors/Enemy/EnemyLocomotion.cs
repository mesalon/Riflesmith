using UnityEngine;
using Pathfinding;

[System.Serializable] public struct LocomotionCfg {
	public float slowWalkSpeed, walkSpeed, jogSpeed, runSpeed, sprintSpeed;
	public float turnSpeed;
	public float lookSpeed;
	public float minAimDistance;

	public static readonly LocomotionCfg Default = new() {
		slowWalkSpeed = 0.75f, walkSpeed = 1.25f, jogSpeed = 2, runSpeed = 3.5f, sprintSpeed = 6,
		turnSpeed = 4,
		lookSpeed = 4,
		minAimDistance = 2,
	};
}

public enum Pace { SlowWalk, Walk, Jog, Run, Sprint }

public class EnemyLocomotion {
	public bool Arrived => path == null;
	private LocomotionCfg cfg => ctx.cfg.locomotion;
	private Seeker seeker;
	private readonly Enemy ctx;
	private Vector3 lastPos;
	private Vector3 destination;
	private float pitch, yaw;
	private float pitchTarget, yawTarget;
	private Path path;
	private int cornerIdx;
	private bool isCrouching;
	private bool isStrafing;
	private float speed;

	public EnemyLocomotion(Enemy ctx) {
		this.ctx = ctx;
		seeker = ctx.GetComponent<Seeker>();
		Quaternion rot = ctx.transform.rotation;
		pitch = rot.eulerAngles.x.NormalizeAngle();
		yaw = rot.eulerAngles.y.NormalizeAngle();
	}

	public void Tick() {
		if (path != null) {
			if (cornerIdx < path.vectorPath.Count) {
				Vector3 dir = (path.vectorPath[cornerIdx] - ctx.transform.position).FlattenY().normalized;
				ctx.locomotion.Move(dir, speed);
				Vector3 pathDirection = (path.vectorPath[cornerIdx] - path.vectorPath[cornerIdx - 1]).FlattenY().normalized;
				if (Vector3.Dot((ctx.transform.position - path.vectorPath[cornerIdx]).FlattenY().normalized, pathDirection) >= 0) { cornerIdx++; }
				if (!isStrafing) FocusAt(pathDirection, false);
			} else {
				path = null;
			}
		}
		pitch = Mathf.Lerp(pitch, pitchTarget, cfg.turnSpeed * Time.deltaTime);
		yaw = Mathf.Lerp(yaw, yawTarget, cfg.turnSpeed * Time.deltaTime);
		ctx.transform.rotation = Quaternion.Lerp(ctx.transform.rotation, Quaternion.Euler(0, yaw, 0), cfg.turnSpeed * Time.deltaTime);
		ctx.ikTarget.position = ctx.eyes.position + Quaternion.Euler(pitch, yaw, 0) * Vector3.forward * 5;
		ctx.anim.SetFloat("MoveX", Mathf.Clamp(Vector3.Dot(ctx.transform.right, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
		ctx.anim.SetFloat("MoveY", Mathf.Clamp(Vector3.Dot(ctx.transform.forward, ctx.transform.position - lastPos) / Time.deltaTime, -1, 1), 0.1f, Time.deltaTime);
		ctx.anim.SetBool("Crouching", isCrouching); 
		lastPos = ctx.transform.position;

		isStrafing = false;
	}

	public void Move(Vector3 destination, Pace pace) {
		float speed = pace switch {
			Pace.SlowWalk => cfg.slowWalkSpeed,
			Pace.Walk => cfg.walkSpeed,
			Pace.Jog => cfg.jogSpeed,
			Pace.Run => cfg.runSpeed,
			Pace.Sprint => cfg.sprintSpeed,
			_ => 0,
		};
		Move(destination, speed);
	}
	public void Move(Vector3 destination, float speed) {
		if (this.destination != destination) {
			seeker.StartPath(ctx.transform.position, destination, OnPathComplete);
			this.destination = destination;
			this.speed = speed;
			cornerIdx = 1;
		}
	}
	public void Stop() {
		destination = ctx.transform.position;
		path = null;
	}

	private void OnPathComplete(Path p) {
		if (!p.error) { path = p; }
		else { Debug.Log($"Error occured during OnPathComplete ({p.error})"); }
	}

	public void FocusAt(Vector3 direction, bool engageStrafe = true) {
		Quaternion rot = Quaternion.LookRotation(direction);
		pitchTarget = rot.eulerAngles.x.NormalizeAngle();
		yawTarget = rot.eulerAngles.y.NormalizeAngle();
		if (engageStrafe) isStrafing = true;
	}
	public void Focus(Vector3 target, bool engageStrafe = true) { FocusAt(target - ctx.eyes.position, engageStrafe); }
}
