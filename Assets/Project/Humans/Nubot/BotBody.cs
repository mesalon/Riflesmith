using RootMotion.Dynamics;
using UnityEngine;

[System.Serializable] public class BotBody {
	public float Health { get; private set; }
	public float strength;
	public float getUpStrength = 25;
	public PuppetMaster pm;
	[SerializeField] float maxHealth = 100;
	[SerializeField] float maxStrength = 50;
	[SerializeField] float regeneration = 5;
	private Nubot ctx;

	public void Init(Nubot ctx) {
		this.ctx = ctx;
		Health = maxHealth;
		strength = maxStrength;
	}

	public void Damage(float amount) {
		Health = Mathf.Max(0, Health - amount);
		strength = Mathf.Max(0, strength - amount * strength);
		if (strength <= 0) {
			Debug.Log($"Fell down due to strength");
			pm.Kill();
			pm.muscleWeight = 0.2f;
		}
		if (Health <= 0) {
			Debug.Log($"Died due to health");
			pm.Kill();
			pm.muscleWeight = 0;
		}
	}

	public void Tick() {
		strength = Mathf.Min(strength + regeneration * Time.deltaTime, maxStrength);
	}
}
