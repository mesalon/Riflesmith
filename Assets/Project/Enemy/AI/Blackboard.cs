using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[System.Serializable]
public class Blackboard {
	public EnemyConfig cfg;
	public EnemyFirearm weapon;
	public Transform eyes;
	public Transform weaponAimPose, weaponRestPose;
	public Transform weaponHandle;
	public Transform ikTarget;
	public Animator anim;
	public CharacterController cc;
	public AnimationClip getUpClip;
	public Transform focus;
	public Transform dingle;
	public Seeker seeker;
	public Transform coreRag;
	public List<Transform> ragdollReference;
	public List<ConfigurableJoint> joints;
	public bool coverDebug, coverDebugFull;
	public float aimError, fixAmount, fixVariance;

	[HideInInspector] public CoverQuery cover;
	[HideInInspector] public Player target;
	[HideInInspector] public Vector3? aimFocus;
	[HideInInspector] public Vector3? targetLKP;
	[HideInInspector] public float confidence, alertness, suppression;
	[HideInInspector] public float LKPAge;
	[HideInInspector] public bool expectsToSeeTarget;
}
