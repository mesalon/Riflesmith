using UnityEngine;

public class Minimap : MonoBehaviour {

    void Update() {
        if(GameManager.I.CurrentMission != null) transform.LookAt(GameManager.I.CurrentMission.CompassPosition);
    }
}