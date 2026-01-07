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

	private void Awake() {
		if (I == null) {
			I = this;
			DontDestroyOnLoad(gameObject);
		}
		else { Destroy(gameObject); }
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
}
