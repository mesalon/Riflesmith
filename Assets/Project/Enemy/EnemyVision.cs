using UnityEngine;
using System;
using System.Text;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;

[Serializable] public class VisionCfg {
	public LayerMask playerMask;
	public LayerMask visionMask;
	public float sightRange = 100;
	public float FOVAngle = 105;
	public float overallVisionFactor = 1;
	public int consistencyFactor = 2;
	public float movementWindow = 1f;
	public float staticPeripheryDecay = 0.2f;
	public float motionPeripheryDecay = 0.02f;
	public bool showDebug;
}

public class EnemyVision {
	private VisionCfg cfg => ctx.cfg.vision;
	private readonly Blackboard ctx;
	private readonly float maxAngleCos;
	private readonly float maxViewAngle;

	private Vector3 lastPlayerPos;
	private float hits;

	public EnemyVision(Blackboard ctx) {
		this.ctx = ctx;
		maxAngleCos = Cos(cfg.FOVAngle * Deg2Rad);
	}

	// todo:
	// Vision should be impaired when moving
	// Right now it's actually better when moving because it sets off the movement detection
	public Player Tick() {
		Player p = null;
		StringBuilder debug = cfg.showDebug ? new() : null;
		float chance = 0;
		foreach (Collider col in Physics.OverlapSphere(ctx.transform.position, cfg.sightRange, cfg.playerMask)) { // todo non alloc
			if (col.gameObject.activeInHierarchy && col.TryGetComponent(out Player player)) {
				p = player;
				Vector3 eyePos = ctx.eyes.position, playerPos = player.rig.head.position;
				if (Physics.Linecast(eyePos, playerPos, out RaycastHit hit, cfg.visionMask)) {
					if(hit.transform.root == player.transform) {
						Vector3 a = ctx.eyes.forward, b = (playerPos - eyePos).normalized;
						float angle = (Dot(a, b));
						float staticChance = Mathf.Exp(cfg.staticPeripheryDecay * angle);
						float movingChance = Mathf.Exp(cfg.motionPeripheryDecay * angle);
						debug?.AppendLines($"Angle: {angle}", $"{staticChance}", $"{movingChance}");

						//chance = angleChance * distanceChance * cfg.overallVisionFactor * Time.fixedDeltaTime * cfg.consistencyFactor;
						lastPlayerPos = p.rig.head.position;
					}
				}
				if (debug != null) { Ext.Label(ctx.transform.position, debug.ToString()); }
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
