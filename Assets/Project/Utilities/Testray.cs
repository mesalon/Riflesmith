using Animancer;
using UnityEngine;

public class Testray : MonoBehaviour {
	[SerializeField] AnimationClip clip;
	[SerializeField] AnimancerComponent anim;
	void Awake() {
		anim.Graph.PauseGraph();
	}

	void Update() {

		anim.Evaluate(Time.deltaTime);
		anim.Play(clip);
		Ext.DrawSkeleton(transform, Color.white);
	}
}
