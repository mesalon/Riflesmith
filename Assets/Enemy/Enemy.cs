using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour {
    public bool isDead;
    public float Health { get; private set; } = 100;
    public EnemyLocomotion locomotion;
    [SerializeField] float strength = 100;
    [SerializeField] private float recoveryMin = 1;
    [SerializeField] private float recoveryMax = 5;
    [SerializeField] private float recoveryDelay = 3;
    [SerializeField] private Transform coreRag;
    [SerializeField] private RagdollController ragdoll;
    [SerializeField] private Animator anim;
    [SerializeField] private List<GameObject> drops;
    [SerializeField] private AnimationClip getUpClip;
    private NavMeshAgent nav;
    public EnemyAI ai;
    private float bleeding;
    private float hitTime;
    [SerializeField] private bool debugs;
    private void Awake() {
        ragdoll.SetRagdoll(false);
        nav = GetComponent<NavMeshAgent>();
        ai = GetComponent<EnemyAI>();
        
        /*getUpClip.SampleAnimation(gameObject, 0);
        foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones))) {
            Transform t = anim.GetBoneTransform(bone);
            if (t) { getUpInitPoses.Add(new(t.localPosition, t.localRotation)); }
        }*/
    }
    
    private void Update() {
        if(debugs) DebugOverlay.CreateOverlay(transform, 2.5f, ("health", Health), ("strength", strength), ("bleeding", bleeding), ("dead", isDead));
        if (!isDead) {
            if (anim.enabled && strength <= 40) {
                anim.enabled = false;
                ragdoll.SetRagdoll(true);
                // todo downstate
            }
            
            float regenRate = Mathf.Lerp(recoveryMin, recoveryMax, hitTime - recoveryDelay / 10);
            strength = Mathf.Min(100, strength + regenRate * (Health / 100) * Time.deltaTime);
            bleeding = Mathf.Max(0, bleeding - bleeding * 0.1f * Time.deltaTime);
            ragdoll.SetForce(Mathf.Min(strength, Health) / 100);
            Health = Mathf.Clamp(Health - bleeding * Time.deltaTime, 0, 100);
            
            if (!anim.enabled && strength >= 100) { // Get up
                print("Got up");
                bleeding = 0;
                ragdoll.SetRagdoll(false);
                Recenter();
                anim.enabled = true;
            }
            
            // Death
            if (Health <= 0) {
                print("Dead");
                //if(drops.Count > 0) Instantiate(drops[Random.Range(0, drops.Count)], transform.position, transform.rotation);
                //nav.enabled = false;
                //fsm.enabled = false;
                anim.enabled = false;
                ragdoll.SetRagdoll(true);
                isDead = true;
                ragdoll.SetForce(0);
            }
        }
        
        hitTime += Time.deltaTime;
    }

    public void Damage(float amount, float force) {
        Health = Mathf.Max(0, Health - amount);
        strength = Mathf.Max(0, strength - force);
        bleeding += amount * 0.1f;
        hitTime = 0;
    }

    private void Recenter() {
        Vector3 original = coreRag.position;
        transform.position = coreRag.position;
        if (Physics.Raycast(coreRag.position, Vector3.down, out RaycastHit hitInfo, LayerMask.NameToLayer("Environment"))) {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        coreRag.position = Vector3.zero;
    }
}