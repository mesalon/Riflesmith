using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class BotBrain {
	private Nubot ctx;
	[SerializeField] List<BotAction> actions = new();
	private BotAction current;

	public void Init(Nubot ctx) {
		this.ctx = ctx;
		actions.Add(new GetUpAction(ctx));
	}

	public void Tick() {
		float highest = 0;
		BotAction action = null;
		foreach (BotAction candidate in actions) {
			float score = candidate.Score();
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
}
