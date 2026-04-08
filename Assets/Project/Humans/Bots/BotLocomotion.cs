using UnityEngine;
using Pathfinding;
using Animancer;

[System.Serializable] public struct LocomotionCfg {
	public float slowWalkSpeed, walkSpeed, jogSpeed, runSpeed, sprintSpeed;
	public float turnSpeed;
	public float lookSpeed;
	public float minAimDistance;
	public float bodyTurnTrigger, bodyTurnCorrection;

	public static readonly LocomotionCfg Default = new() {
		slowWalkSpeed = 0.75f, walkSpeed = 1.25f, jogSpeed = 2, runSpeed = 3.5f, sprintSpeed = 6,
		turnSpeed = 4,
		lookSpeed = 4,
		minAimDistance = 2,
		bodyTurnCorrection = 50,
		bodyTurnTrigger = 1.8f,
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
	private int cornerIdx;
	private bool isStrafing;
	private float speed;
	private Vector3 lookDirection;

	public BotLocomotion(Bot ctx) {
		this.ctx = ctx;
		seeker = ctx.GetComponent<Seeker>();
		Quaternion rot = ctx.transform.rotation;
		lookDirection = ctx.transform.forward;

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

	bool state = false;
	public void Tick() {
		Debug.DrawRay(ctx.eyes.position, lookDirection.normalized, Color.blue);
		Vector3 moveInput = Vector3.zero;
		if (path != null) {
			if (cornerIdx < path.vectorPath.Count) {
				Vector3 dir = (path.vectorPath[cornerIdx] - ctx.transform.position).FlattenY().normalized;
				moveInput = dir * speed;
				Vector3 pathDirection = (path.vectorPath[cornerIdx] - path.vectorPath[cornerIdx - 1]).FlattenY().normalized;
				if (Vector3.Dot((ctx.transform.position - path.vectorPath[cornerIdx]).FlattenY().normalized, pathDirection) >= 0) { cornerIdx++; }
				if (Input.GetKeyDown(KeyCode.L)) { state = !state; }
				if (state) {
					lookDirection = Quaternion.AngleAxis(180 * Time.deltaTime, Vector3.up) * lookDirection;
				} else {
					ctx.transform.rotation = Quaternion.RotateTowards(ctx.transform.rotation, Quaternion.LookRotation(pathDirection), cfg.turnSpeed);
				}
				//FocusAt(pathDirection, false);
			} else {
				path = null;
			}
		}
		moveInput = ctx.transform.InverseTransformDirection(moveInput);
		ctx.anim.Play(ctx.mixer);
		moveParam.TargetValue = new(moveInput.x, moveInput.z);

		// Fix diagonal speed.... I think
		float maxComponent = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.z));
		if (maxComponent > float.Epsilon) {
			float speedMultiplier = moveInput.magnitude / maxComponent;
			mixer.Speed = speedMultiplier;
		}
		else { mixer.Speed = 1f; }
	}

	public void AnimatorMove(Animator anim) {
		ctx.self.locomotion.MoveDirect(anim.deltaPosition);
		ctx.ikLookTarget.position = ctx.eyes.position + lookDirection * 5;
		if (!isStrafing) {
			ctx.transform.rotation = Quaternion.RotateTowards(ctx.transform.rotation, Quaternion.LookRotation(lookDirection), cfg.turnSpeed);
			Ext.Label(ctx.eyes.position, $"spinning? {state}");
		}
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

	public void MoveDirect(Vector2 input) {
		moveParam.TargetValue = new(input.x, input.y);
	}

	private void Move(Vector3 destination, float speed) {
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

	public void Focus(Vector3 target, bool engageStrafe = true) { FocusAt(target - ctx.transform.position, engageStrafe); }
	public void FocusAt(Vector3 direction, bool engageStrafe = true) {
		lookDirection = direction;
		if (engageStrafe) isStrafing = true;
	}

	private void OnActionEnd() {
		upperLayer.StartFade(0, 0.25f);
	}
}
