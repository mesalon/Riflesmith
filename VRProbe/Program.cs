using Valve.VR;
using System;

Console.WriteLine("Initializing OpenVR Probe");
EVRInitError error = EVRInitError.Unknown;
while (error != EVRInitError.None) {
	OpenVR.Init(ref error, EVRApplicationType.VRApplication_Scene);
	Console.WriteLine($"Init finished with {error}");
};
Console.WriteLine($"Connection success!");
