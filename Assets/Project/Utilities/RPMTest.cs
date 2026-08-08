using UnityEngine;

public class RPMTest : MonoBehaviour {
	private float Timestamp => Time.time - startTime;
	private float Interval => 1 / rpm * 60;
	[SerializeField] float rpm;
	[SerializeField] int toFire;
	private float startTime;
	private float count;
	private float t;

	void Update() {
		if (Input.GetKeyDown(KeyCode.F)) {
			startTime = Time.time;
			count = 0;
			print("Start");
		}

		if (startTime != 0 && count < toFire && Time.time >= t) {
			count++;
			t = Time.time + Interval;
			print($"t: {t:f3}");
			print($"Fire {count} at {Timestamp:f3}, {Timestamp - ((count - 1) * Interval):+0.000;-0.000;0.000}");
		}

		if (count == toFire && startTime != 0) {
			print($"Done, took {Timestamp:f2} to fire {toFire} rounds. True RPM: {(toFire) / Timestamp * 60}");
			startTime = 0;
		}
	}
}
