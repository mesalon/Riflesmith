using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIBrain { 
	public List<AIAction> actions;
	private AIAction current;
	private Bot ctx;

	[HideInInspector] public CoverQuery cover;
	[HideInInspector] public Actor target;
	[HideInInspector] public Vector3? aimFocus;
	[HideInInspector] public Vector3? targetLKP;
	[HideInInspector] public float confidence, alertness, suppression;
	[HideInInspector] public float LKPAge;
	[HideInInspector] public bool expectsToSeeTarget;
	public bool coverDebug, coverDebugFull;

	public AIBrain(Bot ctx) {
		this.ctx = ctx;
		actions = new() {
			new PatrolAction(ctx),
			new ShootAction(ctx),
			new SearchAction(ctx),
			new MoveUpAction(ctx),
		};
	}

	private void FixedUpdate() {
		if (target) {
			targetLKP = target.Center;
		} else {
			if (ctx.runVision && ctx.body.isUp && ctx.vision.Tick(out Actor target)) { 
				this.target = target; 
				LKPAge = 0;
			}
		}
		if (!ctx.vision.HasLOS(target)) {
			target = null;
			if (expectsToSeeTarget) LKPAge += Time.fixedDeltaTime;
		}
	}

	public void Tick() {
		if (coverDebug) { cover.ShowDebug(coverDebugFull); }

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
