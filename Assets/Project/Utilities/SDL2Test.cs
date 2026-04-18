using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class SDL2Test : MonoBehaviour {
	const string SDL2_LIB = "libSDL2-2.0.so.0";

	[DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
	private static extern int SDL_Init(uint flags);

	[DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
	private static extern void SDL_Quit();

	[DllImport(SDL2_LIB, CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr SDL_GetError();

	private const uint SDL_INIT_VIDEO = 0x00000020;

	void Start() {
		try {
			Debug.Log("[SDL2Test] Attempting to load SDL2...");

			int result = SDL_Init(SDL_INIT_VIDEO);

			if (result != 0) {
				string error = Marshal.PtrToStringAnsi(SDL_GetError());
				Debug.LogError($"[SDL2Test] SDL_Init failed (code {result}): {error}");
			}
			else {
				Debug.Log("[SDL2Test] SDL2 loaded and initialized successfully!");
				SDL_Quit();
			}
		}
		catch (DllNotFoundException e) {
			Debug.LogError($"[SDL2Test] DllNotFoundException — library not found: {e.Message}");
			Debug.LogError($"[SDL2Test] LD_LIBRARY_PATH = {Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}");
			Debug.LogError($"[SDL2Test] NIX_LD_LIBRARY_PATH = {Environment.GetEnvironmentVariable("NIX_LD_LIBRARY_PATH")}");
		}
		catch (Exception e) {
			Debug.LogError($"[SDL2Test] Unexpected error: {e}");
		}
	}
}
