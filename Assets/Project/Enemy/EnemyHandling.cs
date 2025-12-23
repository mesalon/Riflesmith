using UnityEngine;

public class EnemyHandling {
	private LocomotionCfg cfg => ctx.blackboard.cfg.locomotion;
	private EnemyFirearm gun => ctx.blackboard.weapon;
	private Blackboard board => ctx.blackboard;
	private readonly Enemy ctx;
	private bool isAiming;
	private Vector3 aimDir;
	private float aimMag;
	private Vector3 aimTarget;

	public EnemyHandling(Enemy ctx) {
		this.ctx = ctx;
	}

	public void FireAt(Vector3 target) {
		isAiming = true;
		aimTarget = target;
		ctx.locomotion.Focus(aimTarget);
		Vector3 targetDir = (aimTarget - board.eyes.position).normalized;
		if (Vector3.Angle(aimDir, targetDir) < 5) {
			if (t > rest) {
				board.weapon.FireOnce();
				t -= 0.25f + 0.15f * (0.5f - Random.value);
				burst++;
				if (burst == 3) { 
					t = burst = 0; 
					rest = 0.75f + (0.5f - Random.value) * 0.75f;
				}
			}
		}
		t += Time.deltaTime;
	}

	float t;
	int burst;
	float rest;
	public void Tick() {
		board.gunRestRig.weight = Mathf.Lerp(board.gunRestRig.weight, isAiming ? 0 : 1, Time.deltaTime * cfg.aimSpeed);
		aimMag = (aimTarget - board.eyes.position).magnitude;
		Vector3 targetDir = (aimTarget - board.eyes.position).normalized;
		aimDir = Vector3.RotateTowards(aimDir, targetDir, cfg.lookSpeed * Time.deltaTime, float.MaxValue);
		if (isAiming) board.ikTarget.position = board.eyes.position + aimDir * Mathf.Max(aimMag, 5);
	}

	public void ADS(bool state) {
		isAiming = state;
	}
}
