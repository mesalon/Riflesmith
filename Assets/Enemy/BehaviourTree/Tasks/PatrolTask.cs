using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;


public class PatrolTask : Node {
	private readonly Blackboard ctx;
	private readonly EnemyLocomotion locomotion;
	private float t;
	private List<Vector3> patrolPath = new();
	private int currentPoint;
	private bool isPatrollingBackwards; // or forwards in the order of patrol points
	private float[,] distMap = new float[GameManager.I.patrolPoints.Count, GameManager.I.patrolPoints.Count]; // Path distance from every point, to any point. todo: Cache this in some kind of global data before runtime
	private Vector3[,][] pathMap = new Vector3[GameManager.I.patrolPoints.Count, GameManager.I.patrolPoints.Count][];

	private List<Transform> points => GameManager.I.patrolPoints;

	public PatrolTask(Enemy ctx) {
		this.ctx = ctx.blackboard;
		this.locomotion = ctx.locomotion;
		for (int i = 0; i < points.Count; i++) {
			for (int j = 0; j < points.Count; j++) {
				if (i == j) {
					pathMap[i, j] = Array.Empty<Vector3>();
					continue;
				}

				NavMeshPath path = new NavMeshPath();
				NavMesh.CalculatePath(points[i].position, points[j].position, NavMesh.AllAreas, path);
				pathMap[i, j] = path.corners;
				distMap[i, j] = GetPathLength(path.corners);
			}
		}
		patrolPath = NearestNeighbor();
	}

	public override NodeState Evaluate(out Node active) {
		active = this;
		locomotion.MoveTo(patrolPath[currentPoint]);
		if (locomotion.didArrive) { currentPoint++; }
		return NodeState.Running;
	}


	List<Vector3> NearestNeighbor() {
		List<Vector3> patrolPath = new();
		List<int> unvisited = new(Enumerable.Range(0, points.Count));
		int currentPos = 1;
		int stepCount = 0;

		while (unvisited.Count > 0) {
			float minDistance = float.MaxValue;
			int nearestIdx = 0;

			foreach (int i in unvisited) {
				float d = currentPos == -1 ? (points[i].position - ctx.transform.position).sqrMagnitude : distMap[currentPos, i];
				if (d < minDistance) {
					nearestIdx = i;
					minDistance = d;
				}
			}

			if (currentPos != -1) {
				Vector3[] path = pathMap[currentPos, nearestIdx];
				patrolPath.AddRange(path);
				stepCount++;
			}
			currentPos = nearestIdx;
			unvisited.Remove(nearestIdx);
		}
		return patrolPath;
	}

	private float GetPathLength(Vector3[] path) {
		float d = 0;
		for (int i = 0; i < path.Length - 1; i++) {
			d += (path[i] - path[i + 1]).sqrMagnitude;
		}

		return d;
	}
}
