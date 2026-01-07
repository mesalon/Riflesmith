using UnityEngine;

public class Locomotion {
	private CharacterController cc;
	private float yVelocity;

	public Locomotion(Actor ctx) {
		cc = ctx.GetComponent<CharacterController>();
	}

	public void Tick() {
		if (!cc.isGrounded) { yVelocity += Physics.gravity.y * Time.deltaTime; } 
		else if (yVelocity < 0) { yVelocity = -2f; }
		cc.Move(new(0, yVelocity, 0));

		/* Hold this. It's important
		GrabInteractable L = rig.LHand.held as GrabInteractable;
		GrabInteractable R = rig.RHand.held as GrabInteractable;
		L?.rb.MovePosition(L.rb.position + movement * Time.deltaTime);
		R?.rb.MovePosition(R.rb.position + movement * Time.deltaTime);
		 */
	}

	public void Move(Vector3 direction, float speed) {
		cc.Move(direction * (speed * Time.deltaTime));
	}
}
