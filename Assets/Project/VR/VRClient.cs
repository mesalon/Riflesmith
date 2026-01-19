using UnityEngine;
using Valve.VR;

public class VRClient : MonoBehaviour {
	public void Awake() {
		print("Boot");
		EVRInitError initError = default;
		OpenVR.Init(ref initError);
		print(initError);
	}

	public void Update() {
		Texture_t tex = new() {
			handle = Texture2D.redTexture.GetNativeTexturePtr(),
			eType = ETextureType.Vulkan,
			eColorSpace = EColorSpace.Auto
		};
		VRTextureBounds_t bounds = new() { uMin = 0, vMin = 0, uMax = 1, vMax = 1 };
		EVRCompositorError LError = OpenVR.Compositor.Submit(EVREye.Eye_Left, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);
		EVRCompositorError RError = OpenVR.Compositor.Submit(EVREye.Eye_Right, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);
		print($"Submit. Error? {LError}, {RError}");
	}

	public void OnApplicationQuit() {
		print("Shutdown");
		OpenVR.Shutdown();
	}
}

