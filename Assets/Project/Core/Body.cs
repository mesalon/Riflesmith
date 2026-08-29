using UnityEngine;

public enum BodyForceMode { Force = 0, Impulse = 1 }
public class Body { // Grave sin against polymorphism. The people responsible (Unity team) are to be hung at a trial for crimes against humanity.
	public Transform transform => ab ? ab.transform : rb.transform;
	public float mass => ab ? ab.mass : rb.mass;
	public GameObject gameObject => ab ? ab.gameObject : rb.gameObject;
	public Vector3 GetPointVelocity(Vector3 worldPoint) => ab ? ab.GetPointVelocity(worldPoint) : rb.GetPointVelocity(worldPoint);
	public void AddForceAtPosition(Vector3 force, Vector3 pos, BodyForceMode mode) { 
		if (ab) ab.AddForceAtPosition(force, pos, (ForceMode)mode);
		else rb.AddForceAtPosition(force, pos, (ForceMode)mode);
	}
	public void AddForceAtPosition(Vector3 force, Vector3 pos) => AddForceAtPosition(force, pos, BodyForceMode.Force);
	public void AddRelativeTorque(Vector3 torque) {
		if (ab) ab.AddRelativeTorque(torque);
		else rb.AddRelativeTorque(torque);
	}
	private bool IsNull => !ab && !rb;

	public ArticulationBody ab;
	public Rigidbody rb;

	public static bool operator ==(Body lhs, Body rhs) { 
		bool lhsNull = lhs is null;
		bool rhsNull = rhs is null;
		if (lhsNull && rhsNull) return true;
		if (rhsNull) return lhs.IsNull;
		if (lhsNull) return rhs.IsNull;
		return ReferenceEquals(lhs, rhs);
	}
	public static bool operator !=(Body lhs, Body rhs) => !(lhs == rhs);
	public static implicit operator bool(Body body) => body != null;
	public static implicit operator Body(Rigidbody body) => new() { rb = body };
	public static implicit operator Body(ArticulationBody body) => new() { ab = body };
}

public static class BodyExtensions {
	public static Body GetBody(this RaycastHit hit) {
		if (hit.rigidbody) return hit.rigidbody;
		if (hit.articulationBody) return hit.articulationBody;
		return null;
	}
	public static Body GetBody(this Collider col) {
		if (col.attachedRigidbody) return col.attachedRigidbody;
		if (col.attachedArticulationBody) return col.attachedArticulationBody;
		return null;
	}
	public static void SetBody(this ConfigurableJoint joint, Body body) {
		if (body.ab) joint.connectedArticulationBody = body.ab;
		else joint.connectedBody = body.rb;
	}

	public static Body GetBody(this Component comp) {
		if (comp.TryGetComponent(out Rigidbody rb)) return rb;
		else if (comp.TryGetComponent(out ArticulationBody ab)) return ab;
		else return null;
	}
}
