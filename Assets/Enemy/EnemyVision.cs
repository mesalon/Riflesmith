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
	private VisionCfg cfg => ctx.cfg.vision;
	private readonly float maxAngleCos;

	private PositionBuffer playerPositions;
	private Vector3 lastPlayerPos;
	private float hits;

	public EnemyVision(Blackboard ctx) {
		this.ctx = ctx;
		playerPositions = new(RoundToInt(1 / Time.fixedDeltaTime * cfg.movementWindow));
		maxAngleCos = Cos(Deg2Rad * cfg.viewAngleMax);
	}

	public struct DebugInfo {
		public Player detectedPlayer;
		public Vector3 a, b;
		public float angleChance, distanceChance, movementChance, totalChance, spotTime, dot, angle, angleChanceUnscaled, distanceChanceUnscaled, distance;
		public bool hasLoS;
	}
	public DebugInfo? info;

	// todo:
	// Vision should be impaired when moving
	// Right now it's actually better when moving because it sets off the movement detection
	public Player Tick() {
		Player p = null;
		info = null;
		DebugInfo newInfo = default;
		float chance = 0;
		foreach (Collider col in Physics.OverlapSphere(ctx.transform.position, cfg.sightRange, cfg.playerMask)) { // todo non alloc
			if (col.gameObject.activeInHierarchy && col.TryGetComponent(out Player player)) {
				UnityEngine.Debug.Log("Saw active player");
				p = player;
				newInfo.detectedPlayer = p;
				Vector3 eyePos = ctx.eyes.position, playerPos = player.rig.head.position;
				Vector3 a = ctx.eyes.forward, b = (playerPos - eyePos).normalized;
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
						newInfo.distance = Distance(eyePos, playerPos);
						newInfo.angleChance = angleChance;
						newInfo.distanceChance = distanceChance;
						newInfo.movementChance = movementChance;
						newInfo.totalChance = chance;
						newInfo.angle = Angle(a, b);
						newInfo.angleChanceUnscaled = angleChanceUnscaled;
						newInfo.distanceChanceUnscaled = distanceChanceUnscaled;
						newInfo.hasLoS = true;
					}
				}
				info = newInfo;
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
		string label = "No information.";
		if (info is DebugInfo i) {
			bool detected = hits > 0 && hits % cfg.consistencyFactor == 0;
			Color color = detected ? Color.white : Color.Lerp(new(0, 1, 0, 0.25f), new Color(1, 0, 0, 0.25f), i.spotTime.Remap(10, 0, 0, 1));
			Ext.DrawCubeLine(i.a, i.b, i.hasLoS ? color : new(0, 0, 0, 0.3f));
			label =
				$"AngleChance: {i.angleChance:F3}, DistanceChance: {i.distanceChance:F3}, Movement Chance: {i.movementChance:F3}\n" +
				//$"AngleChanceUnweighed: {i.angleChanceUnscaled:F4}, DistanceChanceUnweighed: {i.distanceChanceUnscaled:F4}\n" +
				$"Distance: {i.distance:F2}, Angle: {i.angle:F2}\n" +
				$"Chance: {i.totalChance:F3}, Average Spot Time: {i.spotTime:F3}{(detected ? ", Detected!" : "")}, Hits: {hits}\n";
		}
		Ext.Label(ctx.transform.position + 2 * Vector3.up, label);
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
