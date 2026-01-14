using UnityEngine;
using System;

public class RecoilTest : MonoBehaviour {
	[SerializeField] Vector3 impulse;
	[SerializeField] Transform pivot, muzzle, stock, feet;
	[SerializeField] float weapMass;
	[SerializeField] Vector3 bodyMassTensor;
	private Vector3 startPos;
	private Quaternion startRot;
	[SerializeField] RecoilSystem rs;
	[SerializeField] float delay;
	[SerializeField] bool auto;
	private float lastShot;

	private void Start() {
		startPos = transform.position;
		startRot = transform.rotation;
		var box = GetComponent<BoxCollider>();
		Vector3 weapMassTensor = new Bounds(box.center, box.size).GetInertiaTensor(pivot.localPosition, weapMass);
		rs.weapon.massTensor = weapMassTensor;
		rs.weapon.source = muzzle.localPosition;
		rs.weapon.pivot = pivot.localPosition;
	}

	private void Update() {
		rs.body.massTensor = bodyMassTensor;
		rs.body.source = stock.localPosition;
		rs.body.pivot = feet.localPosition;
		if (auto) {
			if (Input.GetMouseButton(0) && lastShot > delay) {
				rs.Impel(impulse); 
				lastShot = 0;
			}
		}
		else if (Input.GetMouseButtonDown(0)) { rs.Impel(impulse); }
		rs.Tick();

		transform.position = startPos + transform.TransformDirection((Vector3)rs.weapon.pos + (Vector3)rs.body.pos);
		transform.rotation = startRot * (Quaternion)rs.weapon.rot * (Quaternion)rs.body.rot;
		lastShot += Time.deltaTime;
	}
}

[Serializable] public class RecoilSystem {
	public RecoilBody weapon, body;

	public void Tick() {
		weapon.Tick();
		body.Tick();
	}

	public void Impel(Vector3 impulse) {
		weapon.Impel(impulse);
		body.Impel(impulse);
	}
}

[Serializable] public class RecoilBody {
	public Spring3 pos, rot;
	public Vector3 massTensor, source, pivot; 
	public float mass;

	public void Tick() {
		pos.Tick(mass);
		rot.Tick(massTensor);
	}

	public void Impel(Vector3 impulse) {
		impulse /= -mass;
		pos.Impel(impulse);

		Vector3 lever = source - pivot;
		Vector3 torque = Vector3.Cross(lever, impulse);
		torque.x /= massTensor.x;
		torque.y /= massTensor.y;
		torque.z /= massTensor.z;
		rot.Impel(torque);
	}
}

[Serializable] public class Spring3 {
	public SpringSystem x = new(), y = new(), z = new();
	[SerializeField] float stiffness, dampingRatio;

	public static implicit operator Vector3(Spring3 spring) => new(spring.x.position, spring.y.position, spring.z.position);
	public static implicit operator Quaternion(Spring3 spring) => Quaternion.Euler((Vector3)spring * Mathf.Rad2Deg);

	public void Impel(Vector3 impulse) {
		x.velocity += impulse.x;
		y.velocity += impulse.y;
		z.velocity += impulse.z;
	}

	public void Tick(float massTensor) => Tick(new Vector3(massTensor, massTensor, massTensor));
	public void Tick(Vector3 massTensor) {
		x.Tick(stiffness, dampingRatio, massTensor.x);
		y.Tick(stiffness, dampingRatio, massTensor.y);
		z.Tick(stiffness, dampingRatio, massTensor.z);
	}
}

public class SpringSystem {
	public float position;
	public float velocity;

	public void Tick(float stiffness, float dampingRatio, float mass) {
		if (mass <= 0) return;
		float omegaN = Mathf.Sqrt(stiffness / mass);
		float omegaZeta = omegaN * dampingRatio;
		float newPos, newVel;
		if (dampingRatio > 1f) {
			float omegaD = omegaN * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
			float z1 = -omegaZeta - omegaD;
			float z2 = -omegaZeta + omegaD;
			float e1 = Mathf.Exp(z1 * Time.deltaTime);
			float e2 = Mathf.Exp(z2 * Time.deltaTime);
			float c2 = (velocity - position * z1) / (2 * omegaD);
			float c1 = position - c2;
			newPos = c1 * e1 + c2 * e2;
			newVel = c1 * z1 * e1 + c2 * z2 * e2;
		}
		else if (Mathf.Abs(dampingRatio - 1f) < 0.0001f) {
			float expTerm = Mathf.Exp(-omegaN * Time.deltaTime);
			newPos = (position + (velocity + omegaN * position) * Time.deltaTime) * expTerm;
			newVel = (velocity - (velocity + omegaN * position) * omegaN * Time.deltaTime) * expTerm;
		}
		else {
			float omegaD = omegaN * Mathf.Sqrt(1f - dampingRatio * dampingRatio);
			float expTerm = Mathf.Exp(-omegaZeta * Time.deltaTime);
			float cosTerm = Mathf.Cos(omegaD * Time.deltaTime);
			float sinTerm = Mathf.Sin(omegaD * Time.deltaTime);
			float c1 = position;
			float c2 = (velocity + omegaZeta * position) / omegaD;
			newPos = expTerm * (c1 * cosTerm + c2 * sinTerm);
			newVel = -omegaZeta * newPos + expTerm * (c2 * omegaD * cosTerm - c1 * omegaD * sinTerm);
		}
		position = newPos;
		velocity = newVel;
	}
}

//float yaw = Mathf.Lerp(UnityEngine.Random.Range(Recoil.minRecoilX, Recoil.maxRecoilX), RecoilYaw, Recoil.stability);
//RecoilYaw = Mathf.Clamp(yaw, Recoil.minRecoilX, Recoil.maxRecoilX);
