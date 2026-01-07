using System;
using FMODUnity;
using UnityEngine;

public class SimpleAudio : MonoBehaviour {
	[SerializeField] EventReference clip;

	void Start() {
		RuntimeManager.PlayOneShot(clip, transform.position);
	}
}
