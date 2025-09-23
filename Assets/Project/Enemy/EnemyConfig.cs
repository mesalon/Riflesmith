using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject {
	public BurstCfg enemy;
	public VisionCfg vision;
	public LocomotionCfg locomotion;
	public BodyCfg body;
	public CoverParams cover = CoverParams.Default;
}
