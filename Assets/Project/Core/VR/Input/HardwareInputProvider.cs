using System.Runtime.InteropServices;
using UnityEngine.XR;
using UnityEngine;
using Valve.VR;

[CreateAssetMenu()]
public class HardwareInputProvider : VRInputProvider {
	private static readonly uint size = (uint)Marshal.SizeOf<VRControllerState_t>();

	public override void GetInput(ref VRInput LastInput, ref VRInput Input) {
		ApplyPoseInput(XRNode.CenterEye, ref LastInput.head, ref Input.head);
		ApplyPoseInput(XRNode.LeftHand, ref LastInput.LHand, ref Input.LHand);
		ApplyPoseInput(XRNode.RightHand, ref LastInput.RHand, ref Input.RHand);
		ApplyControllerInput(1, ref Input.LHand);
		ApplyControllerInput(2, ref Input.RHand);
	}

	private void ApplyPoseInput(XRNode node, ref DeviceInput lastInput, ref DeviceInput input) {
		InputDevice device = InputDevices.GetDeviceAtXRNode(node);
		device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
		device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

		if (position != Vector3.zero) {
			input.position = position;
			input.rotation = rotation;
			input.gotFirstInput = true;
			if (!lastInput.gotFirstInput) { lastInput = input; }
		}
	}

	private static bool ApplyControllerInput(uint hand, ref DeviceInput input) {
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
