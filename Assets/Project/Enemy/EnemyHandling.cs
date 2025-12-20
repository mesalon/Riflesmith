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
		if (!isAiming) { ADS(true); }
		aimTarget = target;
	}

	float t;
	int burst;
	public void Tick() {
		board.gunRestRig.weight = Mathf.Lerp(board.gunRestRig.weight, isAiming ? 0 : 1, Time.deltaTime * cfg.aimSpeed);
		aimMag = (aimTarget - board.eyes.position).magnitude;
		Vector3 targetDir = (aimTarget - board.eyes.position).normalized;
		aimDir = Vector3.RotateTowards(aimDir, targetDir, cfg.lookSpeed * Time.deltaTime, float.MaxValue);
		board.ikTarget.position = board.eyes.position + aimDir * aimMag;

		ctx.locomotion.Focus(aimTarget);
		if (Vector3.Angle(aimDir, targetDir) < 5) {
			if (t > 1.5f) {
				board.weapon.FireOnce();
				t -= 0.25f;
				burst++;
				if (burst == 3) { t = burst = 0; }
			}
		}
		t += Time.deltaTime;
	}

	public void ADS(bool state) {
		isAiming = state;
	}
}
