using UnityEngine;

public class Human : MonoBehaviour {
	public Vector3 Center {
		get {
			Vector3 all = Vector3.zero;
			foreach (Transform p in positionPoints) { all += p.position; }
			return all / positionPoints.Length;
		}
	}
	public float health = 100;
	public int IFF;
	[SerializeField] Transform[] positionPoints;

	public void Damage(float amount) {
		health = Mathf.Max(0, health - amount);
	}
}
