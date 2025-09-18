using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
class IKRig {
    public Transform head, LHand, RHand, root;
}

[Serializable]
public class HandPose {
    [FormerlySerializedAs("basePose")] public Quaternion[] poses;

    public Quaternion[] Mirror() {
        Quaternion[] mirrored = new Quaternion[poses.Length];
        for (int i = 0; i < poses.Length; i++)
            mirrored[i] = new(-poses[i].x, poses[i].y, poses[i].z, -poses[i].w);
        return mirrored;
    }

    public static  Quaternion[] Lerp(Quaternion[] a, Quaternion[] b, float t) {
        Quaternion[] poses = new Quaternion[a.Length];
        for (int i = 0; i < poses.Length; i++) {
            poses[i] = Quaternion.Lerp(a[i], b[i], t);
        }
        return poses;
    }
}

public class Rig : MonoBehaviour {
    public Transform head;
    public Hand LHand, RHand;
    public CharacterController cc;
    [SerializeField] Transform[] LHandBones, RHandBones;
    [SerializeField] Transform gear;
    [SerializeField] IKRig ikRig;
    [SerializeField] Animator anim;
    [SerializeField] HandPose idleHP, grippingHP;
    [SerializeField] bool isMock;
    private Controls controls;

    private bool isGrounded;

    void Awake() {
        cc = GetComponent<CharacterController>();
        if (isMock) return;
        controls = new();
    }

    private void OnEnable() {
        if (isMock) return;
        controls.Enable();
        Application.onBeforeRender += UpdateHead;
    }

    private void OnDisable() {
        if (isMock) return;
        controls.Disable();
        Application.onBeforeRender -= UpdateHead;
    }

    [BeforeRenderOrder(-30000)]
    private void UpdateHead() {
        Vector3 pos = controls.Head.Pos.ReadValue<Vector3>();
        head.localPosition = pos;
        head.localRotation = controls.Head.Rot.ReadValue<Quaternion>();
    }

    private void Update() {
        if (isMock) return;

        LHand.Input.stick = controls.LHand.Stick.ReadValue<Vector2>();
        LHand.Input.stickButton = controls.LHand.StickButton.ReadValue<float>() == 1;
        LHand.Input.trigger = controls.LHand.Trigger.ReadValue<float>();
        LHand.Input.grip = controls.LHand.Grip.ReadValue<float>();
        LHand.Input.farButton = controls.LHand.FarButton.ReadValue<float>() == 1;
        LHand.Input.nearButton = controls.LHand.NearButton.ReadValue<float>() == 1;

        RHand.Input.stick = controls.RHand.Stick.ReadValue<Vector2>();
        RHand.Input.stickButton = controls.RHand.StickButton.ReadValue<float>() == 1;
        RHand.Input.trigger = controls.RHand.Trigger.ReadValue<float>();
        RHand.Input.grip = controls.RHand.Grip.ReadValue<float>();
        RHand.Input.farButton = controls.RHand.FarButton.ReadValue<float>() == 1;
        RHand.Input.nearButton = controls.RHand.NearButton.ReadValue<float>() == 1;

        gear.localPosition = new(head.localPosition.x, head.localPosition.y - 0.5f, head.localPosition.z);
        gear.localRotation = Quaternion.Euler(new(0, head.localRotation.eulerAngles.y, 0));

        LHand.transform.SetPose(
            new(controls.LHand.Pos.ReadValue<Vector3>(), controls.LHand.Rot.ReadValue<Quaternion>()), true);
        RHand.transform.SetPose(
            new(controls.RHand.Pos.ReadValue<Vector3>(), controls.RHand.Rot.ReadValue<Quaternion>()), true);

        /*ikRig.root.SetPose(new(new(head.transform.position.x, 0, head.transform.position.z), Quaternion.identity));
        ikRig.head.SetPose(new(head.transform.position, head.transform.rotation));
        ikRig.LHand.SetPose(new(LHand.transform.position, LHand.transform.rotation));
        ikRig.RHand.SetPose(new(RHand.transform.position, RHand.transform.rotation));*/


        UpdateHandPose(LHandBones, HandPose.Lerp(idleHP.Mirror(), grippingHP.Mirror(), LHand.Input.grip));
        UpdateHandPose(RHandBones, HandPose.Lerp(idleHP.poses, grippingHP.poses, RHand.Input.grip));

        LHand.Tick();
        RHand.Tick();

        // Recenter parent
        if (Input.GetKeyDown(KeyCode.Space)) {
            Vector3 movement = new Vector3(head.position.x, 0, head.position.z) -
                               new Vector3(transform.position.x, 0, transform.position.z);
            transform.position += movement;
            head.localPosition -= transform.InverseTransformVector(movement);
            LHand.transform.localPosition -= transform.InverseTransformVector(movement);
            RHand.transform.localPosition -= transform.InverseTransformVector(movement);
        }
    }

    private void UpdateHandPose(Transform[] target, Quaternion[] pose) {
        for (int i = 0; i < target.Length; i++) {
            target[i].localRotation = pose[i];
        }
    }

    void FixedUpdate() {
        if (isMock) return;

        LHand.FixedTick();
        RHand.FixedTick();

        cc.height = head.localPosition.y;
        cc.center = new(head.localPosition.x, cc.height / 2, head.localPosition.z);
    }

    void LateUpdate() {
        if (isMock) return;

        LHand.LastInput = LHand.Input;
        RHand.LastInput = RHand.Input;
    }
}