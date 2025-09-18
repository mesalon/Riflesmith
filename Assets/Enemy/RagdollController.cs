using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RagdollController : MonoBehaviour {
    [SerializeField] List<Transform> reference;
    [SerializeField] List<ConfigurableJoint> joints;
    [SerializeField] private Rigidbody core;
    [SerializeField] float force;
    [SerializeField] private float damper;
    [SerializeField] private float speed = 1;
    private Vector3[] axes;

    private void Awake() {
        SetForce(1);
        axes = new Vector3[joints.Count];
        for (int i = 0; i < joints.Count; i++)
            axes[i] = Random.onUnitSphere;
    }

    private void Update() {
        //for (int i = 0; i < joints.Count; i++) { Debug.DrawRay(joints[i].transform.position, joints[i].targetRotation * Vector3.up * 0.5f, Color.red); }
    }

    private void FixedUpdate() {
        // todo, fix the animations and remove the random spinning
        for (int i = 0; i < joints.Count; i++) {
            joints[i].targetRotation = Quaternion.Euler(
                Mathf.PerlinNoise(Time.time * speed + i + GetInstanceID(), 0) * 360,
                Mathf.PerlinNoise(0, Time.time * speed + i + GetInstanceID()) * 360,
                Mathf.PerlinNoise(Time.time * speed + i * 10 + GetInstanceID(), Time.time * speed + i) * 360
            );
        }
    }
    
    public void SetRagdoll(bool state) {
        foreach (ConfigurableJoint j in joints) {
            Rigidbody rag = j.GetComponent<Rigidbody>();
            rag.isKinematic = !state;
            if (!rag.isKinematic) {
                rag.angularVelocity = Vector3.zero;
                rag.linearVelocity = Vector3.zero;
            }
        }
    }
    
    public void SetForce(float scale) {
        JointDrive drive = joints[0].angularXDrive;
        drive.positionSpring = scale * force;
        drive.positionDamper = scale * damper;
        foreach (ConfigurableJoint j in joints) {
            j.angularXDrive = drive;
            j.angularYZDrive = drive;
        }
    }
}

//[CustomEditor(typeof(RagdollController))]
public class RagdollControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Fix")) {
            RagdollController doll = (RagdollController)target;
            foreach (CapsuleCollider cap in doll.GetComponentsInChildren<CapsuleCollider>()) {
                cap.center /= 100;
                cap.radius /= 100;
                cap.height /= 100;
            }
        }
    }
}
