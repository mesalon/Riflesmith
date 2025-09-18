using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casing : MonoBehaviour {
	public List<Collider> phys;
	public Rigidbody rb;
	
	void Start() { Destroy(gameObject, 5); } 
}