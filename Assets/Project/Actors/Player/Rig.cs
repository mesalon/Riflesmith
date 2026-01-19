using UnityEngine;

public class Rig {
	public Transform head;
	public Hand LHand, RHand;
	[SerializeField] CharacterController cc;
	[SerializeField] Transform gear;
	private readonly Player ctx;

	public Rig(Player ctx) {
		this.ctx = ctx;
	}

	public void Tick() {
		gear.localPosition = new(head.localPosition.x, head.localPosition.y - 0.5f, head.localPosition.z);
		gear.localRotation = Quaternion.Euler(new(0, head.localRotation.eulerAngles.y, 0));

		//LHand.transform.SetPose(new(ctx.controls.LHand.Pos.ReadValue<Vector3>(), ctx.controls.LHand.Rot.ReadValue<Quaternion>()), true);
		//RHand.transform.SetPose(new(ctx.controls.RHand.Pos.ReadValue<Vector3>(), ctx.controls.RHand.Rot.ReadValue<Quaternion>()), true);

		// Recenter
		if (Input.GetKeyDown(KeyCode.Space)) {
			Vector3 adjustment = new Vector3(head.position.x, 0, head.position.z) -
				new Vector3(ctx.transform.position.x, 0, ctx.transform.position.z);
			ctx.transform.position += adjustment;
			head.localPosition -= ctx.transform.InverseTransformVector(adjustment);
			LHand.transform.localPosition -= ctx.transform.InverseTransformVector(adjustment);
			RHand.transform.localPosition -= ctx.transform.InverseTransformVector(adjustment);
		}
	}

	void FixedUpdate() {
		cc.height = head.localPosition.y;
		cc.center = new(head.localPosition.x, cc.height / 2, head.localPosition.z);
	}

	[BeforeRenderOrder(-30000)]
	public void UpdateHead() {
		//head.localPosition = ctx.controls.Head.Pos.ReadValue<Vector3>();
		//head.localRotation = ctx.controls.Head.Rot.ReadValue<Quaternion>();
	}
}
