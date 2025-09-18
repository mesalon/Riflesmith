using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// todo: HVT guards bring HVT to a safe location after taking fire
public class HVTMission : Mission {
    private Enemy HVT;
    private MissionPoint point;

    public override Vector3 CompassPosition {
        get => point.transform.position;
    }

    public override bool IsComplete => HVT.isDead;

    public HVTMission(MissionPoint point) {
        this.point = point;
        if (point.enemySpawns.Count > 0) {
            bool hvtSpawned = false;
            foreach (Transform t in point.enemySpawns.OrderBy(_ => Random.value).Take(5).ToList()) {
                if (!hvtSpawned) {
                    HVT = GameManager.SpawnEnemy(t.position, true);
                    hvtSpawned = true;
                }
                else { GameManager.SpawnEnemy(t.position); }
            }
            Debug.Log("Initialized");
        }
        else {
            Debug.Log("Mission point does not meet the requirements for an HVT mission.");
        }
    }

    public override void Tick() { }

    public override void Exit() {
        Debug.Log("Exited");
    }

}