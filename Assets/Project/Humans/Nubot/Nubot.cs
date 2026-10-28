using Animancer;
using UnityEngine;

public class Nubot : Human {
	public BotLocomotion motion;
	public BotHandling handling;
	public BotVision vision;
	public BotBrain brain;
	public BotBody body;
	public AnimancerComponent anim;

	void Awake() {
		anim.Graph.PauseGraph();
	}

	void Start() {
		body.Init(this);
		brain.Init(this);
		motion.Init(this);
		vision.Init(this);
		handling.Init(this);
	}

	void Update() {
		if (body.Health > 0) {
			anim.Evaluate(Time.deltaTime);
			body.Tick();
			motion.Tick();
			brain.Tick();
			handling.Tick();
		}
	}

	void FixedUpdate() {
		if (body.Health > 0) {
			vision.FixedTick();
		}
	}

	void LateUpdate() {
		if (body.Health > 0) {
			handling.LateTick();
		}
	}
}
