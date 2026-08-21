using UnityEngine;

public class LitePlayer : MonoBehaviour, IVRAnchorProvider {
	[SerializeField] Transform head;
	[SerializeField] float speed, turnSpeed;
	DeviceInput headinput => VRPlayer.Input.head;

	public Pose Anchor => new(head.position, transform.rotation);

	void Awake() => VRPlayer.anchorProvider = this;

	void Update() {
		Vector3 headDelta = transform.rotation * (headinput.position - VRPlayer.LastInput.head.position);
		transform.position += headDelta.FlattenY();
		transform.rotation *= Quaternion.Euler(0, VRPlayer.Input.RHand.stick.x.Deadzone(0.1f) * turnSpeed, 0);
		head.localPosition = new(0, headinput.position.y, 0);

		DeviceInput handInput = VRPlayer.Input.LHand;
		Vector3 movement = transform.rotation * headinput.rotation * new Vector3(handInput.stick.x, 0, handInput.stick.y) * speed * Time.deltaTime;
		transform.position += movement.FlattenY();
		transform.Rotate(new(0, VRPlayer.Input.RHand.stick.x * turnSpeed * Time.deltaTime, 0));
	}
}
