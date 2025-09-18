public class GasBlock : Attachment {
	public override void OnAttach(FirearmReceiver f = null) {
		f.stats.gasBlowback = true;
		print("Blow me");
	}
}