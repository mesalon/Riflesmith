#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using System.IO;
#endif
using UnityEngine;
using System;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;

[Serializable] public class VisionCfg {
	public LayerMask playerMask;
	public LayerMask visionMask;
	public float sightRange;
	public float FOVAngle;
	public float detectionSpeed;
	public float stillPeripheryDecay;
	public float stillSensitivity;
	public float motionPeripheryDecay;
	public float motionSensitivity;
	public float maxAngleDetection;
	public float accumulatorDecay;
	public float rangeDecay;
	public bool showDebug;
	public Gradient heatmap;
	public float simMotion;
	public float maxHeatmapTime;
	public int debugSmoothing;

	public static readonly VisionCfg Default = new() {
		playerMask = ~0,
		visionMask = ~0,
		sightRange = 100,
		FOVAngle = 105,
		detectionSpeed = 1,
		stillPeripheryDecay = 0.15f,
		stillSensitivity = 3,
		motionPeripheryDecay = 0.02f,
		motionSensitivity = 5,
		maxAngleDetection = 15,
		accumulatorDecay = 0.25f,
		rangeDecay = 0.001f,
	};
}

public class EnemyVision {
	private VisionCfg cfg => ctx.cfg.vision;
	private readonly Blackboard ctx;
	private readonly float maxAngleCos;
	private readonly float maxViewAngle;
	private float accumulator;
	private Vector3? lastPlayerPos;
	MobileDebug debug; 

	public EnemyVision(Blackboard ctx) {
		this.ctx = ctx;
		maxAngleCos = Cos(cfg.FOVAngle * Deg2Rad);
		debug = cfg.showDebug ? new(cfg.debugSmoothing) : null;
	}

	public Player Tick() {
		Player detectedPlayer = null;
		Player player = GameManager.I.player;
		if (player.isActiveAndEnabled) {
			Vector3 eyePos = ctx.eyes.position; 
			Vector3 playerPos = player.rig.head.position;
			Vector3 playerDir = (playerPos - eyePos).normalized;
			if (Physics.Linecast(eyePos, playerPos, out RaycastHit hit, cfg.visionMask) && hit.transform.root == player.transform) {
				Vector3 lpd = lastPlayerPos.HasValue ? (lastPlayerPos.Value - eyePos).normalized : playerDir;
				float motion = Mathf.InverseLerp(0, cfg.maxAngleDetection, Vector3.Angle(playerDir, lpd) / Time.fixedDeltaTime);
				float angle = Vector3.Angle(ctx.eyes.forward, playerDir);
				debug?.Add("Angle", angle);
				debug?.Add("Motion", motion);
				if (ComputeRate(Vector3.Distance(eyePos, playerPos), Dot(ctx.eyes.forward, playerDir), angle, motion, out float rate, debug)) {
					lastPlayerPos = playerPos;
					accumulator += rate * Time.fixedDeltaTime;
					if (accumulator >= 1) {
						accumulator = 0;
						detectedPlayer = player;
					}

					float realRate = rate - cfg.accumulatorDecay;
					float spotTime = 1 / realRate;
					debug?.Add("Spot rate", realRate);
					debug?.Add("Spot time", spotTime);
					debug?.Add("Accumulator", accumulator);
					if (cfg.showDebug) { 
						Ext.Label(ctx.transform.position + 2.5f * Vector3.up, debug.ToString(), Time.fixedDeltaTime); 
						Color alpha = new(1, 1, 1, 0.5f);
						Color c = ColorForRate(rate);
						Ext.DrawCubeLine(ctx.eyes.position, playerPos, c * alpha, Time.fixedDeltaTime);
						Ext.DrawCubeRay(player.transform.position, accumulator * (2 * Vector3.up), c, Time.fixedDeltaTime, 0.5f);
						float detection = debug.fields["Spot time"].Average();
						Ext.Label(player.transform.position + 2 * Vector3.up, 
								$"{debug.fields["Accumulator"].Average() * 100:F1}% - detection in {(detection >= 0 ? detection : "NEVER"):F2}", Time.fixedDeltaTime);
					}
				} else {
					lastPlayerPos = null;
				}
			}
		}
		accumulator = Max(0, accumulator - cfg.accumulatorDecay * Time.fixedDeltaTime);
		return detectedPlayer;
	}


	public bool ComputeRate(float range, float dot, float angle, float motion, out float rate, MobileDebug debug = null) {
		rate = 0;
		if (dot > maxAngleCos && range < cfg.sightRange) {
			float rangeScore = 1 / (1 + cfg.rangeDecay * (range * range));
			float stillScore = Exp(-cfg.stillPeripheryDecay * angle) * cfg.stillSensitivity;
			float motionDecay = Exp(-cfg.motionPeripheryDecay * angle);
			float motionScore = motionDecay * motion * cfg.motionSensitivity;
			debug?.Add("motionDecay", motionDecay);
			debug?.Add("stillScore", stillScore);
			debug?.Add("rangeScore", rangeScore);
			rate = cfg.detectionSpeed * rangeScore * Max(stillScore, motionScore);
			return true;
		}
		return false;
	}

	private Color ColorForRate(float rate) {
		float t = rate >= 0 ? (1 / rate) - cfg.accumulatorDecay : cfg.maxHeatmapTime;
		return cfg.heatmap.Evaluate(InverseLerp(0, cfg.maxHeatmapTime, t));
	}

#if UNITY_EDITOR
	public void GenerateHeatmap() {
		long time = System.Diagnostics.Stopwatch.GetTimestamp();
		int textureSize = 512;
		float maxDistance = cfg.sightRange;
		Texture2D heatmap = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
		Color[] pixels = new Color[textureSize * textureSize];
		for (int y = 0; y < textureSize; y++) {
			for (int x = 0; x < textureSize; x++) {
				float xPos = ((float)x / (textureSize - 1) - 0.5f) * 2 * maxDistance;
				float zPos = ((float)y / (textureSize - 1) - 0.5f) * 2 * maxDistance;
				Color pixelColor;
				Vector3 targetPosition = new(xPos, 0, zPos);
				if (ComputeRate(targetPosition.magnitude, Dot(Vector3.forward, targetPosition.normalized), Angle(Vector3.forward, targetPosition.normalized), cfg.simMotion, out float rate)) {
					pixelColor = ColorForRate(rate);
				} else {
					pixelColor = Color.gray1;
				}

				pixels[y * textureSize + x] = pixelColor;
			}
		}

		heatmap.SetPixels(pixels);
		heatmap.Apply();
		byte[] bytes = heatmap.EncodeToPNG();
		string path = "Assets/Editor/vision_heatmap.png";
		File.WriteAllBytes(path, bytes);
		Debug.Log("Vision heatmap saved to: " + path);
		AssetDatabase.Refresh();
		GameObject.DestroyImmediate(heatmap);
		Ext.DebugTimestamp(time, "time");
	}
#endif
}
