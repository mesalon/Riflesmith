using UnityEngine;
using FMODUnity;

[CreateAssetMenu()]
public class CartridgeData : ScriptableObject {
	public EventReference shotSound;	
	public CartridgeObject visual;
	public Material headMat, tailMat;
	public string caliber;
	public float penetration;
	public float damage;
	public float bulletMass;
	public float energy;
}

[System.Serializable] public struct Cartridge {
	public CartridgeData data;
	public bool isFired;

	public Cartridge(CartridgeData data) {
		this.data = data;
		isFired = false;
	}

	public readonly void Render(Matrix4x4 matrix) {
		CartridgeVisual visual = isFired ? data.visual.fired : data.visual.unfired;
		if (visual.mat && visual.mesh) {
			Transform t = visual.gameObject.transform;
			Matrix4x4 local = Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
			Graphics.RenderMesh(new RenderParams(visual.mat), visual.mesh, 0, matrix * local);
		}
	}
}
