using Debug = UnityEngine.Debug;
using System.Diagnostics;
using UnityEngine;
using Pathfinding;
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
	private Seeker seeker;

	private List<CoverPoint> navQueue = new();
	private List<CoverTask> coverTasks = new();
	private List<CoverTask> finished = new();

	public CoverQuery(Vector3 myPos, Vector3 threat, Seeker seeker, CoverParams cfg) {
		this.myPos = myPos;
		this.threat = threat;
		this.seeker = seeker;
		this.cfg = cfg;
		threatTorso = threat + 1.85f * Vector3.up;

		// 1. Range & angle
		foreach (CoverPoint c in CoverGenerator.I.cover) {
			float midpoint = cfg.bodyHeight / 2;
			if ((c.position - myPos).sqrMagnitude < cfg.range * cfg.range && // In range?
					Mathf.Abs(Vector3.Dot((threat - c.position).normalized.FlattenY(), c.normal.FlattenY())) > cfg.minDot && // Angle ok?
					Physics.Linecast(c.position + midpoint * Vector3.up, threatTorso, out var _, cfg.envLayer)) { // Breaks LOS to torso?
				navQueue.Add(c); 
			}
		}
	}

	public void FindCover() {
		long t = Stopwatch.GetTimestamp();

		// 3. Navigation distance TODO: FUCKING REPLACE THIS WITH ACTUAL NAV DISTANCE
		long t2 = Stopwatch.GetTimestamp();
		for (int i = navQueue.Count - 1; i >= 0; i--) {
			float dist = Vector3.Distance(myPos, navQueue[i].position);
			if (dist <= cfg.navRange) { coverTasks.Add(new(navQueue[i], dist, cfg)); }
			navQueue.RemoveAt(i);
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
			foreach (CoverTask task in pool) { task.Debug(); }
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

