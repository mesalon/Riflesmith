// Todo: Update actor body to have more points and blend vision based on how many can be seen
// Todo: light
using UnityEngine;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;
using System.Collections.Generic;

public struct Sighting {
	public float accumulator;
	public Vector3? lastTargetPos;
	public Vector3? position;
}

[System.Serializable] public class BotVision {
	public Transform eyes;
	[SerializeField] LayerMask visionMask = ~0;
	[SerializeField] float sightRange = 100;
	[SerializeField] float FOVAngle = 105;
	[SerializeField] float masterSensitivity = 1;
	[SerializeField] float baseSensitivity = 5;
	[SerializeField] float motionSensitivity = 5;
	[SerializeField] float maxMotion = 15;
	[SerializeField] float peripheryDecay = 0.02f;
	[SerializeField] float rangeDecay = 0.001f;
	[SerializeField] float accumulatorDecay = 0.25f;
	[SerializeField] float maxHeatmapRate;
	[SerializeField] bool visualDebug;
	private Dictionary<Human, Sighting> enemies = new();
	private Nubot ctx;

	public void Init(Nubot ctx) {
		this.ctx = ctx;
	}

	public void FixedTick() {
		foreach (Human target in Human.All) {
			Vector3 targetPos = target.Center;
			Vector3 targetDir = (targetPos - eyes.position).normalized;
			float angle = Angle(eyes.forward, targetDir);
			float distance = Distance(eyes.position, targetPos);
			if (target.IFF != ctx.IFF && distance < sightRange && angle < FOVAngle && HasLOS(target) ) {
				enemies.TryAdd(target, new());
				Sighting s = enemies[target];

				/*
				float motionScore = 0;
				if (s.lastTargetPos != null) {
					Vector3 lDir = (s.lastTargetPos.Value - eyes.position).normalized;
					motionScore = baseScore * InverseLerp(0, maxMotion, Angle(targetDir, lDir) / Time.fixedDeltaTime) * motionSensitivity;
				} 
				s.lastTargetPos = targetPos;
				*/
				float baseScore = Exp(-peripheryDecay * angle) * baseSensitivity;
				float rangeScore = 1 / (1 + rangeDecay * distance.Sqr());
				float rate = baseScore * rangeScore * masterSensitivity;
				s.accumulator += rate * Time.fixedDeltaTime;
				if (s.accumulator >= 1) {
					s.accumulator = 0;
					s.position = targetPos;
				}

				if (visualDebug) {
					Color alpha = new(1, 1, 1, 0.5f);
					Color c = Colormaps.Plasma(InverseLerp(0, maxHeatmapRate, rate));
					VRGizmos.Ray(eyes.position, eyes.forward * 50, Color.blue);
					Ext.DrawCubeLine(eyes.position, targetPos, c * alpha, Time.fixedDeltaTime);
					Ext.DrawCubeRay(target.transform.position, s.accumulator * (2 * up), Colormaps.Plasma(s.accumulator), Time.fixedDeltaTime, 0.5f);

					Ext.Label(eyes.position, $"Angle: {angle:f2}, Distance: {distance:f2}\nBase: {baseScore:f2}, Range: {rangeScore:f2}\nDetection Time: {1/rate:f2}s");
				}

				s.accumulator = Max(0, s.accumulator - accumulatorDecay * Time.fixedDeltaTime);
				enemies[target] = s;
			}
		}
	}

	private bool HasLOS(Human to) => !Physics.Linecast(eyes.position, to.Center, out var _, visionMask);
}
