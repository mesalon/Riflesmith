using UnityEngine;

[CreateAssetMenu(fileName = "AISettings", menuName = "Enemy/Settings")]
public class EnemySettings : ScriptableObject {
	public IntRange burst;
	public FloatRange delay;
	public float rangeBurstWeight, skillBurstWeight, ammoBurstWeight, intentBurstWeight, recoilBurstWeight, panicBurstWeight;
	public float rangeDelayWeight, skillDelayWeight, ammoDelayWeight, intentDelayWeight, recoilDelayWeight, panicDelayWeight;
	public float burstWeightBase, delayWeightBase;
	public float inconsistencyBase;
}

[System.Serializable]
public struct FloatRange {
	public float Min, Max;
}

[System.Serializable]
public struct IntRange {
	public int Min, Max;
}

public class NamedRangeAttribute : PropertyAttribute {
    public readonly string MinLabel;
    public readonly string MaxLabel;

    public NamedRangeAttribute(string minLabel, string maxLabel) {
        this.MinLabel = minLabel;
        this.MaxLabel = maxLabel;
    }
}
