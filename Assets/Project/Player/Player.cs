using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour {
	[SerializeField] float health = 100;
	[SerializeField] Volume damage, damageTemp;
	public Rig rig;
	[SerializeField] float recoverSpeed;
	float maxHealth;
	[SerializeField] private bool isMock;
	[SerializeField] private float spd = 50;

	void Start() {
		maxHealth = health;
	}

	void Update() {
		if (isMock) return;
		damageTemp.weight = Mathf.Lerp(damageTemp.weight, 0, recoverSpeed * Time.deltaTime);
		GetComponent<CharacterController>().Move(new(Mathf.Sin(Time.time * spd), 0, 0));
	}

	public void Damage(float amount) {
		health -= amount;
		if (isMock) return;
		damage.weight = 1 - health / maxHealth;
		damageTemp.weight = 1;
	}
}
