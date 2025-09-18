using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;

public enum EnemyType { Guard, Patrol, Static }

public class EnemyAI : Tree {
	public EnemySettings settings;
	public EnemyLocomotion locomotion;
	public Player target;

	[Header("Weapons")] 
	public EnemyFirearm weapon;
	public GUIStyle gui;
	public float aimTolerance;
	public ProjectileData projectile;

	[Header("Movement")] 
	public AnimationCurve angleChanceCurve;
	public AnimationCurve distanceChanceCurve;

	[Header("Vision")]
	public Transform eyes;
	public LayerMask playerMask;
	public LayerMask visionMask;
	public float sightRange = 100;
	public float viewAngleMax = 100;
	public float overallVisionFactor = 1;
	public int consistencyFactor = 2;
	
	[Header("Burst")] public bool enableFire;
	[Range(0, 1)] public float range, skill, ammo, intent, recoil, panic;
	private float maxAngleCos;
	private float hits;
	private Vector3 lastPlayerPos;
	private PositionBuffer playerPositions;
	public EnemyType enemyType = EnemyType.Guard;
	public float movementWindow = 1f;
	public Transform threat;
	private CoverQuery query;
	private CoverQuery[] que = new CoverQuery[32];

	[SerializeField] CoverParams coverCfg = CoverParams.Default;
	
	private void Awake() {
		maxAngleCos = Cos(Deg2Rad * viewAngleMax);
		playerPositions = new(RoundToInt(1 / Time.fixedDeltaTime * movementWindow));
	}

	public void ResetCover() {
		interval = System.Diagnostics.Stopwatch.GetTimestamp();
		for (int i = 0; i < que.Length; i++) { que[i] = new(transform.position, threat.position, 1.5f, CoverParams.Default); }
		query = new(transform.position, threat.position, 1.5f, coverCfg);
	}

	protected override Node SetupTree() {
		return new SelectorNode(new() {
			new SelectorNode(new() {
				new EngageEnemyTask(this),
			}),
			new PatrolTask(this),
			new DoNothingTask(this)
		});
	}

	public float urgency, aggresion;
	long interval;
	private new void Update() {
		base.Update();
		if (VisionCheck() is { } p) target = p;
		if (debugs) Ext.Label(transform.position + up * 2.3f, $"Target: {(target ? target.name : "None")}");
		if(target) Ext.DrawAxis(target.transform.position);

		if(query != null) {
			query.FindCover();
			if (query.GetBestPoint(urgency, aggresion, out Vector3 cover)) {
				locomotion.MoveTo(cover);
			}
		}
	}

	public Vector3 GetTargetPos() {
		if (target) { return target.rig.head.position; }
		return default;
	}

	private Player VisionCheck() {
		info = default;
		Player p = null;
		float chance = 0;
		foreach (Collider col in Physics.OverlapSphere(transform.position, sightRange, playerMask)) { // todo non alloc
			if (col.TryGetComponent(out Player player)) {
				p = player;
				info.detectedPlayer = p;
				Vector3 eyePos = eyes.position, playerPos = player.rig.head.position;
				Vector3 a = eyes.forward, b = (playerPos - eyePos).normalized;
				if (Physics.Linecast(eyePos, playerPos, out RaycastHit hit, visionMask)) {
					if(hit.transform.root == player.transform) {
						float angleChanceUnscaled = Clamp01((Dot(a, b) - maxAngleCos) / (1 - maxAngleCos));
						float angleChance = angleChanceCurve.Evaluate(Clamp01(angleChanceUnscaled));
						float distanceChanceUnscaled = Clamp01(1f - Distance(eyePos, playerPos) / sightRange);
						float distanceChance = distanceChanceCurve.Evaluate(Clamp01(distanceChanceUnscaled));
						
						playerPositions.Add(playerPos);
						Vector3[] posBuffer = playerPositions.GetPositions();
						float movementAngle = 1 + Angle((posBuffer[0] - eyePos).normalized, (posBuffer[^1] - eyePos).normalized);
						float movementChance = movementAngle; // todo: enemy movement will set this off too, fix
						chance = angleChance * distanceChance * overallVisionFactor * Time.fixedDeltaTime * consistencyFactor;
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
		if (hits > 0 && hits % consistencyFactor == 0) {
			hits = 0; 
			return p;
		}
		return null;
	}
	
#if UNITY_EDITOR
	public bool debugs;

	[Serializable] public struct DebugInfo {
		public Player detectedPlayer;
		public float angleChance, distanceChance, movementChance, totalChance, spotTime, dot, angle, angleChanceUnscaled, distanceChanceUnscaled;
		public bool hasLoS;
	}
	public DebugInfo info;
#endif
   
	private new void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (!debugs) return;
		Ext.DrawCubeRay(eyes.position, -eyes.up, 30, new(0, 0, 1, 0.2f), 0.02f);
		Ext.DrawCubeRay(weapon.transform.position, weapon.transform.forward, 30, new(1, 0, 0, 0.2f), 0.005f);
		if (Application.isPlaying && eyes && info.detectedPlayer) {
			Vector3 start = eyes.position, end = info.detectedPlayer.rig.head.position;
			bool detected = hits > 0 && hits % consistencyFactor == 0;
			Color color = detected ? Color.white : Color.Lerp(new(0, 1, 0, 0.25f), new Color(1, 0, 0, 0.25f), info.spotTime.Remap(10, 0, 0, 1));
			Ext.DrawCubeLine(start, end, info.hasLoS ? color : new(0, 0, 0, 0.3f));
			string label =
				$"AngleChance: {info.angleChance:F3}, DistanceChance: {info.distanceChance:F3}, Movement Chance: {info.movementChance:F3}\n" +
				//$"AngleChanceUnweighed: {info.angleChanceUnscaled:F4}, DistanceChanceUnweighed: {info.distanceChanceUnscaled:F4}\n" +
				$"Distance: {Distance(start, end):F2}, Angle: {info.angle:F2}\n" +
				$"Chance: {info.totalChance:F3}, Average Spot Time: {info.spotTime:F3}{(detected ? ", Detected!" : "")}, Hits: {hits}\n";
			Handles.Label((start + end) / 2 + up * 0.15f, label, gui);
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
