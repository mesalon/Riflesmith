using UnityEngine;
using Animancer;

[System.Serializable] public class BotLocomotion {
	[SerializeField] MixerTransition2D mixer;
	[SerializeField] LinearMixerTransition startMovement;
	[SerializeField] AnimationClip idle;
	[SerializeField] float smoothing;
	[SerializeField] float turnSpeed;
	private Rigidbody rb;
	private Vector2 inputVel;
	private Vector2 input;
	private Nubot ctx;
	private AnimancerLayer lowerAnim;

	public void Init(Nubot ctx) {
		this.ctx = ctx;
		rb = ctx.GetComponent<Rigidbody>();
		lowerAnim = ctx.anim.Layers[0];
		lowerAnim.Play(idle);
		startMovement.Events.OnEnd += () => {
			lowerAnim.Play(mixer);
		};
	}

	public void Tick() {
		Move(Ext.moveDir2D);
	}

	public void Move(Vector2 direction) {
		if (direction.sqrMagnitude > 0 && lowerAnim.CurrentState != startMovement.State) {
			lowerAnim.Play(startMovement);
			float angle = Vector3.SignedAngle(ctx.transform.forward, new Vector3(direction.x, 0, direction.y), Vector3.up);
			Debug.Log($"{angle:f2}");
			startMovement.State.Parameter = angle;
		}
	}

	public void AnimatorMove(Animator anim) {
		ctx.transform.position += anim.deltaPosition;
		ctx.transform.rotation *= anim.deltaRotation;
	}
}
