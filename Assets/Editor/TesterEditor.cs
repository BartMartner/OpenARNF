using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tester))]
public class TesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var tester = (Tester)target;

        GUILayout.Label("Base Length: " + SeedHelper.baseDefinition.Length);

        var defaultColor = GUI.backgroundColor;

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("ConvertKeyToSeed"))
        {
            tester.ConverKeyToSeed();
        }

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("ConvertSeedToKey"))
        {
            tester.ConvertSeedToKey();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Key To Traversal Items"))
        {
            tester.ConvertKeyToTraversalItems();
        }

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Traversal Items To Key"))
        {
            tester.ConvertTraversalItemsToKey();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Key To Achievements"))
        {
            tester.ConvertKeyToAchievements();
        }

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Achievements To Key"))
        {
            tester.ConvertAchievementsToKey();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Key To Parameters"))
        {
            tester.ConvertKeyToParameters();
        }

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Parameters To Key"))
        {
            tester.ConvertParametersToKey();
        }

        GUI.backgroundColor = defaultColor;

        if (GUILayout.Button("Scrap Cost Report"))
        {
            tester.ScrapCostReport();
        }

        if (GUILayout.Button("Count NonItem Achievements"))
        {
            tester.CountNonItemAchievements();
        }

        if (GUILayout.Button("Add All Achievements"))
        {
            tester.AddAllAchievements();
        }
    }
}
