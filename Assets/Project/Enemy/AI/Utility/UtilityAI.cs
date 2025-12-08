using System.Collections.Generic;
using UnityEngine;

public class UtilityAI { 
	private List<AIAction> actions;
	private AIAction current;
	private Enemy ctx;

	public UtilityAI(Enemy ctx) {
		this.ctx = ctx;
		actions = new() {
			new PatrolAction(ctx),
			new ShootAction(ctx),
		};
	}

	public void Tick() {
		actions.Sort((a, b) => b.GetScore().CompareTo(a.GetScore()));

		float highest = 0;
		AIAction action = null;
		foreach (AIAction a in actions) {
			float score = a.GetScore();
			if (score > highest) {
				highest = score;
				action = a;
			}
		}
		if (action != current) {
			current = action;
			current.Enter();
		}
		Ext.Label(ctx.transform.position + 2 * Vector3.up, $"Highest: {current} ({current.GetScore()})");
	}
}
