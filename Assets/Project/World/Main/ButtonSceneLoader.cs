using UnityEngine;

public class ButtonSceneLoader : MonoBehaviour {
	public void LoadFacility() => GameManager.I.TransitionScene("Facility");
}
