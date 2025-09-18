using UnityEngine;

public class ShootPlayerTask : Node {
	private EnemyAI _ai;
	private float fireTimer;
	
	public ShootPlayerTask(EnemyAI ctx) {
		_ai = ctx;
	}
}