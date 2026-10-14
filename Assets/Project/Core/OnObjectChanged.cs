using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using UnityEditor;
#endif

public class OnObjectChangedAttribute : PropertyAttribute {
	public readonly string methodName;
	public OnObjectChangedAttribute(string methodName) => this.methodName = methodName;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OnObjectChangedAttribute))]
public class OnObjectChangedDrawer : PropertyDrawer {
	public override VisualElement CreatePropertyGUI(SerializedProperty property) {
		PropertyField field = new(property);
		Object previousValue = property.objectReferenceValue;
		var attr = (OnObjectChangedAttribute)attribute;
		Object target = property.serializedObject.targetObject;
		System.Type type = fieldInfo.DeclaringType;
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		MethodInfo method = type.GetMethod(attr.methodName, flags);
		if (method != null) {
			field.TrackPropertyValue(property, e => {
					string[] parts = property.propertyPath.Split('.');
					object instance = target;
					for (int i = 0; i < parts.Length - 1; i++) {
						FieldInfo f = instance.GetType().GetField(parts[i], flags);
						instance = f.GetValue(instance);
					}
					method.Invoke(instance, new object[] { previousValue, e.objectReferenceValue });
					previousValue = e.objectReferenceValue;
					});
		} else {
			Debug.Log($"No method called {attr.methodName} found on object {type.Name}");
		}
		return field;
	}
}
#endif
