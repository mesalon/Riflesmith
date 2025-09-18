using System.Collections.Generic;
using UnityEngine;

public class Magazine : Attachment {
	public Stack<ProjectileData> ammo = new();
	[Header("Magazine Settings")]
	[SerializeField] List<Transform> RoundPoints;
	[SerializeField] List<ProjectileData> toLoad;
	[SerializeField] string caliber;
	[SerializeField] int capacity;

	void Awake() {
		base.Awake();
		for (int i = 0; i < capacity; i++) {
			TryInsert(toLoad[i % toLoad.Count]);
		}
	}

	private void Update() {
		base.Update();
		ProjectileData[] rounds = ammo.ToArray();
		for (int i = 0; i < rounds.Length; i++) {
			if (RoundPoints.Count > i) {
				Graphics.RenderMesh(new(rounds[i].mat), rounds[i].mesh, 0, Matrix4x4.TRS(RoundPoints[i].position, RoundPoints[i].rotation, rounds[i].scale));
			}
		}
	}

	public override void OnAttach(FirearmReceiver f = null) { }

	public bool TryInsert(ProjectileData round) {
		if (ammo.Count < capacity && round.caliber == caliber) {
			ammo.Push(round);
			return true;
		} return false;
	}
}