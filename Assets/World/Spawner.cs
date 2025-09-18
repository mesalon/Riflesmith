using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    [SerializeField] private GameObject enemyPF;
    [SerializeField] private List<Transform> points;
    [SerializeField] private float spawnInterval;
    private float t;
    
    void Update() {
        if (t >= spawnInterval) {
            Instantiate(enemyPF, points[Random.Range(0, points.Count)]);
            t = 0;
        }
        t += Time.deltaTime;
    }
}