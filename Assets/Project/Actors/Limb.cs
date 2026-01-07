using UnityEngine;

public class Limb : MonoBehaviour, IDamageable {
    public Actor self;
    public float damageMultiplier = 1;

		public void Damage(float amount) { self.Damage(amount); }
}
