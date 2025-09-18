using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugInfo : MonoBehaviour {
    private TextMeshProUGUI text;

    private void Awake() {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        text.text = $"Mission: {GameManager.I.CurrentMission}\n" +
                    $"Enemies: {FindObjectsByType<Enemy>(FindObjectsSortMode.None).Where(x => !x.isDead).ToList().Count}\n" +
                    $"Tier: {GameManager.I.enemyTier}";
    }
}
