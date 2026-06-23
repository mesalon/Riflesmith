using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class HandPoseObject : ScriptableObject {
	public HandPose data;

	public static implicit operator HandPose(HandPoseObject hpo) => hpo.data;

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(HandPoseObject))]
	public class HandPoseDrawer : PropertyDrawer {
		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			VisualElement root = new();
			root.style.flexDirection = FlexDirection.Row;

			PropertyField prop = new(property);
			prop.style.flexGrow = 1;
			root.Add(prop);

			Object owner = property.serializedObject.targetObject;
			string propPath = property.propertyPath;
			root.Add(new Button(() => {
						// Recreate the object and property references because they're probably gone by now.
				SerializedObject so = new(owner);
				SerializedProperty p = so.FindProperty(propPath);

				GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/poser.prefab");
				PoseCreator poser = Instantiate(asset).GetComponent<PoseCreator>();
				Selection.activeObject = poser;
				HandPoseObject hp = p.objectReferenceValue as HandPoseObject;
				if (!hp) {
					hp = CreateInstance<HandPoseObject>();
					hp.data = new(poser.bones.Length);
					string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Project/Humans/Player/Grab Poses/NewPose.asset");
					AssetDatabase.CreateAsset(hp, assetPath);
					AssetDatabase.SaveAssets();

					p.objectReferenceValue = hp;
					so.ApplyModifiedProperties();
				}
				poser.InitializeFrom(hp, (owner as Component).transform);
			}) { text = "Edit" });
			return root;
		}
	}
#endif
}
