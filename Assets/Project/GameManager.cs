using UnityEditor;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static Camera Camera => Camera.allCameras
		.Where(c => c.targetTexture == null)
		.OrderByDescending(c => c.depth)
		.FirstOrDefault();
	public static GameManager I { get; private set; }
	private Enemy enemyPF;

	private void Awake() {
		if (I == null) {
			I = this;
			DontDestroyOnLoad(gameObject);
		}
		else { Destroy(gameObject); }
	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.K)) {
			foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None)) { enemy.body.Damage(100, 0); }
		}
	}

	private void LateUpdate() {
		float dt = Time.deltaTime;
		for (int i = Ext.labelQueue.Count - 1; i >= 0; i--) { Ext.labelQueue[i].lifespan -= dt; }
		Ext.labelQueue.RemoveAll(r => r.lifespan <= 0);
		for (int i = Ext.drawQueue.Count - 1; i >= 0; i--) { Ext.drawQueue[i].lifespan -= dt; }
		Ext.drawQueue.RemoveAll(r => r.lifespan <= 0);
	}

	private void OnDrawGizmos() {
		if (!Application.isPlaying) {
			Ext.labelQueue.Clear();
			Ext.drawQueue.Clear();
		}
		foreach (var label in Ext.labelQueue) { Handles.Label(label.position, label.text, label.style); }
		foreach (var draw in Ext.drawQueue) { draw.action(); }
	}

	public static Enemy SpawnEnemy(Vector3 position) {
		bool badSpot = false;
		Collider[] overlap = Physics.OverlapCapsule(position, position + Vector3.up * 1.8f, 0.5f);
		foreach (Collider col in overlap) {
			if (col.TryGetComponent(out Limb _)) {
				badSpot = true;
				break;
			}
		}
		return Instantiate(I.enemyPF, badSpot ? position + Vector3.up * 1.8f: position, Quaternion.Euler(0, Random.Range(0, 360), 0));
	}
}
