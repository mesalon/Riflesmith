using System;
using UnityEngine;

public class RecoilTest : MonoBehaviour {
    [SerializeField] private RecoilCurve pos, rot;
    private float t;
    [SerializeField] private float mult;
    [SerializeField] private float time;
    private Vector3 s;
    private void Start() {
        s = transform.position;
    }

    private void Update() {
        t += Time.deltaTime;
        if (Input.GetMouseButtonDown(0)) {
            t = 0;
        }
        transform.position = s + mult * new Vector3(pos.x.Evaluate(t / time), pos.y.Evaluate(t / time), pos.z.Evaluate(t / time));
        transform.rotation = Quaternion.Euler(mult * new Vector3(rot.x.Evaluate(t / time), rot.y.Evaluate(t / time), rot.z.Evaluate(t / time)));
    }
}

[Serializable]
class RecoilCurve {
    public AnimationCurve x, y, z;
}