using UnityEngine;
using System.Collections.Generic;

public class CoverTask {
	public float Safety => safetyHits / (runs + cfg.skepticism);
	public float Offense => canAttackHits / (runs + cfg.skepticism);
	public CoverPoint cover;
	public int runs;
	public int safetyHits, canAttackHits;
	public float distance;
	private CoverParams cfg;
	private List<(Vector3, bool)> safetySplats = new();
	private List<(Vector3, bool)> returnFireSplats = new();
	private List<(Vector3 start, Vector3 end, bool hit)> linecasts = new();

	public CoverTask(CoverPoint cover, float distance, CoverParams cfg) {
		this.cover = cover;
		this.distance = distance;
		this.cfg = cfg;
	}

	// CONSIDER THE "LEVERAGE" OF THE COVER POINT. LIKE, HOW MUCH WILL YOU EXPOSE YOURSELF IF YOU LEAN OUT
	// OKAY LIKE WHEN THE PLAYER WOULD LEAN OUT, CHECK IF YOU'D BE EXPOSED. THAT'S BAD
	// CONVERSELY, CHECK IF YOU LEAN OUT, CHECK IF HE WOULD BE EXPOSED
	// RIGHT NOW IT'S ONLY CONSIDERING A FLAT OFFSET TO THE SIDE. NOT TAKING INTO CONSIDERATION THE RELATIONSHIP OF THESE TWO DIFFERENT SITUATIONS
	public void RunSplat(Vector3 threat, float bodyWidth, float bodyHeight) {
		Quaternion rot = Quaternion.LookRotation((threat - cover.position).FlattenY(), Vector3.up);
		Vector3 myPosSplat = (cover.position + rot * new Vector3(Random.Range(-bodyWidth, bodyWidth), UnityEngine.Random.Range(0, bodyHeight), 0));
		Vector3 threatSplat = (threat + rot * new Vector3(Random.Range(-bodyWidth *  cfg.threatBreadth, bodyWidth * cfg.threatBreadth), UnityEngine.Random.Range(0, bodyHeight), 0));

		bool safetyCheck = Physics.Linecast(threatSplat, myPosSplat, out var _, cfg.envLayer);
		linecasts.Add((threatSplat, myPosSplat, safetyCheck));
		if (safetyCheck) { safetyHits++; }
		safetySplats.Add(new(myPosSplat, safetyCheck));

		bool checkLeft = false;
		for (int i = 0; i < 2; i++) {
			float width = checkLeft ? -bodyWidth : bodyWidth;
			Vector3 returnFireSplat = (cover.position + rot * new Vector3(Random.Range(width, width * 2), UnityEngine.Random.Range(0, bodyHeight), 0));
			bool linecastHit = Physics.Linecast(threatSplat, returnFireSplat, out var _, cfg.envLayer);
			linecasts.Add((threatSplat, returnFireSplat, linecastHit));
			bool returnFireCheck = !linecastHit;
			if (returnFireCheck) { canAttackHits++; }
			returnFireSplats.Add(new(returnFireSplat, returnFireCheck));
			checkLeft = true;
		}

		runs++;
	}

	public void Debug() {
		foreach (var splat in safetySplats) { Ext.DrawCube(splat.Item1, Quaternion.identity, Vector3.one * 0.05f, Ext.GetColor(splat.Item2, Color.green, Color.black)); }
		foreach (var splat in returnFireSplats) { Ext.DrawCube(splat.Item1, Quaternion.identity, Vector3.one * 0.05f, Ext.GetColor(splat.Item2, Color.yellow, Color.black)); }
		Ext.Label(cover.position - 0.5f * Vector3.one, $"Safety: {Safety:F2}\nOffense: {Offense:f2}");
	}

	public void FullDebug() {
		Debug();
		foreach (var linecast in linecasts) {
			UnityEngine.Debug.DrawLine(linecast.start, linecast.end, linecast.hit ? Color.red : Color.cyan);
		}
	}
}
