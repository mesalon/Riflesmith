using System.Collections.Generic;
using UnityEngine;

// At the beginning of the game, there's a fixed amount of resources that are allocated throughout the facility as HC sees fit
// As soon as the alarm sounds, reinforcement resources begin trickling through, and HC dynamically places them at the entry points
// HC gives orders to squad leaders, trying to encircle the player and kill him
public class HighCommand : MonoBehaviour {
	[SerializeField] Nubot soldier;
	[SerializeField] float resourceGain;
	[SerializeField] float mostGoopedSolider;
	[SerializeField] float initialResources;
	private List<Human> men;
	private float resources;

	void Start() {

	}

	void Update() {
		resources += resourceGain * Time.deltaTime;
		mostGoopedSolider += resourceGain * Time.deltaTime;
	}

	private void Deploy(float level, Vector3 position) {
		level = Mathf.Max(level, mostGoopedSolider);
		position = position; // todo Find suitable position
		Nubot bot = Instantiate(soldier, position, Quaternion.identity);
		bot.SendOrder();
	}
}
