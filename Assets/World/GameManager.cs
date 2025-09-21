using System;
using System.Collections.Generic;
using UnityEditor;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static Camera Camera => Camera.allCameras
		.Where(c => c.targetTexture == null)
		.OrderByDescending(c => c.depth)
		.FirstOrDefault();
	[Range(0, 20)] public int var1;
	public static GameManager I { get; private set; }
	public WorldSettings settings;
	public float enemyTier;
	public List<Transform> patrolPoints = new();
	
	[SerializeField] float tierIncrease;
	[SerializeField] List<Transform> spawnPoints;
	[SerializeField] List<MissionPoint> missionPoints;
	[SerializeField] GameObject player;
	[SerializeField] List<GameObject> spawnGear;
	[SerializeField] Enemy enemy;
	[SerializeField] Enemy HVT;
	[SerializeField] bool spawnPlayer;
	public Mission CurrentMission { get; private set; }

	public enum MissionType {
		Puzzle, HVT, Elimination
	}
	[SerializeField] private MissionType availableMissions;
	
	private void Awake() {
		if (I == null) {
			I = this;
			DontDestroyOnLoad(gameObject);
		}
		else { Destroy(gameObject); }
	}
	
	void Start() {
		int i = Random.Range(0, spawnPoints.Count);
		if (spawnPoints.Count > i) {
			Transform spawn = spawnPoints[i];
			if(spawnPlayer) Instantiate(player, spawn.position, spawn.rotation);
			foreach (GameObject go in spawnGear) {
				Instantiate(go, spawn.position, spawn.rotation);
			}
		}
		GetMission();
	}

	void Update() {
		if (CurrentMission != null) {
			CurrentMission.Tick();
			if (CurrentMission.IsComplete) {
				print("Mission Complete");
				CurrentMission.Exit();
				CurrentMission = null;
				GetMission();
			}
		}
		if(Input.GetKeyDown(KeyCode.K)) {
			foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None)) {
				enemy.body.Damage(100, 0);
			}
		}
	}

	void GetMission() {
		int i = Random.Range(0, missionPoints.Count);
		if (missionPoints.Count > i) {
			CurrentMission = availableMissions switch {
				MissionType.Puzzle => new PuzzleMission(missionPoints[i]),
				MissionType.HVT => new HVTMission(missionPoints[i]),
			};
			enemyTier = Mathf.Clamp(enemyTier + tierIncrease, 0, 3);
			print("New mission assigned");
		}
		
		// Patrols
		for (int ii = 0; ii < 3; ii++) {
			//SpawnEnemy(patrolPoints[Random.Range(0, patrolPoints.Count)].position);
		}
	}

	private void OnDrawGizmos() {
		for (int i = Ext.labelRequests.Count - 1; i >= 0; i--) {
			LabelRequest r = Ext.labelRequests[i];
			r.style ??= new() {
				alignment = TextAnchor.MiddleCenter, 
				normal = new() { textColor = r.color == default ? Color.white : r.color }
			};
			Handles.Label(r.position, r.text, r.style);
			Ext.labelRequests.RemoveAt(i);
		}
		for (int i = Ext.drawQueue.Count - 1; i >= 0; i--) {
			Ext.drawQueue[i]();
			Ext.drawQueue.RemoveAt(i);
		}
	}

	public static Enemy SpawnEnemy(Vector3 position, bool hvt = false) {
		bool badSpot = false;
		Collider[] overlap = Physics.OverlapCapsule(position, position + Vector3.up * 1.8f, 0.5f);
		foreach (Collider col in overlap) {
			if (col.TryGetComponent(out Limb _)) {
				badSpot = true;
				break;
			}
		}
		return Instantiate(hvt ? I.HVT : I.enemy, badSpot ? position + Vector3.up * 1.8f: position, Quaternion.Euler(0, Random.Range(0, 360), 0));
	}
}
