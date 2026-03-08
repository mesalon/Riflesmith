using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIBrain { 
	public List<AIAction> actions;
	private AIAction current;
	private Bot ctx;

	public CoverQuery cover;
	public Human target;
	public Vector3? aimFocus;
	public Vector3? targetLKP;
	public float confidence, alertness, suppression;
	public float LKPAge;
	public bool isArmed;
	public bool expectsToSeeTarget;
	public bool coverDebug, coverDebugFull;

	public AIBrain(Bot ctx) {
		this.ctx = ctx;
		ctx.brain = this;
		actions = new() {
			new PatrolAction(ctx),
			new ShootAction(ctx),
			new SearchAction(ctx),
			new MoveUpAction(ctx),
		};
	}

	public void FixedTick() {
		if (target) {
			targetLKP = target.Center;
			if (!ctx.vision.HasLOS(target)) {
				target = null;
				if (expectsToSeeTarget) LKPAge += Time.fixedDeltaTime;
			}
		} else {
			if (ctx.runVision && ctx.body.isUp && ctx.vision.Tick(out Human target)) { 
				this.target = target; 
				LKPAge = 0;
			}
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
			current?.Exit();
			current = action;
			current.Enter();
		}
		current?.Tick();
	}
}
