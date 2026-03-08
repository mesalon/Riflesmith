// Todo: Update actor body to have more points and blend vision based on how many can be seen
// Todo: light
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
	public bool visualDebug;
	public Gradient heatmap;
	public float simMotion;
	public float maxHeatmapTime;

	public static readonly VisionCfg Default = new() {
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

public class BotVision {
	public bool HasLOS(Human to) => !Physics.Linecast(ctx.eyes.position, to.Center, out var _, cfg.visionMask);

	private VisionCfg cfg => ctx.cfg.vision;
	private readonly Bot ctx;
	private readonly float maxAngleCos;
	private float accumulator;
	private Vector3? lastPlayerPos;

	public BotVision(Bot ctx) {
		this.ctx = ctx;
		maxAngleCos = Cos(cfg.FOVAngle * Deg2Rad);
	}

	public bool Tick(out Human actor) {
		actor = null;
		Human target = null;
		Vector3 eyePos = ctx.eyes.position; 
		foreach(Collider col in Physics.OverlapSphere(eyePos, cfg.sightRange)) {
			if (col.TryGetComponent(out Human h) && h != ctx.self) {
				target = h;
				break;
			}
		}

		if (target && target.isActiveAndEnabled) {
			Vector3 playerPos = target.Center;
			Vector3 playerDir = (playerPos - eyePos).normalized;
			if (HasLOS(target)) {
				Vector3 lpd = lastPlayerPos.HasValue ? (lastPlayerPos.Value - eyePos).normalized : playerDir;
				float motion = InverseLerp(0, cfg.maxAngleDetection, Angle(playerDir, lpd) / Time.fixedDeltaTime);
				float angle = Angle(ctx.eyes.forward, playerDir);
				if (ComputeRate(Distance(eyePos, playerPos), Dot(ctx.eyes.forward, playerDir), angle, motion, out float rate)) {
					lastPlayerPos = playerPos;
					accumulator += rate * Time.fixedDeltaTime;
					if (accumulator >= 1) {
						accumulator = 0;
						actor = target;
						return true;
					}

					if (cfg.visualDebug) {
						Color alpha = new(1, 1, 1, 0.5f);
						Color c = ColorForRate(rate);
						Ext.DrawCubeLine(ctx.eyes.position, playerPos, c * alpha, Time.fixedDeltaTime);
						Ext.DrawCubeRay(target.transform.position, accumulator * (2 * up), c, Time.fixedDeltaTime, 0.5f);
					}
				} else {
					lastPlayerPos = null;
				}
			}
		}
		accumulator = Max(0, accumulator - cfg.accumulatorDecay * Time.fixedDeltaTime);
		return false;
	}


	public bool ComputeRate(float range, float dot, float angle, float motion, out float rate) {
		rate = 0;
		if (dot > maxAngleCos && range < cfg.sightRange) {
			float rangeScore = 1 / (1 + cfg.rangeDecay * (range * range));
			float stillScore = Exp(-cfg.stillPeripheryDecay * angle) * cfg.stillSensitivity;
			float motionDecay = Exp(-cfg.motionPeripheryDecay * angle);
			float motionScore = motionDecay * motion * cfg.motionSensitivity;
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
				if (ComputeRate(targetPosition.magnitude, Dot(forward, targetPosition.normalized), Angle(forward, targetPosition.normalized), cfg.simMotion, out float rate)) {
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
		string path = $"Assets/Editor/vision_heatmap.png";
		File.WriteAllBytes(path, bytes);
		Debug.Log("Vision heatmap saved to: " + path);
		AssetDatabase.Refresh();
		GameObject.DestroyImmediate(heatmap);
		Ext.LogTime(time, "time");
	}
#endif
}
