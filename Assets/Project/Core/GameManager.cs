using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class GameManager : MonoBehaviour {
	public static Camera Camera => Camera.allCameras
		.Where(c => c.targetTexture == null)
		.OrderByDescending(c => c.depth)
		.FirstOrDefault();
	public static GameManager I { get; private set; }
	private Scene loading;
	private bool isLoading;

	void Awake() {
		if (I == null) {
			I = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Debug.LogError("You fucked up.");
		}
		SceneManager.sceneLoaded += (s, mode) => {
			foreach (Rigidbody rb in FindObjectsByType<Rigidbody>()) {
				break;
				rb.interpolation = RigidbodyInterpolation.Interpolate;
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
		};
	}

	void Start() {
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive).completed += op => {
			loading = SceneManager.GetSceneByName("Loading");
			loading.SetActive(false);
			if (SceneManager.GetActiveScene().name == "Day One") { TransitionScene("Main"); }
		};
	}

	void LateUpdate() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) { TransitionScene("Main"); }
		if (Input.GetKeyDown(KeyCode.Alpha2)) { TransitionScene("Facility"); }

		float dt = Time.deltaTime;
		Ext.labelQueue.RemoveAll(r => r.lifespan < 0);
		for (int i = Ext.labelQueue.Count - 1; i >= 0; i--) { Ext.labelQueue[i].lifespan -= dt; }
		Ext.drawQueue.RemoveAll(r => r.lifespan < 0);
		for (int i = Ext.drawQueue.Count - 1; i >= 0; i--) { Ext.drawQueue[i].lifespan -= dt; }
	}

	public async void TransitionScene(string scene) {
		if (isLoading) return;
		print($"Transitioning to scene: {scene}");
		isLoading = true;
		loading.SetActive(true);
		AsyncOperation unload = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
		AsyncOperation load = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		while (!unload.isDone || !load.isDone) { await Awaitable.NextFrameAsync(); }
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
		loading.SetActive(false);
		isLoading = false;
	}


#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (!Application.isPlaying) {
			Ext.labelQueue.Clear();
			Ext.drawQueue.Clear();
		}
		foreach (var label in Ext.labelQueue) { Handles.Label(label.position, label.text, label.style); }
		foreach (var draw in Ext.drawQueue) { draw.action(); }
	}
#endif
}
