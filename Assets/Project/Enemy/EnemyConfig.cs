using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Config")]
public class EnemyConfig : ScriptableObject {
	public BurstCfg enemy;
	public BodyCfg body = BodyCfg.Default;
	public VisionCfg vision = VisionCfg.Default;
	public LocomotionCfg locomotion = LocomotionCfg.Default;
	public CoverParams cover = CoverParams.Default;
}
