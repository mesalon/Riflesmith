using UnityEngine;

public class RootMotionRedirect : MonoBehaviour {
	public Nubot target;
	private Animator anim;

	private void Awake() { anim = GetComponent<Animator>(); }
	public void OnAnimatorMove() { target.motion.AnimatorMove(anim); }
}
