using UnityEngine;

public abstract class Actor : MonoBehaviour {
	public float health = 100;
	public Vector3 Center => (LShoulder.position + RShoulder.position + LHip.position + RHip.position) / 4;
	public Locomotion locomotion;
	public GearManager gearManager;
	public int IFF;
	[SerializeField] Transform LShoulder, RShoulder, LHip, RHip;

	private void Awake() {
		locomotion = new(this);
		gearManager = new(this);
	}

	private void Update() {
		locomotion.Tick();
	}

	public abstract void Damage(float amount);
}
