using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

[ExecuteAlways]
public class Magazine : AmmoSource {
	[SerializeField] List<Cartridge> ammo = new();
	[SerializeField] int capacity;
	[SerializeField] SplineContainer ammoPath;
	[SerializeField] Vector3 ammoRotation;
	[SerializeField] float stackWidth;

#if UNITY_EDITOR
	void OnEnable() { if (!Application.isPlaying) EditorApplication.update += EditorApplication.QueuePlayerLoopUpdate; }
	void OnDisable() { if (!Application.isPlaying) EditorApplication.update -= EditorApplication.QueuePlayerLoopUpdate; }
#endif

	void Update() {
		for (int i = 0; i < ammo.Count; i++) {
			if (ammoPath) {
				ammoPath.Evaluate(i / (float)capacity, out float3 pos, out float3 tan, out float3 up);
				Vector3 lPos = transform.InverseTransformPoint(pos);
				float side = i % 2 == 0 ? 1 : -1;
				lPos.x += stackWidth * side;
				Quaternion rot = Quaternion.LookRotation(tan, up) * Quaternion.Euler(ammoRotation);
				ammo[i].Render(Matrix4x4.TRS(transform.TransformPoint(lPos), rot, transform.lossyScale));
			}
			Ext.Label(ammoPath.EvaluatePosition(0), $"Ammo: {ammo.Count}");
		}
	}

	public override Cartridge Strip() {
		Cartridge c = default;
		if (ammo.Count > 0) {
			c = ammo[ammo.Count - 1];
			ammo.RemoveAt(ammo.Count - 1);
		}
		return c;
	}

	public bool TryLoad(Cartridge cartridge) {
		if (ammo.Count < capacity) { ammo.Add(cartridge); return true; }
		return false;
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(Magazine), true), CanEditMultipleObjects]
	public class MagazineEditor : FixedPartEditor {
		public override void BuildInspector(VisualElement root) {
			base.BuildInspector(root);
			var t = (Magazine)target;
			ObjectField data = new("Round") { objectType = typeof(CartridgeData) };
			root.Add(data);
			root.Add(new Button(() => {
						Undo.RecordObject(t, "Fill magazine");
						t.ammo.Clear();
						if (data.value) {
						for (int i = 0; i < t.capacity; i++) { t.TryLoad(new((CartridgeData)data.value)); }
						}
						EditorUtility.SetDirty(t);
						}) { text = "Fill" });
			string s = "";
			for (int i = 0; i < t.capacity; i++) {
				if (i < t.ammo.Count) {
					Cartridge round = t.ammo[i];
					s += $"{(round.data ? round.data.name : "NULL")}{(round.isFired ? " (Fired)" : "")}\n"; 
				} else {
					s += $"(empty)\n";
				}
			}
			var list = new Foldout { text = "Rounds", };
			list.Add(new Label(s));
			root.Add(list);
		}
	}
#endif
}
