using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

[CustomEditor(typeof(LayoutDebug))]
public class LayoutDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var layoutDebug = target as LayoutDebug;

        if(GUILayout.Button("Generate Layout"))
        {
            if (layoutDebug.gameMode == GameMode.BossRush)
            {
                layoutDebug.BossRush();
            }
            else if (layoutDebug.gameMode == GameMode.ClassicBossRush)
            {
                layoutDebug.ClassicBossRush();
            }
            else
            {
                layoutDebug.GenerateLayout();
            }
        }

        if (GUILayout.Button("Random Layout"))
        {
            if (layoutDebug.clearLogOnRoomCountTest) { Debug.ClearDeveloperConsole(); }
            layoutDebug.seed = Random.Range(0, Constants.maxSeed);
            layoutDebug.password = null;
            layoutDebug.GenerateLayout();
        }

        if (GUILayout.Button("Test Room Count"))
        {
            layoutDebug.TestRoomCounts();
        }

        if(GUILayout.Button("Random Seed"))
        {
            layoutDebug.seed = Random.Range(0, Constants.maxSeed);
        }

        if (GUILayout.Button("Load From Save"))
        {
            string path = EditorUtility.OpenFilePanel("Load Save Slot", "C:\\Users\\MattBitner\\AppData\\LocalLow\\Matt Bitner\\A Robot Named Fight", "dat");
            if (path.Length != 0 && File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var save = JsonConvert.DeserializeObject<SaveSlotData>(json);
                layoutDebug.allAchievements = false;
                layoutDebug.achievements = new List<AchievementID>(save.achievements);
                if (save.activeGameData != null)
                {
                    layoutDebug.seed = save.activeGameData.seed;
                    layoutDebug.gameMode = save.activeGameData.gameMode;
                }
            }
        }

        if (GUILayout.Button("Load Layout From Save"))
        {
            string path = EditorUtility.OpenFilePanel("Load Save Slot", "C:\\Users\\MattBitner\\AppData\\LocalLow\\Matt Bitner\\A Robot Named Fight", "dat");
            if (path.Length != 0 && File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var layout = JsonConvert.DeserializeObject<RoomLayout>(json);
                layoutDebug.seed = layout.seed;
                layoutDebug.password = layout.password;
                layoutDebug.currentLayout = layout;
                layoutDebug.gameMode = layout.gameMode;
            }
        }

        if (GUILayout.Button("TestRandom"))
        {
            var xor = new XorShift(43243419);
            Debug.Log("ints 0 to 5");
            Debug.Log("===========");
            for (int i = 0; i < 100; i++)
            {
                Debug.Log(xor.Range(0, 5));
            }

            Debug.Log("===========");
            Debug.Log("floats 0 to 5");
            Debug.Log("===========");
            for (int i = 0; i < 100; i++)
            {
                Debug.Log(xor.Range(0f, 5f));
            }
        }

        if(GUILayout.Button("TestUnevenSplit"))
        {
            var numberToSplit = 6;
            var waysToSplit = 3;
            Debug.Log("Uneven split of " + numberToSplit + " into " + waysToSplit + " numbers");
            Debug.Log("===========");
            var split = Constants.UnevenSplit(numberToSplit, waysToSplit, new MicrosoftRandom());
            foreach (var number in split)
            {
                Debug.Log(number);
            }
        }
    }
}
