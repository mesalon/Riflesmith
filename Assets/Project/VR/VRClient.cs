using UnityEngine;
using Valve.VR;

public class VRClient : MonoBehaviour {
	TrackedDevicePose_t[] renderPoses = new TrackedDevicePose_t[3];
	TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[3];

	public void Awake() {
		print("Boot");
		EVRInitError initError = default;
		OpenVR.Init(ref initError);
		print(initError);
	}

	public void Update() {
		EVRCompositorError error = OpenVR.Compositor.WaitGetPoses(renderPoses, gamePoses);
		print($"Get Pose. Error? {error}");
		PrintPose(renderPoses[0]);
		PrintPose(renderPoses[1]);
		PrintPose(renderPoses[2]);
		return;

		Texture_t tex = new() {
			handle = Texture2D.redTexture.GetNativeTexturePtr(),
			eType = ETextureType.Vulkan,
			eColorSpace = EColorSpace.Auto
		};
		VRTextureBounds_t bounds = new() { uMin = 0, vMin = 0, uMax = 1, vMax = 1 };
		EVRCompositorError LError = OpenVR.Compositor.Submit(EVREye.Eye_Left, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);
		EVRCompositorError RError = OpenVR.Compositor.Submit(EVREye.Eye_Right, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);
		print($"Submit. Error? {LError}, {RError}");

		VREvent_t e = default;
		if (OpenVR.System.PollNextEvent(ref e, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t)))) {
			print($"Event: {(EVREventType)e.eventType}");
		}
	}

	public void OnApplicationQuit() {
		print("Shutdown");
		OpenVR.Shutdown();
	}
	void PrintPose(TrackedDevicePose_t dev) {
		HmdMatrix34_t m = dev.mDeviceToAbsoluteTracking;
		print(@$"Tracked device - Connected? {dev.bDeviceIsConnected}. Result? {dev.eTrackingResult}. Matrix below.
				{m.m1}, {m.m2}, {m.m3}, {m.m4}, {m.m5}, {m.m6}, {m.m7}, {m.m8}, {m.m9}, {m.m10}, {m.m11}, ");
	}
}
