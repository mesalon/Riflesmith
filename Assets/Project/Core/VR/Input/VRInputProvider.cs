using UnityEngine;

public abstract class VRInputProvider : MonoBehaviour {
	public abstract void GetInput(ref VRInput LastInput, ref VRInput Input);
}

public struct VRInput {
	public static VRInput TPose => new() {
		head = new() { position = new(0, 1.8f, 0), rotation = Quaternion.identity },
		LHand = new() { position = new(-0.5f, 0.5f, 0), rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right) },
		RHand = new() { position = new(0.5f, 0.5f, 0), rotation = Quaternion.LookRotation(Vector3.forward, Vector3.left) },
	};
	public DeviceInput head, LHand, RHand;
}


public struct DeviceInput {
	public Vector3 position;
	public Quaternion rotation;
	public Vector2 stick;
	public float trigger;
	public float grip;
	public bool stickButton;
	public bool nearButton;
	public bool farButton;
	public bool gotFirstInput;

	public readonly override string ToString() {
		return $"Pose: {position:f2} - {rotation:f2}" +
			$"\nStick: {stick:f2}, pressed: {stickButton:f2}" + 
			$"\nNear: {nearButton:f2}, far: {farButton}" +
			$"\nTrigger: {trigger:f2}, grip: {grip:f2}";
	}

	public readonly DeviceInput RelativeTo(Vector3 offset) {
		DeviceInput result = this;
		result.position -= offset;
		return result;
	}
}
