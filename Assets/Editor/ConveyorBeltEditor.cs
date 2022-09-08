using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConveyorBelt))]
public class ConveyorBeltEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var belt = target as ConveyorBelt;
        serializedObject.Update();
        var size = serializedObject.FindProperty("size");
        var direction = serializedObject.FindProperty("direction");

        EditorGUILayout.BeginVertical();
        var newSize = EditorGUILayout.FloatField("Size", belt.size);
        if (newSize < 2) newSize = 2;
        var newDirection = (Direction)EditorGUILayout.EnumPopup("Direction", belt.direction);
        
        if (belt.size != newSize)
        {
            Undo.RegisterCompleteObjectUndo(target, "Change Size (" + target.name + ")");
            size.floatValue = newSize;
            belt.SetSize(newSize);
            EditorUtility.SetDirty(target);
        }

        if (belt.direction != newDirection)
        {
            Undo.RegisterCompleteObjectUndo(target, "Change Direction (" + target.name + ")");
            direction.enumValueIndex = (int)newDirection;
            belt.SetDirection(newDirection);
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
}
