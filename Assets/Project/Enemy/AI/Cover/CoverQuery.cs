using Debug = UnityEngine.Debug;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[System.Serializable] public struct CoverParams {
	public float range;
	public float navRange;
	public float minDot;
	public float bodyWidth;
	public float bodyHeight;
	public bool doDebugs;
	public int maxPoints;
	public int maxSplats;
	public int navBatch;
	public int splatBatch;
	public int threatBreadth;
	public int skepticism;
	public LayerMask envLayer;

	public static readonly CoverParams Default = new() {
		range = 10,
		navRange = 15,
		minDot = 0.5f,
		bodyWidth = 0.3f,
		bodyHeight = 1.8f,
		doDebugs = true,
		maxPoints = 20,
		maxSplats = 30,
		navBatch = 1,
		splatBatch = 20,
		threatBreadth = 3,
		skepticism = 10,
		envLayer = ~0,
	};
}

public class CoverQuery {
	private CoverParams cfg;
	private Vector3 scale = Vector3.one * 0.1f;
	private Vector3 myPos, threat, threatTorso;
	private NavMeshPath path;

	private List<CoverPoint> navQueue = new();
	private List<CoverTask> coverTasks = new();
	private List<CoverTask> finished = new();

	public CoverQuery(Vector3 myPos, Vector3 threat, float threatHeight, CoverParams cfg) {
		this.myPos = myPos;
		this.threat = threat;
		this.threatTorso = threat + threatHeight * Vector3.up;
		this.cfg = cfg;
		path = new();

		// 1. Range & angle
		foreach (CoverPoint c in CoverGenerator.I.cover) {
			wasd.Add(new(c.position + (cfg.bodyHeight / 2) * Vector3.up, threatTorso));
			if ((c.position - myPos).sqrMagnitude < cfg.range * cfg.range && // In range?
					Mathf.Abs(Vector3.Dot((threat - c.position).normalized.FlattenY(), c.normal.FlattenY())) > cfg.minDot && // Angle ok?
					Physics.Linecast(c.position + (cfg.bodyHeight / 2) * Vector3.up, threatTorso, out var _, cfg.envLayer)) { // Breaks LOS to torso?
				navQueue.Add(c); 
			}
		}
	}

	List<(Vector3 a, Vector3 b)> wasd = new();
	public void FindCover() {
		foreach(var v in wasd) { Debug.DrawLine(v.a, v.b, Physics.Linecast(v.a, v.b, out var _, cfg.envLayer) ? Color.red : Color.green); }

		long t = System.Diagnostics.Stopwatch.GetTimestamp();

		// 3. Navigation distance
		long t2 = Stopwatch.GetTimestamp();
		int navBatch = Mathf.Min(navQueue.Count, cfg.navBatch);
		if (navBatch > 0) {
			for (int i = 0; i < navBatch; i++) {
				CoverPoint c = navQueue.PopRandom();
				NavMesh.CalculatePath(myPos, c.position, NavMesh.AllAreas, path);
				if (cfg.doDebugs && path.status == NavMeshPathStatus.PathComplete ) { Ext.DrawPath(path.corners); }
				float dist = path.corners.GetPathLength();
				if (path.status == NavMeshPathStatus.PathComplete && dist <= cfg.navRange) { 
					coverTasks.Add(new(c, dist, cfg));
				}
			}
		}
		
		GraphWindow.AddToGraph("Nav", (float)Ext.LogTime(t2, message: false));

		// 4. Splatting
		long t3 = Stopwatch.GetTimestamp();
		int splatsDone = 0;
		for (int i = coverTasks.Count; i --> 0;) {
			if (splatsDone >= cfg.splatBatch) { break; }

			CoverTask task = coverTasks[i];
			while (splatsDone < cfg.splatBatch) {
				if (task.runs < cfg.maxSplats) {
					task.RunSplat(threat, cfg.bodyWidth, cfg.bodyHeight);
					splatsDone++;
				}
				else {
					finished.Add(task);
					coverTasks.RemoveAt(i);
					break;
				}
			}
		}
		GraphWindow.AddToGraph("Splatting", (float)Ext.LogTime(t3, message: false));

		GraphWindow.AddToGraph("Total FindCover", (float)Ext.LogTime(t, message: false));

		if (cfg.doDebugs) {
			Debug.Log($"NAV: {navQueue.Count}, tasks: {coverTasks.Count}, finished: {finished.Count}");
			List<CoverTask> pool = new();
			pool.AddRange(coverTasks);
			pool.AddRange(finished);
			foreach (CoverTask task in pool) { 
				task.Debug();
				if (task == pool[GameManager.I.var1]) { task.FullDebug(); }
			}
		}
	}


	public bool GetBestPoint(float urgency, float aggression, out Vector3 point) {
		if (finished.Count == 0 || coverTasks.Count > 0 || navQueue.Count > 0 ) {
			point = default;
			return false;
		}

		float Quality(CoverTask task) {
			float distScore = 1 - Mathf.InverseLerp(0, cfg.navRange, task.distance);
			return task.Safety + task.Offense * aggression + distScore * urgency; 
		}

		CoverTask best = null;
		float maxQuality = float.NegativeInfinity;
		foreach (CoverTask task in finished) {
			float currentQuality = Quality(task);
			if (currentQuality > maxQuality) {
				maxQuality = currentQuality;
				best = task;
			}
		}

		point = best.cover.position;
		return true;
	}
}

