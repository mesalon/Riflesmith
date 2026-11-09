using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class BotBrain {
	private Nubot ctx;
	[SerializeField] List<BotAction> actions = new();
	[SerializeField] float orderBoost;
	private BotAction current;
	private Type order;

	public void Init(Nubot ctx) {
		this.ctx = ctx;
		actions.Add(new GetUpAction(ctx));
	}

	public void Tick() {
		float highest = 0;
		BotAction action = null;
		foreach (BotAction candidate in actions) {
			float score = candidate.Score();
			if (candidate.GetType() == order) { score += orderBoost; }
			if (score > highest) {
				highest = score;
				action = candidate;
			}
		}
		if (action != current) {
			current?.Exit();
			current = action;
			current?.Enter();
		}
		MyGizmos.Label(ctx.Center + Vector3.up, $"Action: {current}");
		current?.Tick();
	}

	public void SendOrder<T>() where T : BotAction {
		order = typeof(T);
	}
}
