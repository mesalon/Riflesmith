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

public class Bot : MonoBehaviour {
	public Vector3 test;
	public BotConfig cfg;
	public Transform eyes;
	public Transform weaponHolster, weaponAimPose, weaponRestPose;
	public Transform weaponContainer;
	public Transform ikLookTarget;
	public Transform rightHand;
	public bool runLocomotion = true, runVision = true, runBody = true, runBrain = true, runHandling = true;

	public FullBodyBipedIK ik;

	public AnimancerComponent anim;
	public TransitionAsset mixer;
	public SmoothedVector2Parameter moveParam;
	public AnimancerLayer upperLayer;
	public AnimancerLayer lowerLayer;
	public ClipTransition equipWeapon;
	public ClipTransition dequipWeapon;
	public ClipTransition aim;
	public AvatarMask mask;
	public StringAsset moveX, moveY;
	public CharacterController cc;

	public BotVision vision;
	public BotBody body;
	public BotHandling handling;
	public BotLocomotion motion;
	public AIBrain brain;
	[HideInInspector] public Human self;
	[SerializeField] RootMotionRedirect redirect;

	private void Awake() {
		self = GetComponent<Human>();
		cc = GetComponent<CharacterController>();
		motion = new(this);
		vision = new(this);
		handling = new(this);
		brain = new(this);
		body = new();
		redirect.target = motion;
	}

	private void Update() {
		//motion.pitch += Input.GetAxis("Mouse Y") * 3;
		motion.MoveDirect(new Vector2(
				-(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
				-(Input.GetKey(KeyCode.S) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)));
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(GameManager.Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {
			motion.Move(hit.point, Pace.Jog);
		}
		if (body.isUp) {
			if (runLocomotion) motion.Tick();
			if (runHandling) handling.Tick();
			if (runBrain) brain.Tick();
		}
	}

	private void FixedUpdate() {
		if (runBrain) brain.FixedTick();
	}

	public void Damage(float amount) {
		Damage(amount);
		body.strength = Mathf.Max(0, body.strength - amount);
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
