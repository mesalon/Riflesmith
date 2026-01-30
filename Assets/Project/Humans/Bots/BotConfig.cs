using UnityEngine;

[CreateAssetMenu(menuName = "Bot/Config")]
public class BotConfig : ScriptableObject {
	public HandlingCfg handling;
	public BodyCfg body = BodyCfg.Default;
	public VisionCfg vision = VisionCfg.Default;
	public LocomotionCfg locomotion = LocomotionCfg.Default;
	public CoverParams cover = CoverParams.Default;
}
