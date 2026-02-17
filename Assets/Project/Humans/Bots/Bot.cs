using RootMotion.FinalIK;
using UnityEngine;
using Animancer;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

/* AI Todo
	 Change the reaction of shootaction based on alertness
	 Modify vision by alertness
	 The handling of the gun is really janky and so is the viewing. I should fix that
	 */

public class Bot : Interactor {
	public Vector3 test;
	public BotConfig cfg;
	public Transform eyes;
	public Transform weaponHolster, weaponAimPose, weaponRestPose;
	public Transform weaponContainer;
	public Transform ikLookTarget;
	public Transform rightHand;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true, runHandling = true;

	public TransitionAsset mixer;
	public SmoothedVector2Parameter moveParam;
	public AnimancerComponent anim;
	public AnimancerLayer upperLayer;
	public AnimancerLayer lowerLayer;
	public ClipTransition equipWeapon;
	public ClipTransition dequipWeapon;
	public FullBodyBipedIK ik;
	[SerializeField] ClipTransition aim;
	[SerializeField] AvatarMask mask;
	[SerializeField] StringAsset moveX, moveY;
	public StringAsset eqDequip;

	public BotVision vision;
	public BotBody body;
	public BotHandling handling;
	public BotLocomotion motionController;
	public AIBrain brain;
	[HideInInspector] public Human self;

	private void Awake() {
		lowerLayer = anim.Layers[0];
		upperLayer = anim.Layers[1];
		upperLayer.Mask = mask;
		equipWeapon.Events.OnEnd = dequipWeapon.Events.OnEnd = OnActionEnd;
    moveParam = new SmoothedVector2Parameter(
        anim,
        moveX,
        moveY,
        0.1f);
		self = GetComponent<Human>();
		motionController = new(this);
		vision = new(this);
		handling = new(this);
		brain = new(this);
		body = new();

	}

	private void Update() {
		//upperLayer.Play(aim);
		anim.Play(mixer);

		//pitch += Input.GetAxis("Mouse Y");
		//yaw += Input.GetAxis("Mouse X");
		Vector2 moveDir = new Vector2(
				-(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
				-(Input.GetKey(KeyCode.S) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0));
		//moveDir = Vector2.ClampMagnitude(moveDir, 1);

		moveParam.TargetValue = moveDir;

		float maxComponent = Mathf.Max(Mathf.Abs(moveDir.x), Mathf.Abs(moveDir.y));
		if (maxComponent > float.Epsilon) {
			float speedMultiplier = moveDir.magnitude / maxComponent;
			mixer.Speed = speedMultiplier;
		}
		else {
			mixer.Speed = 1f;
		}
		if (body.isUp) {
			if (runLocomotion) motionController.Tick();
			if (runHandling) handling.Tick();
			if (runBrain) brain.Tick();
		}
	}

	public void Damage(float amount) {
		Damage(amount);
		body.strength = Mathf.Max(0, body.strength - amount);
	}

	private void OnActionEnd() {
		upperLayer.StartFade(0, 0.25f);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Bot))]
public class BotEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		EditorGUILayout.Space();
		Bot e = (Bot)target;
		if (e.brain != null) {
			var topItems = e.brain.actions
				.Select(a => new { Action = a, Score = a.GetScore() }) 
				.OrderByDescending(x => x.Score)
				.Take(5)
				.ToList();
			for (int i = 0; i < topItems.Count(); i++) {
				EditorGUILayout.LabelField($"{i}: {topItems[i].Action} | {topItems[i].Score}");
			}
		}
	}
}
#endif
