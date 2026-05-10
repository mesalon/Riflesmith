using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

public class VRPlayer : MonoBehaviour, IVRAnchorProvider {
	public Vector3 Anchor { get; set; }

	public static VRInput Input, LastInput;
	public static IVRAnchorProvider anchorProvider;
	private static readonly uint size = (uint)Marshal.SizeOf<VRControllerState_t>();
	private bool gotFirstInput;
	private bool didResetLast;

	public void OnEnable() { Application.onBeforeRender += UpdateInput; }
	public void OnDisable() { Application.onBeforeRender -= UpdateInput; }
	private void Awake() {
		Anchor = transform.position;
		anchorProvider ??= this; // You an an anchor provider of last resort.
	}
	
	[BeforeRenderOrder(-30000)]
	public void UpdateInput() {
		LastInput = Input;

		VRInput currentInput = default;
		InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);                                                                                                                         
		if (hmd.TryGetFeatureValue(CommonUsages.devicePosition, out currentInput.head.position) && !gotFirstInput) { gotFirstInput = true; }
		hmd.TryGetFeatureValue(CommonUsages.deviceRotation, out currentInput.head.rotation);  
		InputDevice LHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
		LHand.TryGetFeatureValue(CommonUsages.devicePosition, out currentInput.LHand.position);
		LHand.TryGetFeatureValue(CommonUsages.deviceRotation, out currentInput.LHand.rotation);
		InputDevice RHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		RHand.TryGetFeatureValue(CommonUsages.devicePosition, out currentInput.RHand.position);
		RHand.TryGetFeatureValue(CommonUsages.deviceRotation, out currentInput.RHand.rotation);

		GetControllerInput(1, ref currentInput.LHand);
		GetControllerInput(2, ref currentInput.RHand);

		if (gotFirstInput) { 
			Input = currentInput; 
			if (!didResetLast) {
				LastInput = Input;
				didResetLast = true;
			}
		}

		transform.position = anchorProvider.Anchor;
		transform.rotation = Input.head.rotation;
	}

	private static bool GetControllerInput(uint hand, ref HandInput input) {
		VRControllerState_t state = new();
		if (OpenVR.System.GetControllerState(hand, ref state, size)) {
			input.stick = new(state.rAxis0.x, state.rAxis0.y);
			input.trigger = state.rAxis1.x;
			input.grip = state.rAxis2.x;
			input.stickButton = IsPressed(state, EVRButtonId.k_EButton_Axis0);
			input.nearButton = IsPressed(state, EVRButtonId.k_EButton_A);
			input.farButton = IsPressed(state, EVRButtonId.k_EButton_IndexController_B);
			return true;
		}
		return false;
	}

	private static bool IsPressed(VRControllerState_t state, EVRButtonId button) => (state.ulButtonPressed & (1UL << (int)button)) != 0;
}

public struct VRInput {
	public HeadInput head;
	public HandInput LHand, RHand;
}

public struct HeadInput {
	public Vector3 position;
	public Quaternion rotation;
	public bool isAlive;
}

public struct HandInput {
	public Vector3 position;
	public Quaternion rotation;
	public Vector2 stick;
	public float trigger;
	public float grip;
	public bool stickButton;
	public bool nearButton;
	public bool farButton;

	public readonly override string ToString() {
		return $"Pose: {position:f2} - {rotation:f2}" +
			$"\nStick: {stick:f2}, pressed: {stickButton:f2}" + 
			$"\nNear: {nearButton:f2}, far: {farButton}" +
			$"\nTrigger: {trigger:f2}, grip: {grip:f2}";
	}
}

public interface IVRAnchorProvider {
	public Vector3 Anchor { get; }
}
