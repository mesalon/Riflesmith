using UnityEngine;

public abstract class Mount : MonoBehaviour {
	public string mountType = "";
	public Receiver receiver;
	public abstract void Register();
	public abstract void Deregister();
}
