using UnityEngine;
using Pathfinding;
using Animancer;

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

public class BotLocomotion {
	private LocomotionCfg cfg => ctx.cfg.locomotion;
	private AnimancerComponent anim => ctx.anim;
	private AnimancerLayer upperLayer => ctx.upperLayer;
	private ClipTransition equipWeapon => ctx.equipWeapon;
	private ClipTransition dequipWeapon => ctx.dequipWeapon;
	private SmoothedVector2Parameter moveParam => ctx.moveParam;
	private AvatarMask mask => ctx.mask;
	private StringAsset moveX => ctx.moveX;
	private StringAsset moveY => ctx.moveY;
	private TransitionAsset mixer => ctx.mixer;

	private readonly Bot ctx;

	public bool Arrived => path == null;
	private Seeker seeker;
	private Path path;
	private Vector3 destination;
	private float pitch, yaw;
	private float pitchTarget, yawTarget;
	private int cornerIdx;
	private bool isStrafing;
	private float speed;

	public BotLocomotion(Bot ctx) {
		this.ctx = ctx;
		seeker = ctx.GetComponent<Seeker>();
		Quaternion rot = ctx.transform.rotation;
		pitch = pitchTarget = rot.eulerAngles.x.NormalizeAngle();
		yaw = yawTarget = rot.eulerAngles.y.NormalizeAngle();

		ctx.lowerLayer = anim.Layers[0];
		ctx.upperLayer = anim.Layers[1];
		upperLayer.Mask = mask;
		equipWeapon.Events.OnEnd = dequipWeapon.Events.OnEnd = OnActionEnd;
    ctx.moveParam = new SmoothedVector2Parameter(
        anim,
        moveX,
        moveY,
        0.1f);
	}

	public void Tick() {
		Vector3 moveInput = Vector3.zero;
		if (path != null) {
			if (cornerIdx < path.vectorPath.Count) {
				Vector3 dir = (path.vectorPath[cornerIdx] - ctx.transform.position).FlattenY().normalized;
				moveInput = dir * speed;
				Vector3 pathDirection = (path.vectorPath[cornerIdx] - path.vectorPath[cornerIdx - 1]).FlattenY().normalized;
				if (Vector3.Dot((ctx.transform.position - path.vectorPath[cornerIdx]).FlattenY().normalized, pathDirection) >= 0) { cornerIdx++; }
				if (!isStrafing) FocusAt(pathDirection, false);
			} else {
				path = null;
			}
		}
		moveInput = ctx.transform.InverseTransformDirection(moveInput);
		Debug.DrawRay(ctx.transform.position, moveInput * 5, Color.green);
		ctx.anim.Play(ctx.mixer);
		moveParam.TargetValue = new(moveInput.x, moveInput.z);

		// Fix diagonal speed.... I think
		float maxComponent = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.z));
		if (maxComponent > float.Epsilon) {
			float speedMultiplier = moveInput.magnitude / maxComponent;
			mixer.Speed = speedMultiplier;
		}
		else { mixer.Speed = 1f; }
		isStrafing = false;
	}


	public void AnimatorMove(Animator anim) {
		pitch = Mathf.Lerp(pitch, pitchTarget, cfg.turnSpeed * Time.deltaTime);
		yaw = Mathf.Lerp(yaw, yawTarget, cfg.turnSpeed * Time.deltaTime);
		ctx.ikLookTarget.position = ctx.eyes.position + Quaternion.Euler(pitch, yaw, 0) * Vector3.forward * 5;
		ctx.transform.rotation = Quaternion.Lerp(ctx.transform.rotation, Quaternion.Euler(0, yaw, 0), cfg.turnSpeed * Time.deltaTime);
		ctx.self.locomotion.MoveDirect(anim.deltaPosition);
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

	private void OnActionEnd() {
		upperLayer.StartFade(0, 0.25f);
	}
}
