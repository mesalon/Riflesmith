using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;
using UnityEditor;

public class EnemyFSM : MonoBehaviour {
// 	public NavMeshAgent agent;
// 	[FormerlySerializedAs("animator")] public Animator anim;
// 	public Vector3 lastPos;
// 	public Transform muzzle;
// 	public Player target;
// 	[Header("Movement and vision")] public Transform eyes;
// 	public Transform head;
// 	public float speed;
// 	public float patrolRadius = 10;
// 	public float sightRange = 100;
// 	public float viewAngleMax = 100;
// 	public Transform aimTarget;
// 	public LayerMask playerMask;
// 	public LayerMask visionMask;
// 	public float overallVisionFactor = 1;
// 	public int consistencyFactor = 2;
// 	[FormerlySerializedAs("spotChanceCurve")] [SerializeField] private AnimationCurve angleChanceCurve;
// 	[SerializeField] private AnimationCurve distanceChanceCurve;
// 	[Header("Weapons")] 
// 	[SerializeField] private Transform gun;
// 	public float weaponRange;
// 	public float fireInterval;
// 	public bool isFullAuto;
// 	public float spread;
// 	public ProjectileData projectile;
// 	public GUIStyle gui;
//
// 	[SerializeField] Enemy enemy;
// 	private float t;
// 	private Vector3 startPoint;
// 	private Vector3 walkPoint;
// 	private Vector3 lastPlayerPos;
// 	public enum EnemyType { Guard, Patrol }
// 	public EnemyType enemyType = EnemyType.Guard;
// 	
// 	public StateMachine fsm = new();
// 	private float maxAngleCos;
// 	[SerializeField] float movementWindow = 1f;
// 	private PositionBuffer playerPositions;
// 	
// #if UNITY_EDITOR
// 	[Serializable] public struct DebugInfo {
// 		public Player detectedPlayer;
// 		public float angleChance, distanceChance, movementChance, totalChance, spotTime, dot, angle, angleChanceUnscaled, 
// 			distanceChanceUnscaled;
// 		public bool hasLoS;
// 		public RaycastHit hit;
// 	}
// 	public DebugInfo info;
// #endif
// 	
// 	private void Awake() {
// 		maxAngleCos = Cos(Deg2Rad * viewAngleMax);
// 		playerPositions = new(RoundToInt(1 / Time.fixedDeltaTime * movementWindow));
// 		startPoint = transform.position;
// 	}
//
// 	private float hits;
// 	void OnDrawGizmos() {
// 		Ext.DrawCubeRay(head.position, -head.up, 30, new(0, 0, 1, 0.2f), 0.02f);
// 		Ext.DrawCubeRay(gun.position, gun.forward, 30, new(1, 0, 0, 0.2f), 0.005f);
// 		if (Application.isPlaying && head && info.detectedPlayer) {
// 			Vector3 start = head.position, end = info.detectedPlayer.rig.head.position;
// 			Color color;
// 			bool detected = hits > 0 && hits % consistencyFactor == 0;
// 			color = detected ? Color.white : Color.Lerp(new Color(0, 1, 0, 0.25f), new Color(1, 0, 0, 0.25f), info.spotTime.Remap(10, 0, 0, 1));
// 			Ext.DrawCubeLine(start, end, info.hasLoS ? color : new Color(0, 0, 0, 0.3f));
// 			string label =
// 				$"AngleChance: {info.angleChance:F3}, DistanceChance: {info.distanceChance:F3}, Movement Chance: {info.movementChance:F3}\n" +
// 				//$"AngleChanceUnweighed: {info.angleChanceUnscaled:F4}, DistanceChanceUnweighed: {info.distanceChanceUnscaled:F4}\n" +
// 				$"Distance: {Distance(start, end):F2}, Angle: {info.angle:F2}\n" +
// 				$"Chance: {info.totalChance:F3}, Average Spot Time: {info.spotTime:F3}{(detected ? ", Detected!" : "")}\n";
// 			Handles.Label((start + end) / 2 + up * 0.15f, label, gui);
// 		}	
// 	}
//
//
// 	
// 	public void VisionCheck() {
// 		if (hits % consistencyFactor == 0) { hits = 0; }
// 		Player p;
// 		DebugInfo i = new DebugInfo();
// 		foreach (Collider col in Physics.OverlapSphere(transform.position, sightRange, playerMask)) {
// 			if (col.TryGetComponent(out Player player)) {
// 				p = player;
// 				i.detectedPlayer = player;
// 				Vector3 headPos = head.position, playerPos = player.rig.head.position;
// 				Vector3 a = eyes.forward, b = (playerPos - headPos).normalized;
// 				i.angle = Angle(a, b);
// 				if (Physics.Linecast(headPos, playerPos, out RaycastHit hit, visionMask)) {
// 					if(hit.transform.root == player.transform) {
// 						float angleChanceUnscaled = Clamp01((Dot(a, b) - maxAngleCos) / (1 - maxAngleCos));
// 						float angleChance = angleChanceCurve.Evaluate(Clamp01(angleChanceUnscaled));
// 						float distanceChanceUnscaled = Clamp01(1f - Distance(headPos, playerPos) / sightRange);
// 						float distanceChance = distanceChanceCurve.Evaluate(Clamp01(distanceChanceUnscaled));
// 						
// 						playerPositions.Add(playerPos);
// 						Vector3[] posBuffer = playerPositions.GetPositions();
// 						float movementAngle = 1 + Angle((posBuffer[0] - headPos).normalized, (posBuffer[^1] - headPos).normalized);
// 						//Debug.DrawLine(headPos, posBuffer[0], Color.green);
// 						//Debug.DrawLine(headPos, posBuffer[^1], Color.green);
// 						float movementChance = movementAngle; // todo: enemy movement will set this off too, fix
// 						float chance = angleChance * distanceChance * movementChance * overallVisionFactor * Time.fixedDeltaTime * consistencyFactor;
// 						i.hit = hit;
// 						i.dot = movementAngle;
// 						i.angleChanceUnscaled = angleChanceUnscaled;
// 						i.distanceChanceUnscaled = distanceChanceUnscaled;
// 						i.totalChance = chance;
// 						i.angleChance = angleChance;
// 						i.distanceChance = distanceChance;
// 						i.hasLoS = true;
// 						i.movementChance = movementChance;
// 						i.spotTime = 1 / (chance / consistencyFactor) * Time.fixedDeltaTime;
// 					
// 						lastPlayerPos = p.rig.head.position;
// 					}
// 				}
// 				break;
// 			}
// 		}
// 		if (Random.value < info.totalChance) {
// 			hits++;
// 		}
// 		info = i;
// 	}
//
//
// 	
// 	public void Update() {
// 		VisionCheck();
// 	}
//
// 	public void Tick() {
// 		//fsm.Tick();
// 	}
}

/*
	public void VisionCheckDebug() {
   	Debug.Log("Begin Vision Tick");
   	foreach (Collider col in Physics.OverlapSphere(transform.position, sightRange, playerMask)) {
   		if (col.TryGetComponent(out Player player)) {
   			Debug.Log($"Found player object on {player.gameObject.name}");
   			Vector3 headPos = head.position;
   			Vector3 playerPos = player.rig.head.position;
   			Debug.DrawLine(headPos, playerPos, Color.blue);
   			if (Physics.Linecast(headPos, playerPos, out RaycastHit hit, visionMask) && hit.transform.root == player.transform) {
   				print($"Linecast collided with {hit.collider.gameObject.name}");
   				lastPlayerPos = player.rig.head.position;
   				Gizmos.color = Color.green;
   			}
   			else if(hit.collider) {
   				print($"No hit, obstructed by {hit.collider.name}");
   				Gizmos.color = Color.yellow;
   			}
   			else {
   				print("No hit");
   				Gizmos.color = Color.red;
   			}
   			Gizmos.DrawSphere(headPos, 0.5f);
   			Gizmos.DrawSphere(playerPos, 0.5f);
   		}
   	}
   	Debug.Log("End Vision Tick");
   }*/