using UnityEngine;

public abstract class Mount : MonoBehaviour {
	public string mountType = "";
	public Receiver receiver;
	public abstract Vector3 GetAttachPoint(Vector3 center);
	public abstract void Register();
}
