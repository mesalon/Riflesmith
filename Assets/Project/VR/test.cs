using UnityEngine;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour {
	void Awake() {
		DontDestroyOnLoad(this);
	}
	void OnDestroy() { print($"OnDestroy!"); }
	void OnDisable() { print($"OnDisable!"); }
	void Update() {
		if (Input.GetKeyDown(KeyCode.I)) {
			SceneManager.LoadScene("Assets/Project/World/Main/Main.unity");
		}
	}
}
