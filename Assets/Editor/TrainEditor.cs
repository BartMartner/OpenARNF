using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Train))]
public class TrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var train = target as Train;

        if (GUILayout.Button("PreSort"))
        {
            train.PreSort();
        }
    }
}
