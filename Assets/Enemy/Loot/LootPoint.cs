using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootPoint : MonoBehaviour, IInteractable {
	static readonly int opacity = Shader.PropertyToID("_Opacity");
	public Hand Interactor { get; set; }
	public bool PreventInteraction { get; set; }
	[SerializeField] DropPool pool;
	[SerializeField] float dropChance = 0.5f;
	[SerializeField] MeshRenderer mR;
	MaterialPropertyBlock mPB;
	bool isUsed;

	void Awake() {
		mPB = new();
	}

	public void OnPicked() {
		if (!isUsed && dropChance > Random.Range(0, 1)) {
			float total = 0;
			List<float> weights = new();
			List<DropEntry> gear = pool.drops;
			foreach (DropEntry item in gear) {
				float prob = item.frequency.Evaluate(GameManager.I.enemyTier);
				total += prob;
				weights.Add(prob);
			}

			float r = Random.Range(0, total);
			for (int i = 0; i < weights.Count; i++) {
				if (r <= weights[i]) {
					print($"({i}) Picked {gear[i]}");
					GameObject inst = Instantiate(gear[i].items.Random(), Interactor.transform.position, Interactor.transform.rotation);
					if (inst.TryGetComponent(out IInteractable interactable)) { Interactor.Pick(interactable); }
					break;
				}
				r -= weights[i];
			}
			isUsed = true;
			PreventInteraction = true;
			mR.GetPropertyBlock(mPB);
			mPB.SetFloat(opacity, 0);
			mR.SetPropertyBlock(mPB);
		}
	}

	public void OnHold() { }
	public void OnHoldFixed() {
		
	}

	public void OnDropped() { }
}

[Serializable] public class DropEntry {
	public string name;
	public List<GameObject> items;
	public AnimationCurve frequency = new();
}