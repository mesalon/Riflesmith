using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WorldSettings", menuName = "Scriptable Objects/WorldSettings")]
public class WorldSettings : ScriptableObject {
    public AnimationCurve blowbackCurve;
}
