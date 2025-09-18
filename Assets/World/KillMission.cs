using System.Collections.Generic;
using UnityEngine;

/*public class KillMission : Mission {
	[SerializeField] List<Transform> spawns;
	public List<Enemy> toKill;

	public override bool IsComplete {
		get {
			foreach (Enemy e in toKill) {
				if (e.health > 0) { return false; }
			}
			return true;
		}
	}
	
	
	List<Vector3> usedPositions = new List<Vector3>();
	public override void Initialize() {
		for (int i = 0; i < 5; i++) {
			Vector3 spawnPos;
			do { spawnPos = spawns[Random.Range(0, spawns.Count)].position; } 
			while (usedPositions.Contains(spawnPos));
			usedPositions.Add(spawnPos);
			toKill.Add(GameManager.SpawnEnemy(spawnPos));
		}
	}
}*/