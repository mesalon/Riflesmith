using UnityEngine;
using Valve.VR;

public class VRClient : MonoBehaviour {
	[SerializeField] Camera cam;
	private TrackedDevicePose_t[] renderPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
		TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

	public void Awake() {
		EVRInitError initError = default;
		OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref initError);
		print($"Get generic interface: {initError}");

		EVRInitError error = EVRInitError.None;
		OpenVR.Init(ref error, EVRApplicationType.VRApplication_Scene);
		print($"Init: {error}");
	}

	public void Update() {
		EVRCompositorError error = OpenVR.Compositor.WaitGetPoses(renderPoses, gamePoses);
		print($"Get Pose. Error? {error}");
		foreach (TrackedDevicePose_t pose in renderPoses) {
			PrintPose(pose);
		}

		VREvent_t e = default;
		if (OpenVR.System.PollNextEvent(ref e, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t)))) {
			print($"Event: {(EVREventType)e.eventType}");
		}

		Texture_t tex = new() {
			handle = cam.activeTexture.GetNativeTexturePtr(),
			eType = ETextureType.Vulkan,
			eColorSpace = EColorSpace.Auto
		};
		Texture_t tex2 = new() {
			handle = cam.activeTexture.GetNativeTexturePtr(),
			eType = ETextureType.Vulkan,
			eColorSpace = EColorSpace.Auto
		};
		VRTextureBounds_t bounds = new() { uMin = 0, uMax = 1, vMin = 0, vMax = 1 };
		EVRCompositorError LError = OpenVR.Compositor.Submit(EVREye.Eye_Left, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);
		EVRCompositorError RError = OpenVR.Compositor.Submit(EVREye.Eye_Right, ref tex2, ref bounds, EVRSubmitFlags.Submit_Default);
		print($"Submit. Error? {LError}, {RError}");
	}

	public void OnApplicationQuit() {
		print("Shutdown");
		OpenVR.Shutdown();
	}
	void PrintPose(TrackedDevicePose_t dev) {
		//HmdMatrix34_t m = dev.mDeviceToAbsoluteTracking;
		//print(@$"Tracked device - Connected? {dev.bDeviceIsConnected}. Result? {dev.eTrackingResult}. Matrix below.
		//		{m.m1}, {m.m2}, {m.m3}, {m.m4}, {m.m5}, {m.m6}, {m.m7}, {m.m8}, {m.m9}, {m.m10}, {m.m11}, ");
	}
}
