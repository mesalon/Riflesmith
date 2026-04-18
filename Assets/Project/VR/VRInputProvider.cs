using UnityEngine;

public class VRInputProvider {

}

public struct VRInput {
	public HeadInput head;
	public HandInput LHand, RHand;
}

public struct HeadInput {
	public Vector3 pos, rot;
	public bool isAlive;
}

public struct HandInput {
	public Vector3 pos, rot;
	public Vector2 stick;
	public bool stickButton;
	public float trigger;
	public float grip;
	public bool farButton;
	public bool nearButton;
	public bool isAlive;
}
