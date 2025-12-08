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

		var top = new (float score, AIAction action)[3];
		for(int i=0; i<3; i++) top[i] = (float.MinValue, null);
		foreach (AIAction a in actions) {
			float v = a.GetScore();
			if (v > top[0].score) {
				top[2] = top[1]; 
				top[1] = top[0]; 
				top[0] = (v, a);
			}
			else if (v > top[1].score) {
				top[2] = top[1]; 
				top[1] = (v, a);
			}
			else if (v > top[2].score) {
				top[2] = (v, a);
			}
		}
		Ext.Label(ctx.transform.position + 2 * Vector3.up, $"{top[0].action} ({top[0].score})\n{top[1].action} ({top[1].score})\n{top[2].action} ({top[2].score})");
		top[0].action.Execute();
	}
}
