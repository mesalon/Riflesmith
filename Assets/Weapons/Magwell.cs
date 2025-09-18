using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class Magwell : AttachmentMount {
	public Magazine Magazine => attachment as Magazine;
	
	void Update() {
		/*if (magazine && reciever.input.nearButton) {
			magazine.PreventInteraction = false;
			magazine.rb.AddForce(magazine.transform.rotation * Vector3.down * 5);
			RuntimeManager.PlayOneShot(magOut, transform.position);
			magazine.rb.isKinematic = false;
			magazine.magwell = null;
			magazine = null;
		}*/
	}
}