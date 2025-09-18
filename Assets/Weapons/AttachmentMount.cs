using UnityEngine;
using UnityEngine.Serialization;

public enum AttachmentType { Fixed, Rail }

public class AttachmentMount : MonoBehaviour {
	public FirearmReceiver Receiver {
		get { // One of these have to be assigned
			if (receiver) return receiver;
			if (parent && parent.Receiver) return parent.Receiver;
			return null;
		}
	}
	public Rigidbody Rb {
		get { // One of these have to be assigned
			if (grab) return grab.rb;
			if (parent && parent.Receiver) return parent.rb;
			return null;
		}
	}
	
	public string mountType;
	public AttachmentType type;
	public float railMin, railMax;
	[HideInInspector] public Attachment attachment;
	[SerializeField] Attachment parent;
	[SerializeField] FirearmReceiver receiver;
	[SerializeField] GrabInteractable grab;

	public void UpdateAttachment(FirearmReceiver f) {
		if(attachment) {
			if (f) { attachment.OnAttach(f); }
			foreach (AttachmentMount mount in attachment.mounts) {
				mount.UpdateAttachment(f);
			}
		}
	}
}