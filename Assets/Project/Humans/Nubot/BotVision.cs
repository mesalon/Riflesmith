// Todo: Update actor body to have more points and blend vision based on how many can be seen
// Todo: light
using UnityEngine;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;
using System.Collections.Generic;

public struct Sighting {
	public float accumulator;
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
			if (target == ctx) continue;

			Vector3 targetPos = target.Center;
			Vector3 targetDir = (targetPos - eyes.position).normalized;

			float IFFScore = target.IFF != ctx.IFF ? 1 : 0;
			float LoSScore = HasLoS(target) ? 1 : 0;
			float angle = Angle(eyes.forward, targetDir);
			float angleScore = angle < FOVAngle ? Exp(-peripheryDecay * angle) : 0;
			float distance = Distance(eyes.position, targetPos);
			float distanceScore = distance < sightRange ? 1 / (1 + rangeDecay * distance.Sqr()) : 0;
			float motionScore = 0;
			/*
				 if (s.lastTargetPos != null) {
				 Vector3 lDir = (s.lastTargetPos.Value - eyes.position).normalized;
				 motionScore = baseScore * InverseLerp(0, maxMotion, Angle(targetDir, lDir) / Time.fixedDeltaTime) * motionSensitivity;
				 } 
				 s.lastTargetPos = targetPos;
				 */
			float score = IFFScore * LoSScore * angleScore * distanceScore * motionScore * masterSensitivity;
			score = (score - accumulatorDecay) * Time.fixedDeltaTime;

			if (visualDebug) {
				Debug.Log($"helno?");
				MyGizmos.Label(eyes.position, $"Angle: {angle:f2}, Distance: {distance:f2}\nScore: {score:f2}, Detection Time: {1/score:f2}s", Time.fixedDeltaTime);
			}
			if (!enemies.ContainsKey(target)) { // First time seeing this chump?
				if (score > 0) { enemies.TryAdd(target, new()); }
				else { continue; }
			}

			Sighting s = enemies[target];
			s.accumulator = Clamp01(s.accumulator - score);
			if (s.accumulator >= 1) {
				s.accumulator = 0;
				s.position = targetPos;
			}
			enemies[target] = s;

			if (visualDebug) {
				Color alpha = new(1, 1, 1, 0.5f);
				Color c = Colormaps.Plasma(InverseLerp(0, maxHeatmapRate, score));
				VRGizmos.Ray(eyes.position, eyes.forward * 50, Color.blue);
				Ext.DrawCubeLine(eyes.position, targetPos, c * alpha, Time.fixedDeltaTime);
				Ext.DrawCubeRay(target.transform.position, s.accumulator * (2 * up), Colormaps.Plasma(s.accumulator), Time.fixedDeltaTime, 0.2f);
			}

		}
	}

	private bool HasLoS(Human to) => !Physics.Linecast(eyes.position, to.Center, out var _, visionMask);
}
