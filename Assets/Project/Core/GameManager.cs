// I'm fucking boiling with rage right now
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static Camera Camera => Camera.allCameras
		.Where(c => c.targetTexture == null)
		.OrderByDescending(c => c.depth)
		.FirstOrDefault();
	public static GameManager I { get; private set; }
	public GameConfig config;
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

	void Update() {
		MyGizmos.DrawSphere(Vector3.zero, Vector3.one);
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
}
