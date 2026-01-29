using UnityEngine;

public class Rig {
	public Transform head;
	public Hand LHand, RHand;
	private readonly Player ctx;

	public Rig(Player ctx) {
		this.ctx = ctx;
	}

	public void Tick() {
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
		ctx.cc.height = head.localPosition.y;
		ctx.cc.center = new(head.localPosition.x, ctx.cc.height / 2, head.localPosition.z);
	}

	[BeforeRenderOrder(-30000)]
	public void UpdateHead() {
		//head.localPosition = ctx.controls.Head.Pos.ReadValue<Vector3>();
		//head.localRotation = ctx.controls.Head.Rot.ReadValue<Quaternion>();
	}
}
