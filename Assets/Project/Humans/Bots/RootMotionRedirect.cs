using UnityEngine;

public class RootMotionRedirect : MonoBehaviour {
	public BotLocomotion target;
	private Animator anim;

	private void Awake() {
		anim = GetComponent<Animator>();
	}

	public void OnAnimatorMove() {
		target.AnimatorMove(anim);
	}
}
