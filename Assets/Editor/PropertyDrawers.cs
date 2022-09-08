using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (Int2D))]
public class Int2DDrawer :PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var x = property.FindPropertyRelative("x");
        var y = property.FindPropertyRelative("y");

        Rect xLabelRect = new Rect(position.x, position.y, 15, position.height);
        Rect xRect = new Rect(position.x + 15, position.y, 60, position.height);
        Rect yLabelRect = new Rect(position.x + 80, position.y, 15, position.height);
        Rect yRect = new Rect(position.x + 95, position.y, 60, position.height);

        EditorGUI.PrefixLabel(xLabelRect, new GUIContent("X"));
        EditorGUI.PropertyField(xRect, x, GUIContent.none);
        EditorGUI.PrefixLabel(yLabelRect, new GUIContent("Y"));
        EditorGUI.PropertyField(yRect, y, GUIContent.none);
        EditorGUI.EndProperty();

        EditorGUI.indentLevel = indent;
    }
}

[CustomPropertyDrawer(typeof(Int2DDirection))]
public class Int2DDirectionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var int2D = property.FindPropertyRelative("position");
        var direction = property.FindPropertyRelative("direction");

        Rect int2DRect = new Rect(position.x, position.y, 160, position.height);
        Rect directionRect = new Rect(position.x + 165, position.y, 60, position.height);        
        EditorGUI.PropertyField(int2DRect, int2D, GUIContent.none);
        EditorGUI.PropertyField(directionRect, direction, GUIContent.none);
        EditorGUI.EndProperty();

        EditorGUI.indentLevel = indent;
    }
}

/// <summary>
/// For info see: http://answers.unity3d.com/questions/486694/default-editor-enum-as-flags-.html
/// If this proves problematic use: http://wiki.unity3d.com/index.php/EnumFlagPropertyDrawer
/// </summary>
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
    }
}
