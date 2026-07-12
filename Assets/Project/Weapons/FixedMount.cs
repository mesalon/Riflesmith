using UnityEngine;

public class FixedMount : Mount {
	public Part attached;
	public override Vector3 GetAttachPoint(Vector3 center) => transform.position;
}
