using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabInteractable : MonoBehaviour, IInteractable {
	public Hand Interactor { get; set; }
	public bool PreventInteraction { get; set; }
	[SerializeField] Transform pose;
	private Rigidbody rb;
	private Quaternion initRot, targetRot, actualRot;

	private void Awake() {
		rb = GetComponent<Rigidbody>(); 
	}

	public void OnPicked() {
		actualRot = initRot = rb.rotation;
		targetRot = Interactor.palm.rotation;
	}
	public void OnHold() { 
		ConfigurableJoint other = Interactor.other.grabJoint;
		if (other != null && other.transform.root == transform) {
			Quaternion look = Quaternion.LookRotation(Interactor.other.transform.position - Interactor.transform.position);
		}
	}
	public void OnHoldFixed() {
		ConfigurableJoint joint = Interactor.grabJoint;
		joint.targetPosition = Vector3.Lerp(joint.targetPosition, Vector3.zero, Time.fixedDeltaTime * Interactor.grabPosSpeed);
		actualRot = Quaternion.Slerp(actualRot, targetRot, Interactor.grabRotSpeed * Time.fixedDeltaTime);
		joint.SetTargetRotationLocal(actualRot, initRot);
		
	}
	public void OnDropped() { }
}
//        if (Interactor.other.held is GrabInteractable s && s.transform.root == transform.root && priority > s.priority) {
//            Vector3 gripOffset = s.grabPoint.position - grabPoint.position;
//            Vector3 handsDir = s.Hand.position - Hand.position;
//            Vector3 adjustedDir = handsDir - gripOffset;
//            targetRotation = Quaternion.LookRotation(adjustedDir, Vector3.Cross(adjustedDir, Hand.right));
//       }
