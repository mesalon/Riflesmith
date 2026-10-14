using UnityEngine;

public class MoveToCoverAction : AIAction {
	private readonly Bot ctx;
	private AIBrain brain => ctx.brain;
	public MoveToCoverAction(Bot ctx) {
		this.ctx = ctx;
	}

	public override float GetScore() {
		return 0;
	}

	public override void Enter() {
	}

	public override void Tick() {
		if (brain.currentCover == null) {
			if (brain.cover.TryGetCover(out CoverTask cover)) {
			} else {
				brain.cover.Search();
			}
		}
	}

	public override void Exit() {
	}
}
