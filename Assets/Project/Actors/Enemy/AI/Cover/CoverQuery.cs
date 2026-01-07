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
	public float aggression;
	public float urgency;
	public Vector3? posBias;
	public LayerMask envLayer;

	public static readonly CoverParams Default = new() {
		range = 10,
		navRange = 15,
		minDot = 0.5f,
		bodyWidth = 0.3f,
		bodyHeight = 1.8f,
		maxPoints = 20,
		maxSplats = 20,
		navBatch = 1,
		splatBatch = 40,
		threatBreadth = 3,
		skepticism = 10,
		aggression = 1,
		urgency = 1,
		envLayer = ~0,
	};
}

public class CoverQuery {
	private CoverParams cfg;
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

	public void Search() {
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
			while (task.runs < cfg.maxSplats && splatsDone < cfg.splatBatch) {
				task.RunSplat(threat, cfg.bodyWidth, cfg.bodyHeight);
				splatsDone++;
			}
			if (task.runs >= cfg.maxSplats) {
				finished.Add(task);
				coverTasks.RemoveAt(i);
			}
		}
		GraphWindow.AddToGraph("Splatting", (float)Ext.LogTime(t3, message: false));

		GraphWindow.AddToGraph("Total FindCover", (float)Ext.LogTime(t, message: false));
	}

	public bool TryGetCover(out CoverTask cover) {
		if (finished.Count == 0 || coverTasks.Count > 0 || navQueue.Count > 0) {
			cover = default;
			return false;
		}

		CoverTask best = null;
		float maxScore = float.NegativeInfinity;
		foreach (CoverTask task in finished) {
			float bias = 0;
			if (cfg.posBias != null) { bias = 1 - Mathf.InverseLerp(0, cfg.navRange * cfg.navRange, (cfg.posBias.Value - task.point.position).sqrMagnitude); }
			float distScore = 1 - Mathf.InverseLerp(0, cfg.navRange, task.distance);
			float score = task.Safety + task.Offense * cfg.aggression + distScore * cfg.urgency + bias;
			if (score > maxScore) {
				maxScore = score;
				best = task;
			}
		}

		cover = best;
		return true;
	}

	public void ShowDebug(bool full) {
		List<CoverTask> pool = new();
		pool.AddRange(coverTasks);
		pool.AddRange(finished);
		foreach (CoverTask task in pool) { 
			task.Debug(); 
			if (full) {
				float bias = 0;
				if (cfg.posBias != null) { bias = 1 - Mathf.InverseLerp(0, cfg.navRange * cfg.navRange, (cfg.posBias.Value - task.point.position).sqrMagnitude); }
				float distScore = 1 - Mathf.InverseLerp(0, cfg.navRange, task.distance);
				float score = task.Safety + task.Offense * cfg.aggression + distScore * cfg.urgency + bias;
				Ext.Label(task.point.position, @$"
						Safety: {task.Safety:F2}
						Offense: {task.Offense * cfg.aggression:f2}
						Distance: {distScore * cfg.urgency:f2}
						PosBias: {bias:f2}
						Score: {score:f2}");
			}
		}
	}
}

