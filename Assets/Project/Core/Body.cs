using System.Runtime.CompilerServices;
using UnityEngine;

public enum BodyForceMode { Force = 0, Impulse = 1 }
public class Body { // Those responsible for this blasphemy against polymorphism should be hung at trial.
	public Transform transform => ab ? ab.transform : rb.transform;
	public float mass => ab ? ab.mass : rb.mass;
	public GameObject gameObject => ab ? ab.gameObject : rb.gameObject;
	public Vector3 position => ab ? ab.transform.position : rb.position;
	public Quaternion rotation => ab ? ab.transform.rotation : rb.rotation;
	public Vector3 angularVelocity => ab ? ab.angularVelocity : rb.angularVelocity;
	public Vector3 linearVelocity => ab ? ab.linearVelocity : rb.linearVelocity;
	public Vector3 inertiaTensor => ab ? ab.inertiaTensor : rb.inertiaTensor;
	public Quaternion inertiaTensorRotation => ab ? ab.inertiaTensorRotation : rb.inertiaTensorRotation;
	private bool IsNull => !ab && !rb;
	public ArticulationBody ab;
	public Rigidbody rb;

	public static bool operator ==(Body lhs, Body rhs) { 
		bool lhsNull = lhs is null;
		bool rhsNull = rhs is null;
		if (lhsNull && rhsNull) return true;
		if (rhsNull) return lhs.IsNull;
		if (lhsNull) return rhs.IsNull;
		return ReferenceEquals(lhs.ab, rhs.ab) || ReferenceEquals(lhs.rb, rhs.rb);
	}
	public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
	public override bool Equals(object obj) {
		if (obj is null) return IsNull;
		return obj is Body other && this == other;
	}
	public override string ToString() => ab ? ab.ToString() : rb.ToString();

	public static bool operator !=(Body lhs, Body rhs) => !(lhs == rhs);
	public static implicit operator bool(Body body) => body != null;
	public static implicit operator Body(Rigidbody body) => new() { rb = body };
	public static implicit operator Body(ArticulationBody body) => new() { ab = body };

	public Vector3 GetPointVelocity(Vector3 worldPoint) => ab ? ab.GetPointVelocity(worldPoint) : rb.GetPointVelocity(worldPoint);
	public void AddForceAtPosition(Vector3 force, Vector3 pos, BodyForceMode mode) { 
		if (ab) ab.AddForceAtPosition(force, pos, (ForceMode)mode);
		else rb.AddForceAtPosition(force, pos, (ForceMode)mode);
	}
	public void AddForceAtPosition(Vector3 force, Vector3 pos) => AddForceAtPosition(force, pos, BodyForceMode.Force);
	public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force) {
		if (ab) ab.AddTorque(torque, mode);
		else rb.AddTorque(torque, mode);
	}
	public void AddRelativeTorque(Vector3 torque) {
		if (ab) ab.AddRelativeTorque(torque);
		else rb.AddRelativeTorque(torque);
	}
	public Vector3 TransformPoint(Vector3 position) => ab ? ab.transform.TransformPoint(position) : rb.TransformPoint(position);
	public Vector3 TransformVector(Vector3 position) => ab ? ab.transform.TransformVector(position) : rb.TransformVector(position);
	public Vector3 TransformDirection(Vector3 position) => ab ? ab.transform.TransformDirection(position) : rb.TransformDirection(position);
	public Vector3 InverseTransformPoint(Vector3 position) => ab ? ab.transform.InverseTransformPoint(position) : rb.InverseTransformPoint(position);
	public Vector3 InverseTransformVector(Vector3 position) => ab ? ab.transform.InverseTransformVector(position) : rb.InverseTransformVector(position);
	public Vector3 InverseTransformDirection(Vector3 position) => ab ? ab.transform.InverseTransformDirection(position) : rb.InverseTransformDirection(position);
	public Quaternion TransformRotation(Quaternion rotation) => ab ? ab.transform.TransformRotation(rotation) : rb.TransformRotation(rotation);
	public Quaternion InverseTransformRotation(Quaternion rotation) => ab ? ab.transform.InverseTransformRotation(rotation) : rb.InverseTransformRotation(rotation);
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
