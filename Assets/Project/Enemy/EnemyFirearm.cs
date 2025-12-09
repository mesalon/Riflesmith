using FMODUnity;
using UnityEngine;

public class EnemyFirearm : MonoBehaviour {
	public ReceiverStats.RecoilSettings recoil;
	public Transform muzzle;
	public int rounds;
	[HideInInspector] public bool triggerState;
	[SerializeField] private float cyclicRate;
	[SerializeField] private int capacity;
	[SerializeField] private EventReference shot;
	[SerializeField] ProjectileData projectile;
	private Vector3 kickPos, kickRot;
	private float recoilH, recoilV;
	private float fireTime;
	private bool releaseImmediately;

	private void Awake() {
		rounds = capacity;
	}

	private void Update() {
		if (triggerState && fireTime > 1 / (cyclicRate / 60) && rounds > 0) {
			if (releaseImmediately) {
				triggerState = false;
				releaseImmediately = false;
			}
			RuntimeManager.PlayOneShot(shot, muzzle.position);
			ProjectileManager.CreateProjectile(new(projectile, muzzle.position, muzzle.forward));
			recoilH = Mathf.Clamp(Mathf.Lerp(Random.Range(-recoil.rot.y, recoil.rot.y), recoilH, recoil.stability), -recoil.rot.y, recoil.rot.y);
			recoilV = Mathf.Clamp(Mathf.Lerp(Random.Range(-recoil.rot.x, recoil.rot.x), recoilV, recoil.stability), -recoil.rot.x, recoil.rot.x);
			kickPos += new Vector3(Random.Range(-recoil.pos.x, recoil.pos.x), recoil.pos.y, -recoil.pos.z) / 100;
			kickRot += new Vector3(recoilV, recoilH, 0);

			fireTime = 0;
			rounds--;
		}

		transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * recoil.posRecovery);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoil.rotRecovery);

		Vector3 appliedPos = recoil.posSpeed * Time.deltaTime * kickPos;
		Vector3 appliedRot = recoil.rotSpeed * Time.deltaTime * kickRot;
		transform.localPosition += appliedPos;
		transform.localRotation *= Quaternion.Euler(appliedRot);
		kickPos -= appliedPos;
		kickRot -= appliedRot;

		fireTime += Time.deltaTime;
	}
	public void FireOnce() {
		triggerState = true;
		releaseImmediately = true;
	}
}
