using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleMission : Mission {
    private Puzzle triggerPuzzle;
    private List<Puzzle> toSolve = new();
    private MissionPoint point;
    private bool isStarted;

    public override Vector3 CompassPosition {
        get => point.transform.position;
    }

    public override bool IsComplete {
        get {
            if (!triggerPuzzle.isSolved) return false;
            foreach (Puzzle p in toSolve) {
                if (!p.isSolved) {
                    return false;
                }
            }
            return true;
        }
    }

    public PuzzleMission(MissionPoint point) {
        this.point = point;
        if (point.puzzleSpawns.Count > 0 && point.triggerSpawn) {
            triggerPuzzle = GameObject.Instantiate(point.puzzlePF, point.triggerSpawn.position, point.triggerSpawn.rotation);
            Debug.Log("Initialized");
        }
        else {
            Debug.Log("Mission point does not meet the requirements for a puzzle mission.");
        }
    }

    public override void Tick() {
        if (!isStarted && triggerPuzzle.isSolved) {
            Debug.Log("Spawning enemies");
            // Create guards. todo: Make these spawn at entry points and look for the player
            List<Transform> e = point.enemySpawns.OrderBy(_ => Random.value).Take(5).ToList();
            e.ForEach(s => GameManager.SpawnEnemy(s.position));

            // Create puzzles
            List<Transform> p = point.puzzleSpawns.OrderBy(_ => Random.value).Take(5).ToList();
            p.ForEach(s => toSolve.Add(GameObject.Instantiate(point.puzzlePF, s.position, s.rotation)));
            
            isStarted = true;
        }
    }

    public override void Exit() {
        foreach (Puzzle p in toSolve) { GameObject.Destroy(p.gameObject); }
        GameObject.Destroy(triggerPuzzle.gameObject);
        Debug.Log("Exited");
    }

}