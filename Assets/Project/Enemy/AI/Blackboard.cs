using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Blackboard {
	public EnemyConfig cfg;
	public EnemyFirearm weapon;
	public Transform transform;
	public Transform eyes;
	public Animator anim;
	public CharacterController cc;
	public AnimationClip getUpClip;
	public Transform coreRag;
	public List<Transform> ragdollReference;
	public List<ConfigurableJoint> joints;
	public UnityEngine.Animations.Rigging.Rig gunRestRig;
	public Transform ikTarget;

	public Player target;
	public Vector3? aimFocus;
	public Vector3 targetLKP;
	public Transform focus;
	public Vector3? cover;
	public bool bool1;
	public Transform dingle;
}
