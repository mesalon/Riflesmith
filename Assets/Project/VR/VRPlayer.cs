using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;
using Debug = UnityEngine.Debug;
// todo: make it so when a player spawns he keeps facing the way he does instead of snapping to the raw rotation of the head

public class VRPlayer : MonoBehaviour, IVRAnchorProvider {
	public Pose Anchor { get; set; }

	public static VRInput Input, LastInput;
	public static IVRAnchorProvider anchorProvider;
	public static new Camera camera;
	private static readonly uint size = (uint)Marshal.SizeOf<VRControllerState_t>();
	private Process VRProbe;

	private void CreateProbe() {
		string probeExe = Application.platform == RuntimePlatform.WindowsPlayer ? "winprobe/VRProbe.exe" : "linprobe/VRProbe";
		VRProbe = Process.Start(new ProcessStartInfo(Path.Combine(Application.streamingAssetsPath, probeExe)) { 
				UseShellExecute = false,
				RedirectStandardOutput = true,
				});
		VRProbe.OutputDataReceived += (sender, e) => { Debug.Log($"[VRProbe] {e.Data}"); };
		VRProbe.BeginOutputReadLine();
	}

	void Awake() {
		camera = GetComponent<Camera>();
		Anchor = new(transform.position, transform.rotation);
		CreateProbe();
		LastInput = Input = VRInput.TPose;
	}

	void OnDisable() { Application.onBeforeRender -= UpdateInput; }
	void OnDestroy() {
		if (VRProbe != null && !VRProbe.HasExited) { VRProbe.Kill(); }
		StopVR();
	}

	void Update() {
		// I am going to flay you with a cheese grater.
		if (VRProbe != null && VRProbe.HasExited) {
			if (VRProbe.ExitCode == 0) {
				VRProbe = null;
				StartVR();
			}
		}
	}

	public void StartVR() {
		print("Initializing Loader...");
		XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
		if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
			Debug.LogError("Initializing VR Failed, no active loader selected.");
		} else {
			print($"Loaded {XRGeneralSettings.Instance.Manager.activeLoader}");
			print("Starting VR Subsystems...");
			try {
				XRGeneralSettings.Instance.Manager.StartSubsystems();
				Application.onBeforeRender += UpdateInput;
			} catch (NullReferenceException e) {
				print($"Failed to start subsystems.\n{e}");
			}
			print("Started VR Subsystems");
		}
	}

	void StopVR() {
		print("Stopping VR...");
		XRGeneralSettings.Instance.Manager.StopSubsystems();
		XRGeneralSettings.Instance.Manager.DeinitializeLoader();
		Application.onBeforeRender -= UpdateInput;
		print("VR Stopped");
	}

	[BeforeRenderOrder(-30000)]
	public void UpdateInput() {
		LastInput = Input;

		ApplyPoseInput(XRNode.CenterEye, ref LastInput.head, ref Input.head);
		ApplyPoseInput(XRNode.LeftHand, ref LastInput.LHand, ref Input.LHand);
		ApplyPoseInput(XRNode.RightHand, ref LastInput.RHand, ref Input.RHand);
		ApplyControllerInput(1, ref Input.LHand);
		ApplyControllerInput(2, ref Input.RHand);

		if (anchorProvider as UnityEngine.Object != null) {
			transform.position = anchorProvider.Anchor.position;
			transform.rotation = anchorProvider.Anchor.rotation * Input.head.rotation;
		} else {
			anchorProvider = this; // You are an anchor provider of last resort.
		}
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

public struct VRInput {
	public static VRInput TPose => new() {
		head = new() { position = new(0, 1.8f, 0), rotation = Quaternion.identity },
		LHand = new() { position = new(-0.5f, 0.5f, 0), rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right) },
		RHand = new() { position = new(0.5f, 0.5f, 0), rotation = Quaternion.LookRotation(Vector3.forward, Vector3.left) },
	};
	public DeviceInput head, LHand, RHand;
}

public struct DeviceInput {
	public readonly bool IsValid => position != Vector3.zero;
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

public interface IVRAnchorProvider {
	public Pose Anchor { get; }
}
