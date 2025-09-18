using UnityEngine;

public class AutoSear : Attachment {
	public override void OnAttach(FirearmReceiver f = null) {
		f.stats.isFullAuto = true;
	}
}
