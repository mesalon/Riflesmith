using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

public class Copyer : EditorWindow {
	private GameObject object1;
	private GameObject object2;

	[MenuItem("Tools/Ragdoll Copyer")]
	public static void ShowWindow() {
		GetWindow<Copyer>("Ragdoll Copyer");
	}

	void OnGUI() {
		GUILayout.Label("Ragdoll Copyer Tool", EditorStyles.boldLabel);

		object1 = (GameObject)EditorGUILayout.ObjectField("Object 1", object1, typeof(GameObject), true);
		object2 = (GameObject)EditorGUILayout.ObjectField("Object 2", object2, typeof(GameObject), true);
		if (GUILayout.Button("Fix")) {
			int fixedAmt = 0;
			Transform[] children1 = object1.GetComponentsInChildren<Transform>();
			Transform[] children2 = object2.GetComponentsInChildren<Transform>();
			Debug.Log($"{children1.Length} and {children2.Length}");
			foreach (Transform child in children1) {
				Transform found = children2.FirstOrDefault(x => x.name == child.name);
				if (found) {
					fixedAmt++;
					Debug.Log("Fixed!");
					ComponentCopier.CopyComponents(child.gameObject, found.gameObject);
				}
			}
			Debug.Log($"Fixed {fixedAmt} children out of {children1.Length} and {children2.Length}");
		}
	}
}

public static class ComponentCopier
{
    /// <summary>
    /// Copies all components from a source GameObject to a target GameObject.
    /// </summary>
    /// <param name="source">The GameObject to copy components from.</param>
    /// <param name="target">The GameObject to paste components onto.</param>
    public static void CopyComponents(GameObject source, GameObject target)
    {
        if (source == null || target == null)
        {
            Debug.LogError("Source or target GameObject is null!");
            return;
        }

        // Get all components from the source
        Component[] sourceComponents = source.GetComponents<Component>();
        if (sourceComponents == null || sourceComponents.Length == 0)
        {
            Debug.LogWarning("No components found on source GameObject!");
            return;
        }

        // Register undo for the target GameObject
        Undo.RegisterCompleteObjectUndo(target, $"{target.name}: Paste All Components");

        foreach (Component sourceComponent in sourceComponents)
        {
            if (sourceComponent == null) continue;

            // Skip Transform since every GameObject already has one
            if (sourceComponent is Transform) continue;

            // Copy the component to the clipboard
            UnityEditorInternal.ComponentUtility.CopyComponent(sourceComponent);

            // Check if the target already has this component
            Component targetComponent = target.GetComponent(sourceComponent.GetType());

            if (targetComponent != null) // If the component already exists
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentValues(targetComponent))
                {
                    Debug.Log($"Successfully pasted: {sourceComponent.GetType()} onto {target.name}");
                }
                else
                {
                    Debug.LogError($"Failed to paste values for: {sourceComponent.GetType()} onto {target.name}");
                }
            }
            else // If the component doesn’t exist, add it as new
            {
                if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target))
                {
                    Debug.Log($"Successfully added and pasted: {sourceComponent.GetType()} onto {target.name}");
                }
                else
                {
                    Debug.LogError($"Failed to add: {sourceComponent.GetType()} onto {target.name}");
                }
            }
        }
    }
}