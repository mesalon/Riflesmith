using UnityEngine;

public class Barrel : Attachment {
	[SerializeField] float length;
	public override void OnAttach(FirearmReceiver f = null) {
		f.stats.barrelLength = length;
	}
}
