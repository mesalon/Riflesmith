using UnityEngine;
using System.Collections.Generic;

public class CoverTask {
	public float Safety => (float)safetyHits / runs;
	public float Offense => (float)Mathf.Max(leftAttackHits, rightAttackHits) / runs;
	public Vector3 ReturnFirePoint {
		get {
			Vector3 point = Vector3.zero;
			bool useLeft = leftAttackHits > rightAttackHits;
			List<(Vector3, bool)> side = useLeft ? returnFireLeft : returnFireRight;
			int hits = useLeft ? leftAttackHits : rightAttackHits;
			if (side.Count > 0) {
				foreach ((Vector3 pos, bool LOS) splat in side) {
					if (splat.LOS) point += splat.pos; 
				}
				point /= hits;
			} else {
				point = this.point.position;
			}
			return point;
		}
	}
	public CoverPoint point;
	public int runs;
	public int safetyHits, leftAttackHits, rightAttackHits;
	public float distance;
	private CoverParams cfg;
	private List<(Vector3, bool)> safetySplats = new();
	private List<(Vector3, bool)> returnFireLeft = new();
	private List<(Vector3, bool)> returnFireRight = new();

	public CoverTask(CoverPoint point, float distance, CoverParams cfg) {
		this.point = point;
		this.distance = distance;
		this.cfg = cfg;
	}

	// CONSIDER THE "LEVERAGE" OF THE COVER POINT. LIKE, HOW MUCH WILL YOU EXPOSE YOURSELF IF YOU LEAN OUT
	// OKAY LIKE WHEN THE PLAYER WOULD LEAN OUT, CHECK IF YOU'D BE EXPOSED. THAT'S BAD
	// CONVERSELY, CHECK IF YOU LEAN OUT, CHECK IF HE WOULD BE EXPOSED
	// RIGHT NOW IT'S ONLY CONSIDERING A FLAT OFFSET TO THE SIDE. NOT TAKING INTO CONSIDERATION THE RELATIONSHIP OF THESE TWO DIFFERENT SITUATIONS
	public void RunSplat(Vector3 threat, float bodyWidth, float bodyHeight) {
		Quaternion rot = Quaternion.LookRotation((threat - point.position).FlattenY(), Vector3.up);
		Vector3 rand = new(Random.Range(-bodyWidth, bodyWidth), Random.Range(0, bodyHeight), 0);
		Vector3 myPosSplat = point.position + rot * rand;
		Vector3 threatSplat = threat + rot * rand;

		bool safetyCheck = Physics.Linecast(threatSplat, myPosSplat, out var _, cfg.envLayer);
		if (safetyCheck) { safetyHits++; }
		safetySplats.Add(new(myPosSplat, safetyCheck));

		bool checkLeft = false;
		for (int i = 0; i < 2; i++) {
			float width = checkLeft ? -bodyWidth : bodyWidth;
			Vector3 returnFireSplat = point.position + rot * new Vector3(Random.Range(width, width * 2), Random.Range(0, bodyHeight), 0);
			bool linecastHit = Physics.Linecast(threatSplat, returnFireSplat, out var _, cfg.envLayer);
			bool returnFireCheck = !linecastHit;
			if (returnFireCheck) { 
				if (checkLeft) leftAttackHits++;
				else rightAttackHits++;
			}
			if (checkLeft) returnFireLeft.Add(new(returnFireSplat, returnFireCheck));
			else returnFireRight.Add(new(returnFireSplat, returnFireCheck));
			checkLeft = true;
		}

		runs++;
	}

	public void Debug() {
		foreach (var splat in safetySplats) { Ext.DrawCube(splat.Item1, Quaternion.identity, Vector3.one * 0.05f, splat.Item2 ? Color.green : Color.black); }
		foreach (var splat in returnFireLeft) { Ext.DrawCube(splat.Item1, Quaternion.identity, Vector3.one * 0.05f, splat.Item2 ? Color.greenYellow : Color.grey); }
		foreach (var splat in returnFireRight) { Ext.DrawCube(splat.Item1, Quaternion.identity, Vector3.one * 0.05f, splat.Item2 ? Color.greenYellow : Color.grey); }
		Ext.DrawCube(ReturnFirePoint, Quaternion.identity, Vector3.one * 0.1f, Color.blue);
	}
}
