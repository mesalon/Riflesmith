using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UtilityAI { 
	public List<AIAction> actions;
	private AIAction current;
	private Enemy ctx;

	public UtilityAI(Enemy ctx) {
		this.ctx = ctx;
		actions = new() {
			new PatrolAction(ctx),
			new ShootAction(ctx),
			new SearchAction(ctx),
			new MoveUpAction(ctx),
		};
	}

	public void Tick() {
		var msg = string.Join("\n", actions.OrderByDescending(a => a.GetScore()).Take(3).Select((a, i) => $"{i+1}: {a} ({a.GetScore()})"));
		Ext.Label(ctx.transform.position + Vector3.up * 2, msg);

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
			if (current != null) current.Exit();
			current = action;
			current.Enter();
		}
		current.Tick();
	}
}
