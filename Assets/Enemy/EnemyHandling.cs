using UnityEngine;

public class EnemyHandling : MonoBehaviour {
    private EnemyFirearm gun => ai.weapon;
    [SerializeField] private EnemyAI ai;
    [SerializeField] private float reactionTime;
    [SerializeField] private float activeRecovery;
    [SerializeField] private float minErrorV = 1, maxErrorV = 1;
    [SerializeField] private float minErrorH = 1, maxErrorH = 1;
    
    private void Update() {

    }
}