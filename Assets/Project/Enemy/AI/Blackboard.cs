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
	public int x, y;
	public float simMotion;
	public Gradient heatmap;
	public float maxHeatmapTime;

	public Player target;
	public Vector3? aimFocus;
	public Vector3 targetLKP;
	public Transform dingle;
	public Transform focus;
	public Vector3? cover;
}
