using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Management;
using Debug = UnityEngine.Debug;
// todo: make it so when a player spawns he keeps facing the way he does instead of snapping to the raw rotation of the head

public class VRPlayer : MonoBehaviour, IVRAnchorProvider {
	public Pose Anchor { get; set; }
	private bool UseMockHMD => inputProvider is MockInputProvider;

	public static VRInput Input, LastInput;
	public static IVRAnchorProvider anchorProvider;
	public static new Camera camera;
	[SerializeField] VRInputProvider inputProvider;
	private Process VRProbe;

	void Awake() {
		camera = GetComponent<Camera>();
		Anchor = new(transform.position, transform.rotation);
		if (!UseMockHMD) {
			CreateProbe();
		} else {
			Application.onBeforeRender += UpdateInput;
		}
		LastInput = Input = VRInput.TPose;
	}

	void OnDisable() { Application.onBeforeRender -= UpdateInput; }
	void OnDestroy() {
		if (!UseMockHMD) {
			if (VRProbe != null && !VRProbe.HasExited) { VRProbe.Kill(); }
			StopVR();
		}
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

	private void CreateProbe() {
		string probeExe = Application.platform == RuntimePlatform.WindowsPlayer ? "winprobe/VRProbe.exe" : "linprobe/VRProbe";
		VRProbe = Process.Start(new ProcessStartInfo(Path.Combine(Application.streamingAssetsPath, probeExe)) { 
				UseShellExecute = false,
				RedirectStandardOutput = true,
				});
		VRProbe.OutputDataReceived += (sender, e) => { Debug.Log($"[VRProbe] {e.Data}"); };
		VRProbe.BeginOutputReadLine();
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
		inputProvider.GetInput(ref LastInput, ref Input);
		// You are an anchor provider of last resort.
		if (anchorProvider as UnityEngine.Object == null) { anchorProvider = this; }
		transform.position = anchorProvider.Anchor.position;
		transform.rotation = anchorProvider.Anchor.rotation * Input.head.rotation;
	}

}

public interface IVRAnchorProvider {
	public Pose Anchor { get; }
}
