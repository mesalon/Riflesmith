using UnityEngine;
using UnityEditor;
using System;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;

[Serializable] public class VisionCfg {
	public AnimationCurve angleChanceCurve;
	public AnimationCurve distanceChanceCurve;
	public LayerMask playerMask;
	public LayerMask visionMask;
	public float sightRange = 100;
	public float viewAngleMax = 100;
	public float overallVisionFactor = 1;
	public int consistencyFactor = 2;
	public float movementWindow = 1f;
}

public class EnemyVision {
	private readonly Blackboard ctx;
	private readonly VisionCfg cfg;
	private readonly Transform eyes;
	private readonly float maxAngleCos;

	private PositionBuffer playerPositions;
	private Vector3 lastPlayerPos;
	private float hits;

	public EnemyVision(Blackboard ctx) {
		this.ctx = ctx;
		cfg = ctx.cfg.vision;
		playerPositions = new(RoundToInt(1 / Time.fixedDeltaTime * cfg.movementWindow));
		maxAngleCos = Cos(Deg2Rad * cfg.viewAngleMax);
	}

	public struct DebugInfo {
		public Player detectedPlayer;
		public float angleChance, distanceChance, movementChance, totalChance, spotTime, dot, angle, angleChanceUnscaled, distanceChanceUnscaled;
		public bool hasLoS;
	}
	public DebugInfo info;

	public Player Tick() {
		info = default;
		Player p = null;
		float chance = 0;
		foreach (Collider col in Physics.OverlapSphere(ctx.transform.position, cfg.sightRange, cfg.playerMask)) { // todo non alloc
			if (col.TryGetComponent(out Player player)) {
				p = player;
				info.detectedPlayer = p;
				Vector3 eyePos = eyes.position, playerPos = player.rig.head.position;
				Vector3 a = eyes.forward, b = (playerPos - eyePos).normalized;
				if (Physics.Linecast(eyePos, playerPos, out RaycastHit hit, cfg.visionMask)) {
					if(hit.transform.root == player.transform) {
						float angleChanceUnscaled = Clamp01((Dot(a, b) - maxAngleCos) / (1 - maxAngleCos));
						float angleChance = cfg.angleChanceCurve.Evaluate(Clamp01(angleChanceUnscaled));
						float distanceChanceUnscaled = Clamp01(1f - Distance(eyePos, playerPos) / cfg.sightRange);
						float distanceChance = cfg.distanceChanceCurve.Evaluate(Clamp01(distanceChanceUnscaled));
						
						playerPositions.Add(playerPos);
						Vector3[] posBuffer = playerPositions.GetPositions();
						float movementAngle = 1 + Angle((posBuffer[0] - eyePos).normalized, (posBuffer[^1] - eyePos).normalized);
						float movementChance = movementAngle; // todo: enemy movement will set this off too, fix
						chance = angleChance * distanceChance * cfg.overallVisionFactor * Time.fixedDeltaTime * cfg.consistencyFactor;
						lastPlayerPos = p.rig.head.position;
						info.angleChance = angleChance;
						info.distanceChance = distanceChance;
						info.movementChance = movementChance;
						info.totalChance = chance;
						info.angle = Angle(a, b);
						info.angleChanceUnscaled = angleChanceUnscaled;
						info.distanceChanceUnscaled = distanceChanceUnscaled;
						info.hasLoS = true;
					}
				}
				break;
			}
		}
		if (Random.value < chance) { hits++; }
		if (hits > 0 && hits % cfg.consistencyFactor == 0) {
			hits = 0; 
			return p;
		}
		return null;
	}

	public void Debug() {
		if (Application.isPlaying && eyes && info.detectedPlayer) {
			Vector3 start = eyes.position, end = info.detectedPlayer.rig.head.position;
			bool detected = hits > 0 && hits % cfg.consistencyFactor == 0;
			Color color = detected ? Color.white : Color.Lerp(new(0, 1, 0, 0.25f), new Color(1, 0, 0, 0.25f), info.spotTime.Remap(10, 0, 0, 1));
			Ext.DrawCubeLine(start, end, info.hasLoS ? color : new(0, 0, 0, 0.3f));
			string label =
				$"AngleChance: {info.angleChance:F3}, DistanceChance: {info.distanceChance:F3}, Movement Chance: {info.movementChance:F3}\n" +
				//$"AngleChanceUnweighed: {info.angleChanceUnscaled:F4}, DistanceChanceUnweighed: {info.distanceChanceUnscaled:F4}\n" +
				$"Distance: {Distance(start, end):F2}, Angle: {info.angle:F2}\n" +
				$"Chance: {info.totalChance:F3}, Average Spot Time: {info.spotTime:F3}{(detected ? ", Detected!" : "")}, Hits: {hits}\n";
			Handles.Label((start + end) / 2 + up * 0.15f, label);
		}	
	}
}

public class PositionBuffer {
	private Vector3[] playerPositions;
	private int maxSize;
	private int currentIndex;
	private bool isFull;

	public PositionBuffer(int size) {
		maxSize = size;
		playerPositions = new Vector3[size];
	}

	public void Add(Vector3 position) {
		playerPositions[currentIndex] = position;
		currentIndex = (currentIndex + 1) % maxSize;
		if (currentIndex == 0) isFull = true;
	}

	public Vector3[] GetPositions() {
		if (!isFull && currentIndex == 0) return Array.Empty<Vector3>();

		Vector3[] orderedPositions = new Vector3[isFull ? maxSize : currentIndex];
		int index = isFull ? currentIndex : 0;
		for (int i = 0; i < orderedPositions.Length; i++) {
			orderedPositions[i] = playerPositions[(index + i) % maxSize];
		}
		return orderedPositions;
	}
}
