using UnityEditor;
using UnityEngine;
using System.Reflection;

public class RangeDrawer : PropertyDrawer {
    private const float SubLabelSpacing = 4;
    private const float BottomSpacing = 2;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        pos.height -= BottomSpacing;
        label = EditorGUI.BeginProperty(pos, label, prop);
        var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

        // --- Attribute Logic ---
        string minName = "Min";
        string maxName = "Max";
        
				var namedAttribute = fieldInfo.GetCustomAttribute<NamedRangeAttribute>();
        // Check if the property has our custom attribute
        if (namedAttribute != null) {
            minName = namedAttribute.MinLabel;
            maxName = namedAttribute.MaxLabel;
        }

        // --- Prepare data for drawing ---
        var labels = new[] { new GUIContent(minName), new GUIContent(maxName) };
        var properties = new[] { prop.FindPropertyRelative("Min"), prop.FindPropertyRelative("Max") };
        
        DrawMultiplePropertyFields(contentRect, labels, properties);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) + BottomSpacing;
    }

    private static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props) {
        var indent = EditorGUI.indentLevel;
        var labelWidth = EditorGUIUtility.labelWidth;

        var propsCount = props.Length;
        var width = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;
        var contentPos = new Rect(pos.x, pos.y, width, pos.height);
        
        EditorGUI.indentLevel = 0;
        for (var i = 0; i < propsCount; i++) {
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
            EditorGUI.PropertyField(contentPos, props[i], subLabels[i]);
            contentPos.x += width + SubLabelSpacing;
        }

        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.indentLevel = indent;
    }
}

[CustomPropertyDrawer(typeof(FloatRange))]
public class FloatRangeDrawer : RangeDrawer { }
[CustomPropertyDrawer(typeof(IntRange))]
public class IntRangeDrawer : RangeDrawer { }
