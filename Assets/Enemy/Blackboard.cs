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
}

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject {
	public BurstCfg enemy;
	public VisionCfg vision;
	public LocomotionCfg locomotion;
	public BodyCfg health;
	public CoverParams cover = CoverParams.Default;
}
