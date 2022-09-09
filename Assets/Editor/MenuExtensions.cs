using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using CreativeSpore.SuperTilemapEditor;
using UnityEditor.SceneManagement;
using System.Text;
using System;

public class MenuExtensions : MonoBehaviour
{
    [MenuItem("Extensions/Snap to Grid 0.5f %#g")]
    static void MenuSnapToGridPoint5()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
        {
            t.position = new Vector3(
                Mathf.Round(t.position.x / 0.5f) * 0.5f,
                Mathf.Round(t.position.y / 0.5f) * 0.5f,
                Mathf.Round(t.position.z / 0.5f) * 0.5f
            );
        }
    }

    [MenuItem("Extensions/Snap to Grid Pixel %g")]
    static void MenuSnapToPixel()
    {
        var p = 1 / 32d;
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
        {
            var x = t.position.x / p;
            var y = t.position.y / p;
            x = (int)(x + (x >= 0 ? 0.5 : -0.5)) * p;
            y = (int)(y + (y >= 0 ? 0.5 : -0.5)) * p;
            t.position = new Vector3((float)x, (float)y, 0);
        }
    }

    [MenuItem("Extensions/Sort Game Objects By Y then X")]
    public static void SortSelected()
    {
        var transforms = new List<Transform>(UnityEditor.Selection.GetTransforms(SelectionMode.TopLevel));
        transforms = transforms.OrderByDescending(t => t.position.y).ThenBy(t => t.position.x).ToList();
        var lowestIndex = transforms[0].GetSiblingIndex();
        foreach (var t in transforms)
        {
            var i = t.GetSiblingIndex();
            if (i < lowestIndex)
            {
                lowestIndex = i;
            }
        }

        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].SetSiblingIndex(i + lowestIndex);
        }
    }

    [MenuItem("Extensions/Sort Game Objects By Distance From (0,0,0)")]
    public static void SortSelectedFromZero()
    {
        var transforms = new List<Transform>(Selection.GetTransforms(SelectionMode.TopLevel));
        transforms = transforms.OrderByDescending(t => -Vector3.Distance(t.position, Vector3.zero)).ToList();
        var lowestIndex = transforms[0].GetSiblingIndex();
        foreach (var t in transforms)
        {
            var i = t.GetSiblingIndex();
            if (i < lowestIndex)
            {
                lowestIndex = i;
            }
        }

        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].SetSiblingIndex(i + lowestIndex);
        }
    }

    [MenuItem("Extensions/Snap Polygon Collider")]
    static void MenuSnapPolygonCollider()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
        {
            var polygonCollider = t.GetComponent<PolygonCollider2D>();
            if (polygonCollider)
            {
                Undo.RecordObject(t, "Snap Polygon Collider");
                for (int i = 0; i < polygonCollider.pathCount; i++)
                {
                    var path = polygonCollider.GetPath(i);
                    for (int j = 0; j < path.Length; j++)
                    {
                        var point = path[j];
                        path[j] = new Vector2(Mathf.Round(point.x * 4) / 4, Mathf.Round(point.y * 4) / 4);
                    }
                    polygonCollider.SetPath(i, path);
                }
            }
        }
    }

    [MenuItem("Extensions/Item Info/Save CSV")]
    static void GetItemCSV()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));

        var stringBuilder = new StringBuilder();
        var row = string.Empty;
        row += "name, ";
        row += "fullName, ";
        row += "type, ";
        row += "description, ";
        row += "advancedDescription, ";
        row += "itemPageDescription, ";
        //pools
        row += "artificerPool, ";
        row += "bossRushPool, ";
        row += "deathmatch, ";
        row += "gunSmithPool, ";
        row += "orbSmithPool, ";
        row += "theTraitorPool, ";
        row += "shrinePools, ";
        row += "startingItemPool, ";
        //scrap cost
        row += "grayScrapCost, ";
        row += "redScrapCost, ";
        row += "greenScrapCost, ";
        row += "blueScrapCost, ";
        //other
        row += "allowHovering, ";
        row += "applyDamageTypeToProjectile, ";
        row += "arcShots, ";
        row += "baseAttackMultiplier, ";
        row += "baseDamageBonus, ";
        row += "baseEnergyBonus, ";
        row += "baseHealthBonus, ";
        row += "baseShotSizeBonus, ";
        row += "baseShotSpeedBonus, ";
        row += "baseSpeedBonus, ";
        row += "bonusNanobots, ";
        row += "damageType, ";
        row += "environmentalResistance, ";
        row += "fireArc, ";
        row += "follower, ";
        row += "homing, ";
        row += "homingArc, ";
        row += "homingRadius, ";
        row += "hoverMaxVelocity, ";
        row += "hoverTime, ";
        row += "ignoreTerrain, ";
        row += "isActivatedItem, ";
        row += "isEnergyWeapon, ";
        row += "isTraversalItem, ";
        row += "itemEnergyRegenRate, ";
        row += "maxItemOrder, ";
        row += "minimumShopEnvironment, ";
        row += "minItemOrder, ";
        row += "noSprite, ";
        row += "ownedLimit, ";
        row += "penetrativeShot, ";
        row += "pickUpRangeBonus, ";
        //row += "projectileChildEffect.ToString(), ";
        row += "projectileStatusEffects, ";
        row += "projectileType, ";
        row += "regenerationRate, ";
        row += "rendersUseless, ";
        row += "requiredAchievement, ";
        row += "speedMod, ";
        //sprites
        row += "decalOrder, ";
        row += "armDecal, ";
        row += "armSprite, ";
        row += "headSprite, ";
        row += "legDecal, ";
        row += "legSprite, ";
        row += "nontraversalPool, ";
        row += "shoulderPadSprite, ";
        row += "torsoDecal, ";
        row += "torsoSprite, ";
        row += "paletteOverride, ";
        stringBuilder.AppendLine(row);

        foreach (var item in loadedItemInfos)
        {
            row = string.Empty;
            row += item.name + ", ";
            row += item.fullName + ", ";
            row += item.type + ", ";
            row += item.description + ", ";
            row += item.advancedDescription + ", ";
            row += item.itemPageDescription + ", ";
            //pools
            row += item.artificerPool + ", ";
            row += item.bossRushPool + ", ";
            row += item.deathmatch + ", ";
            row += item.gunSmithPool + ", ";
            row += item.orbSmithPool + ", ";
            row += item.theTraitorPool + ", ";
            row += item.shrinePools + ", ";
            row += item.startingItemPool + ", ";
            row += item.restrictedEnvironments + ", ";
            row += item.finalItem + ", ";
            //scrap cost
            row += item.grayScrapCost + ", ";
            row += item.redScrapCost + ", ";
            row += item.greenScrapCost + ", ";
            row += item.blueScrapCost + ", ";
            //other
            row += item.allowHovering + ", ";
            row += item.applyDamageTypeToProjectile + ", ";
            row += item.arcShots + ", ";
            row += item.baseAttackMultiplier + ", ";
            row += item.baseDamageBonus + ", ";
            row += item.baseEnergyBonus + ", ";
            row += item.baseHealthBonus + ", ";
            row += item.baseShotSizeBonus + ", ";
            row += item.baseShotSpeedBonus + ", ";
            row += item.baseSpeedBonus + ", ";
            row += item.bonusNanobots + ", ";
            row += item.damageType + ", ";
            row += item.environmentalResistance + ", ";
            row += item.fireArc + ", ";
            row += item.follower + ", ";
            row += item.homing + ", ";
            row += item.homingArc + ", ";
            row += item.homingRadius + ", ";
            row += item.hoverMaxVelocity + ", ";
            row += item.hoverTime + ", ";
            row += item.ignoreTerrain + ", ";
            row += item.isActivatedItem + ", ";
            row += item.isEnergyWeapon + ", ";
            row += item.isTraversalItem + ", ";
            row += item.itemEnergyRegenRate + ", ";
            row += item.noSprite + ", ";
            row += item.penetrativeShot + ", ";
            row += item.pickUpRangeBonus + ", ";
            //row += item.projectileChildEffect.ToString()+ ", ";
            row += item.projectileStatusEffects.ToString() + ", ";
            row += item.projectileType + ", ";
            row += item.regenerationRate + ", ";
            row += item.rendersUseless + ", ";
            row += item.requiredAchievement + ", ";
            row += item.speedMod + ", ";
            //sprites
            row += item.decalOrder + ", ";
            row += item.armDecal + ", ";
            row += item.armSprite + ", ";
            row += item.headSprite + ", ";
            row += item.legDecal + ", ";
            row += item.legSprite + ", ";
            row += item.nontraversalPool + ", ";
            row += item.shoulderPadSprite + ", ";
            row += item.torsoDecal + ", ";
            row += item.torsoSprite + ", ";
            row += item.paletteOverride + ", ";

            stringBuilder.AppendLine(row);
        }

        string path = Application.isEditor ? Application.dataPath + "/../Layout Info/" : Application.dataPath + "/Layout Info/";
        var name = "ItemCounts.txt";
        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
        File.WriteAllText(path + name, stringBuilder.ToString());
        Debug.Log("Item CSV Finished!");
    }

    [MenuItem("Extensions/Item Info/List Achievement None Items")]
    static void ListStartingItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));

        var list = string.Empty;
        foreach (var item in loadedItemInfos)
        {
            if (item.requiredAchievement == AchievementID.None)
            {
                Debug.Log(item.type);
                list += item.type + ", ";
            }
        }

        Debug.Log(list);
    }

    [MenuItem("Extensions/Item Info/List Shrine Items")]
    static void ListShrineItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));

        var shrineDict = new Dictionary<ShrineType, List<string>>();
        var stringBuilder = new StringBuilder();

        foreach (var item in loadedItemInfos)
        {
            if (item.shrinePools != null)
            {
                foreach (var t in item.shrinePools)
                {
                    if (!shrineDict.ContainsKey(t)) { shrineDict[t] = new List<string>(); }
                    shrineDict[t].Add(item.name);
                }
            }
        }

        foreach (var kvp in shrineDict)
        {
            stringBuilder.AppendLine("==============");
            stringBuilder.AppendLine(kvp.Key.ToString());
            foreach (var name in kvp.Value)
            {
                stringBuilder.Append(name + ", ");
            }
            stringBuilder.AppendLine("==============");
        }

        var list = stringBuilder.ToString();
        Debug.Log(list);
    }

    [MenuItem("Extensions/Item Info/List Orb Smith Items")]
    static void ListOrbSmithItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));
        var sb = new StringBuilder();
        var total = 0;
        var premium = 0;
        var anyArchaic = 0;
        var red = 0;
        var green = 0;
        var blue = 0;

        foreach (var item in loadedItemInfos)
        {
            if (item.orbSmithPool)
            {
                int archaicScrapCount = 0;
                if (item.redScrapCost > 0)
                {
                    red += item.redScrapCost;
                    archaicScrapCount++;
                }

                if (item.greenScrapCost > 0)
                {
                    green += item.greenScrapCost;
                    archaicScrapCount++;
                }

                if (item.blueScrapCost > 0)
                {
                    blue += item.blueScrapCost;
                    archaicScrapCount++;
                }

                sb.AppendLine(item.type + ", s: " + item.grayScrapCost + " r: " + item.redScrapCost + " g: " + item.greenScrapCost + " b: " + item.blueScrapCost);
                total++;
                if (archaicScrapCount > 0) { anyArchaic++; }
                if (archaicScrapCount > 1) { premium++; }
            }
        }

        sb.AppendLine("total - red: " + red + " green: " + green + " blue: " + blue);
        sb.AppendLine("archaic: " + anyArchaic + "/" + total);
        sb.AppendLine("premium: " + premium + "/" + anyArchaic);
        Debug.Log(sb.ToString());
    }

    [MenuItem("Extensions/Item Info/List Shop Items")]
    static void ListShopItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));
        var sb = new StringBuilder();
        var total = 0;
        var premium = 0;
        var anyArchaic = 0;
        var red = 0;
        var green = 0;
        var blue = 0;

        foreach (var item in loadedItemInfos)
        {
            if (item.gunSmithPool || item.artificerPool || item.orbSmithPool)
            {
                int archaicScrapCount = 0;
                if (item.redScrapCost > 0)
                {
                    red += item.redScrapCost;
                    archaicScrapCount++;
                }

                if (item.greenScrapCost > 0)
                {
                    green += item.greenScrapCost;
                    archaicScrapCount++;
                }

                if (item.blueScrapCost > 0)
                {
                    blue += item.blueScrapCost;
                    archaicScrapCount++;
                }

                sb.AppendLine(item.type + ", s: " + item.grayScrapCost + " r: " + item.redScrapCost + " g: " + item.greenScrapCost + " b: " + item.blueScrapCost);
                total++;
                if (archaicScrapCount > 0) { anyArchaic++; }
                if (archaicScrapCount > 1) { premium++; }
            }
        }

        sb.AppendLine("total - red: " + red + " green: " + green + " blue: " + blue);
        sb.AppendLine("archaic: " + anyArchaic + "/" + total);
        sb.AppendLine("premium: " + premium + "/" + anyArchaic);
        Debug.Log(sb.ToString());
    }

    [MenuItem("Extensions/Item Info/List Gun Smith Items")]
    static void ListGunSmithItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));
        var sb = new StringBuilder();
        var total = 0;
        var premium = 0;
        var anyArchaic = 0;
        var red = 0;
        var green = 0;
        var blue = 0;

        foreach (var item in loadedItemInfos)
        {
            if (item.gunSmithPool)
            {
                int archaicScrapCount = 0;
                if (item.redScrapCost > 0)
                {
                    red += item.redScrapCost;
                    archaicScrapCount++;
                }

                if (item.greenScrapCost > 0)
                {
                    green += item.greenScrapCost;
                    archaicScrapCount++;
                }

                if (item.blueScrapCost > 0)
                {
                    blue += item.blueScrapCost;
                    archaicScrapCount++;
                }

                sb.AppendLine(item.type + ", s: " + item.grayScrapCost + " r: " + item.redScrapCost + " g: " + item.greenScrapCost + " b: " + item.blueScrapCost);
                total++;
                if (archaicScrapCount > 0) { anyArchaic++; }
                if (archaicScrapCount > 1) { premium++; }
            }
        }

        sb.AppendLine("total - red: " + red + " green: " + green + " blue: " + blue);
        sb.AppendLine("archaic: " + anyArchaic + "/" + total);
        sb.AppendLine("premium: " + premium + "/" + anyArchaic);
        Debug.Log(sb.ToString());
    }

    [MenuItem("Extensions/Item Info/List Artificer Items")]
    static void ListArtificerItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));
        var sb = new StringBuilder();
        var total = 0;
        var anyArchaic = 0;
        var premium = 0;
        var red = 0;
        var green = 0;
        var blue = 0;

        foreach (var item in loadedItemInfos)
        {
            if (item.artificerPool)
            {
                int archaicScrapCount = 0;

                if (item.redScrapCost > 0)
                {
                    red += item.redScrapCost;
                    archaicScrapCount++;
                }

                if (item.greenScrapCost > 0)
                {
                    green += item.greenScrapCost;
                    archaicScrapCount++;
                }

                if (item.blueScrapCost > 0)
                {
                    blue += item.blueScrapCost;
                    archaicScrapCount++;
                }

                sb.AppendLine(item.type + ", s: " + item.grayScrapCost + " r: " + item.redScrapCost + " g: " + item.greenScrapCost + " b: " + item.blueScrapCost);
                total++;
                if (archaicScrapCount > 0) { anyArchaic++; }
                if (archaicScrapCount > 1) { premium++; }
            }
        }

        sb.AppendLine("total - red: " + red + " green: " + green + " blue: " + blue);
        sb.AppendLine("archaic: " + anyArchaic + "/" + total);
        sb.AppendLine("premium: " + premium + "/" + anyArchaic);
        Debug.Log(sb.ToString());
    }

    [MenuItem("Extensions/Item Info/List Non-Traversal Achievement None Items")]
    static void ListNonTraversalNoAchievementItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));

        var list = string.Empty;
        foreach (var item in loadedItemInfos)
        {
            if (item.requiredAchievement == AchievementID.None && !item.isTraversalItem)
            {
                Debug.Log(item.type);
                list += item.type + ", ";
            }
        }

        Debug.Log(list);
    }

    [MenuItem("Extensions/Item Info/List Deathmatch Items")]
    static void ListDeathmatchItems()
    {
        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));

        var list = string.Empty;
        foreach (var item in loadedItemInfos)
        {
            if (item.deathmatch)
            {
                Debug.Log(item.type);
                list += item.type + ", ";
            }
        }

        Debug.Log(list);
    }

    [MenuItem("Extensions/Rotate 90 %&r")]
    static void Rotate90()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
        {
            var eulerRotation = t.rotation.eulerAngles;
            var eulerZ = (eulerRotation.z) + 90 % 360;
            t.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, eulerZ);
        }
    }

    [MenuItem("Extensions/Flip 180  &y")]
    static void Flip180Y()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
        {
            var eulerRotation = t.rotation.eulerAngles;
            var eulerX = (eulerRotation.x + 180) % 360;
            t.rotation = Quaternion.Euler(eulerX, eulerRotation.y, eulerRotation.z);
        }
    }

    [MenuItem("Extensions/Flip 180 &f")]
    static void Flip180()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
        {
            var eulerRotation = t.rotation.eulerAngles;
            t.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
        }
    }


    [MenuItem("Extensions/Show All Exits %#e")]
    static void ShowAllExits()
    {
        var room = FindObjectOfType<Room>();
        var exitToggles = room.GetComponentsInChildren<ExitObjectToggle>(true);
        foreach (var t in exitToggles)
        {
            t.exitMatch.SetActive(true);
            t.exitNotMatch.SetActive(false);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("Extensions/Hide All Exits %&e")]
    static void HideAllExits()
    {
        var room = FindObjectOfType<Room>();
        var exitToggles = room.GetComponentsInChildren<ExitObjectToggle>(true);
        foreach (var t in exitToggles)
        {
            t.exitMatch.SetActive(false);
            t.exitNotMatch.SetActive(true);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("Extensions/Set Door Sprites")]
    static void SetDoorSprites()
    {
        var doors = Resources.FindObjectsOfTypeAll<Door>();

        foreach (var door in doors)
        {
            Undo.RecordObject(door, "Setup Door");
            door.SetupDoor();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("Extensions/Remove Deleted Scenes From Build Settings")]
    static void RemoveDeletedScenes()
    {
        string[] files = Directory.GetFiles(Constants.roomDataPath, "*.txt");

        var existingScenes = new List<EditorBuildSettingsScene>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (File.Exists(scene.path))
            {
                existingScenes.Add(scene);
            }
            else
            {
                Debug.Log("Scene at " + scene.path + " no longer exits!");
            }
        }

        EditorBuildSettings.scenes = existingScenes.ToArray();

        foreach (var file in files)
        {
            string json = File.ReadAllText(file);
            var roomInfo = JsonConvert.DeserializeObject<RoomInfo>(json);
            if (!EditorBuildSettings.scenes.Any(r => Path.GetFileNameWithoutExtension(r.path) == roomInfo.sceneName))
            {
                Debug.Log(roomInfo.sceneName + " doesn't have a corresponding scene! Deleting!");
                File.Delete(file);
            }
        }
    }

    [MenuItem("Extensions/Export All Room Info")]
    static void ExportAllRoomInfo()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(buildScene.path);
            var scene = EditorSceneManager.GetSceneByPath(buildScene.path);
            var room = FindObjectOfType<Room>();
            progress++;

            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);

            if (room)
            {
                room.SaveRoomInfo();
                EditorSceneManager.SaveScene(scene);
            }

            EditorSceneManager.CloseScene(scene, true);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/GetEnemyCounts")]
    static void GetEnemyCounts()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        Dictionary<string, int> enemyCounts = new Dictionary<string, int>();
        Dictionary<EnvironmentType, Dictionary<string, int>> envEnemyCounts = new Dictionary<EnvironmentType, Dictionary<string, int>>();

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            try
            {
                EditorSceneManager.OpenScene(buildScene.path);
                var scene = EditorSceneManager.GetSceneByPath(buildScene.path);
                var room = FindObjectOfType<Room>();
                if (room)
                {
                    var env = room.roomInfo.environmentType;
                    if (!envEnemyCounts.ContainsKey(env))
                    {
                        envEnemyCounts[env] = new Dictionary<string, int>();
                    }

                    var enemies = room.GetComponentsInChildren<Enemy>(true);
                    var spawns = room.GetComponentsInChildren<MonsterSpawnPoint>(true); ;

                    foreach (var e in enemies)
                    {
                        var key = e.name;
                        if (key.Contains("FleshLump")) { continue; }

                        for (int i = 0; i < 30; i++) { key = key.Replace("(" + i + ")", string.Empty); }
                        key = key.Replace(" ", string.Empty);

                        if (!enemyCounts.ContainsKey(key)) { enemyCounts.Add(key, 0); }
                        enemyCounts[key]++;
                        if (!envEnemyCounts[env].ContainsKey(key)) { envEnemyCounts[env].Add(key, 0); }
                        envEnemyCounts[env][key]++;
                    }

                    foreach (var s in spawns)
                    {
                        foreach (var p in s.spawns)
                        {
                            var key = p.prefab.name;
                            if (key.Contains("FleshLump")) { continue; }

                            for (int i = 0; i < 30; i++) { key = key.Replace("(" + i + ")", string.Empty); }
                            key = key.Replace(" ", string.Empty);

                            if (!enemyCounts.ContainsKey(key)) { enemyCounts.Add(key, 0); }
                            enemyCounts[key]++;
                            if (!envEnemyCounts[env].ContainsKey(key)) { envEnemyCounts[env].Add(key, 0); }
                            envEnemyCounts[env][key]++;
                        }
                    }
                }

                EditorSceneManager.CloseScene(scene, true);
            }
            catch (Exception exc)
            {
                Debug.Log(exc.Message);
            }

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);
        }

        var builder = new StringBuilder();

        foreach (var kvp in envEnemyCounts)
        {
            var env = kvp.Key;
            builder.AppendLine("===" + env + "===");
            var sorted = kvp.Value.ToList();
            sorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            sorted.Reverse();
            foreach (var kvp2 in sorted)
            {
                var lead = "*";
                var enemyName = kvp2.Key;
                foreach (var kvp3 in envEnemyCounts)
                {
                    var e = kvp.Key;
                    if (e == env || e == EnvironmentType.Surface || e == EnvironmentType.ForestSlums) { continue; }
                    if (kvp3.Value.ContainsKey(enemyName))
                    {
                        lead = string.Empty;
                        break;
                    }
                }
                builder.AppendLine(lead + enemyName + ": " + kvp2.Value);
            }
        }

        builder.AppendLine("===TOTAL===");
        var sortedEnemyCounts = enemyCounts.ToList();
        sortedEnemyCounts.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        sortedEnemyCounts.Reverse();
        foreach (var kvp in sortedEnemyCounts)
        {
            builder.AppendLine(kvp.Key + ": " + kvp.Value);
        }

        string path = Application.isEditor ? Application.dataPath + "/../Layout Info/" : Application.dataPath + "/Layout Info/";
        var name = "EnemyCounts.txt";
        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
        File.WriteAllText(path + name, builder.ToString());

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/Destroy Transition Fades")]
    static void DestroyTransitionFades()
    {
        bool dirty = false;
        var tilemaps = Resources.FindObjectsOfTypeAll<TilemapTransitionFade>();
        for (int i = 0; i < tilemaps.Length; i++)
        {
            dirty = true;
            Debug.Log("Destroying TilemapTransitionFade on " + tilemaps[i].gameObject.name);
            DestroyImmediate(tilemaps[i]);
        }

        var spriteGroups = Resources.FindObjectsOfTypeAll<SpriteRendererGroupTransitionFade>();
        for (int i = 0; i < spriteGroups.Length; i++)
        {
            dirty = true;
            Debug.Log("Destroying SpriteRendererGroupTransitionFade on " + spriteGroups[i].gameObject.name);
            DestroyImmediate(spriteGroups[i]);
        }

        var sprites = Resources.FindObjectsOfTypeAll<SpriteRendererTransitionFade>();
        for (int i = 0; i < sprites.Length; i++)
        {
            dirty = true;
            Debug.Log("Destroying SpriteRendererTransitionFade on " + sprites[i].gameObject.name);
            DestroyImmediate(sprites[i]);
        }

        if (dirty)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }

    [MenuItem("Extensions/Tilemaps/Assign Tileset")]
    static void TilemapAssignTileset()
    {
        bool dirty = false;
        var room = FindObjectOfType<Room>();
        if (room)
        {
            var env = room.roomInfo.environmentType.ToString();
            var path = "Assets/Tilesets/" + env + "/" + env + ".asset";
            var tileset = AssetDatabase.LoadAssetAtPath<Tileset>(path);

            if (!tileset)
            {
                Debug.LogWarning("Could not find tileset at " + path);
                return;
            }

            var validNames = new string[] { "Cave", "Factory", "BuriedCity", "BeastGuts", "Surface", "ForestSlums", "GreyBox", "CoolantSewers", };
            var tilemaps = Resources.FindObjectsOfTypeAll<STETilemap>();
            foreach (var tilemap in tilemaps)
            {
                if (validNames.Contains(tilemap.Tileset.name))
                {
                    tilemap.Tileset = tileset;
                    Debug.Log("Setting " + tilemap.name + "'s tileset to " + tileset.name);
                }
            }

            if (dirty)
            {
                EditorSceneManager.MarkAllScenesDirty();
            }
        }
        else
        {
            Debug.LogWarning("Couldn't find object of type room.");
        }
    }

    [MenuItem("Extensions/New Environment")]
    static void NewEnvironement()
    {
        var greyBox = EnvironmentType.GreyBox;
        var newEnvironment = EnvironmentType.CrystalMines;
        float progress = 0f;

        var paths = Directory.GetFiles(Application.dataPath + "/Scenes/Rooms/" + greyBox.ToString() + "/", "*.unity", SearchOption.AllDirectories);

        foreach (var path in paths)
        {
            var newPath = path.Replace(greyBox.ToString(), newEnvironment.ToString());
            var dir = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(path, newPath, true);

            EditorSceneManager.OpenScene(newPath);
            var scene = EditorSceneManager.GetActiveScene();
            var room = FindObjectOfType<Room>();

            if (room)
            {
                room.roomInfo.environmentType = newEnvironment;
                room.name = room.name.Replace(greyBox.ToString(), newEnvironment.ToString());
                room.roomInfo.sceneName = room.name;

                TilemapAssignTileset();
                //AssignEnvMaterial();

                Undo.RegisterFullObjectHierarchyUndo(room, "undo room changes");

                progress++;

                EditorUtility.DisplayProgressBar("Progress", progress + "//" + paths.Length, progress / paths.Length);

                if (EditorSceneManager.SaveScene(scene))
                {
                    if (!room.name.Contains("Template"))
                    {
                        room.SaveRoomInfo();
                    }

                    var scenePaths = new List<string>();
                    foreach (var s in EditorBuildSettings.scenes)
                    {
                        var scenePath = s.path;
                        scenePaths.Add(scenePath);
                    }

                    if (!scenePaths.Contains(scene.path))
                    {
                        Debug.Log("Adding Scene to BuildSettings");
                        var original = EditorBuildSettings.scenes;
                        var newSettings = new EditorBuildSettingsScene[original.Length + 1];
                        System.Array.Copy(original, newSettings, original.Length);
                        newSettings[newSettings.Length - 1] = new EditorBuildSettingsScene(scene.path, true);
                        EditorBuildSettings.scenes = newSettings;
                    }
                }
                else
                {
                    Debug.LogError("Couldn't save " + scene.name);
                }
            }

            EditorSceneManager.CloseScene(scene, true);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/SetAllRoomInfoTravLimJumpHeightsTo6")]
    static void SetAllRoomInfoTravLimJumpHeightsTo6()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(buildScene.path);
            var scene = EditorSceneManager.GetSceneByPath(buildScene.path);
            var room = FindObjectOfType<Room>();
            if (room)
            {
                if (!room.roomInfo.sceneName.Contains("Water"))
                {
                    room.roomInfo.traversalLimitations.requiredJumpHeight = 6;
                    room.SaveRoomInfo();
                    EditorSceneManager.MarkAllScenesDirty();
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/SetPermanentStateObjects")]
    static void SetPermanentStateObjects()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        string[] nonRoomScenes = new string[] { "SpookyEnding", "SplashScreenWait", "NewGame", "Congratulations", "EndScreen01", "EndScreen02", "EndScreen03" };
        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(buildScene.path);

            var scene = EditorSceneManager.GetSceneByPath(buildScene.path);

            if (nonRoomScenes.Contains(scene.name)) { continue; }

            var room = FindObjectOfType<Room>();
            if (room)
            {
                if (room.roomInfo.permanentStateObjectCount > 0) { continue; }
                var permanentStateObjects = room.GetComponentsInChildren<PermanentStateObject>();

                if (permanentStateObjects.Length > 0)
                {
                    Debug.Log("Setting " + room.name + " permanent state objects");
                    room.SetPermanentStateObjects();
                    room.SaveRoomInfo();
                    EditorSceneManager.MarkAllScenesDirty();
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/GiveCoolantSewerIceReplaceScript")]
    static void GiveCoolantSewerIceReplaceScript()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        List<STETilemap> tilemaps = new List<STETilemap>();
        var toReplacePath = "Assets/Tilesets/CoolantSewers/CoolantSewerIceHolderBrush.asset";
        var replaceWithPath = "Assets/Tilesets/CoolantSewers/CoolantSewerIceReplaceBrush.asset";
        var toReplace = AssetDatabase.LoadAssetAtPath<RandomBrush>(toReplacePath);
        var replaceWith = AssetDatabase.LoadAssetAtPath<RandomBrush>(replaceWithPath);

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.path.Contains("CoolantSewers")) continue;

            EditorSceneManager.OpenScene(buildScene.path);
            var scene = EditorSceneManager.GetSceneByPath(buildScene.path);
            var room = FindObjectOfType<Room>();
            string info = "skipping " + room.name;
            bool needSave = false;

            if (room)
            {
                room.GetComponentsInChildren<STETilemap>(true, tilemaps);
                foreach (var t in tilemaps)
                {
                    if (t.Tileset != toReplace.Tileset) continue;
                    if (t.GetComponent<ReplaceTilesIfEnvEffect>()) continue;

                    info = "Checking " + t.name + " in " + room.name;
                    bool needsScript = false;
                    for (int x = t.MinGridX; x <= t.MaxGridX; x++)
                    {
                        for (int y = t.MinGridY; y <= t.MaxGridY; y++)
                        {
                            int tileId = (int)(t.GetTileData(x, y) & Tileset.k_TileDataMask_TileId);
                            if (toReplace.RandomTileList.Any((tile) => tile.tileData == tileId))
                            {
                                needsScript = true;
                                break;
                            }
                        }
                        if (needsScript) break;
                    }

                    if (needsScript)
                    {
                        Debug.Log("Adding ReplaceTilesIfEnvEffect to " + t.name + " in " + room.name);
                        var r = t.gameObject.AddComponent<ReplaceTilesIfEnvEffect>();
                        r.tilemap = t;
                        r.toReplace = toReplace;
                        r.replaceWith = replaceWith;
                        r.envEffect = EnvironmentalEffect.Heat;
                        needSave = true;
                    }

                    EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount + " " + info, progress / sceneCount);
                }

                if (needSave)
                {
                    EditorSceneManager.MarkAllScenesDirty();
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount + " " + info, progress / sceneCount);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/CheckForExitPermutations")]
    static void CheckForExitPermutations()
    {
        var files = Resources.LoadAll<TextAsset>("RoomData");

        Dictionary<EnvironmentType, Dictionary<Direction, bool>> phaseWallExits = new Dictionary<EnvironmentType, Dictionary<Direction, bool>>();
        Dictionary<EnvironmentType, Dictionary<Direction, bool>> phaseProofSmallGapExits = new Dictionary<EnvironmentType, Dictionary<Direction, bool>>();

        foreach (var file in files)
        {
            var roomInfo = JsonConvert.DeserializeObject<RoomInfo>(file.text);
            if (roomInfo.environmentType == EnvironmentType.BeastGuts ||
                roomInfo.environmentType == EnvironmentType.Glitch)
            {
                continue;
            }

            if (!phaseWallExits.ContainsKey(roomInfo.environmentType))
            {
                phaseWallExits.Add(roomInfo.environmentType, new Dictionary<Direction, bool>());
            }
            var phaseDict = phaseWallExits[roomInfo.environmentType];


            if (!phaseProofSmallGapExits.ContainsKey(roomInfo.environmentType))
            {
                phaseProofSmallGapExits.Add(roomInfo.environmentType, new Dictionary<Direction, bool>());
            }
            var phaseProofSmallGapDict = phaseProofSmallGapExits[roomInfo.environmentType];

            foreach (var e in roomInfo.possibleExits)
            {
                if (!phaseDict.ContainsKey(e.direction)) { phaseDict[e.direction] = false; }
                if (!phaseDict[e.direction] && e.toExit.supportsPhaseThroughWalls)
                {
                    phaseDict[e.direction] = true;
                }

                if (!phaseProofSmallGapDict.ContainsKey(e.direction)) { phaseProofSmallGapDict[e.direction] = false; }
                if (!phaseProofSmallGapDict[e.direction] && e.toExit.supportsGroundedSmallGaps && e.toExit.phaseProof)
                {
                    phaseProofSmallGapDict[e.direction] = true;
                }
            }
        }

        foreach (var kvp in phaseWallExits)
        {
            foreach (var kvp2 in kvp.Value)
            {
                if (!kvp2.Value)
                {
                    Debug.Log(kvp.Key + " is missing a phase wall for direction " + kvp2.Key);
                }
            }
        }

        foreach (var kvp in phaseProofSmallGapExits)
        {
            foreach (var kvp2 in kvp.Value)
            {
                if (!kvp2.Value)
                {
                    Debug.Log(kvp.Key + " is missing a phase proof small for direction " + kvp2.Key);
                }
            }
        }
    }

    [MenuItem("Extensions/CheckRoomsForErrors")]
    static void CheckRoomsForErrors()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        var cLayer = LayerMask.NameToLayer("Caulk");
        var ecLayer = LayerMask.NameToLayer("EnemyCaulkOnly");

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(buildScene.path);
            var room = FindObjectOfType<Room>();
            if (room)
            {
                if (room.roomInfo.traversalLimitations.requiredJumpHeight <= 0)
                {
                    Debug.Log(room.roomInfo.sceneName + " has requiredJumpHeight of 0");
                }

                var caulk = false;
                for (int i = 0; i < room.transform.childCount; i++)
                {
                    var child = room.transform.GetChild(i);
                    if (child.gameObject.layer == cLayer || child.gameObject.layer == ecLayer) caulk = true;
                }

                if (!caulk)
                {
                    Debug.Log(room.roomInfo.sceneName + " is missing Caulk");
                }
            }

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/AddDoorCaulk")]
    static bool AddDoorCaulk()
    {
        bool dirty = false;

        var room = Resources.FindObjectsOfTypeAll<Room>().First();
        if (room)
        {
            ShowAllExits();
            var triggers = room.GetComponentsInChildren<RoomTransitionTrigger>();
            foreach (var trigger in triggers)
            {
                var trigY = trigger.transform.localPosition.y;
                var door = trigger.GetComponentInChildren<Door>();
                if (!door) continue;
                var caulkLayer = LayerMask.NameToLayer("Caulk");
                bool hasCaulk = false;
                foreach (Transform child in door.transform)
                {
                    if (child.gameObject.layer == caulkLayer) { hasCaulk = true; }
                }
                if (hasCaulk) continue;

                var offset = Mathf.Abs(trigY - (-6.5f));
                if (trigger.direction == Direction.Down && offset != 0)
                {
                    dirty = true;
                    var caulk = new GameObject("Caulk", typeof(BoxCollider2D));
                    caulk.layer = caulkLayer;
                    caulk.transform.parent = door.transform;
                    caulk.transform.localScale = new Vector3(4, offset, 1);
                    caulk.transform.localPosition = new Vector3(0, -0.5f + (offset / -2), 0);
                    Debug.Log("Added Caulk to Door " + trigger.direction + " " + trigger.localGridPosition + " in room " + room.name);
                }

                offset = Mathf.Abs(trigY - 6.5f);
                if (trigger.direction == Direction.Up && offset != 0)
                {
                    dirty = true;
                    var caulk = new GameObject("Caulk", typeof(BoxCollider2D));
                    caulk.layer = caulkLayer;
                    caulk.transform.parent = door.transform;
                    caulk.transform.localScale = new Vector3(4, offset, 1);
                    caulk.transform.localPosition = new Vector3(0, 0.5f + (offset / 2), 0);
                    Debug.Log("Added Caulk to Door " + trigger.direction + " " + trigger.localGridPosition + " in room " + room.name);
                }
            }
        }

        if (dirty)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        return dirty;
    }

    [MenuItem("Extensions/AddDoorCaulkAllRooms")]
    static void AddDoorCaulkAllRooms()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        var cLayer = LayerMask.NameToLayer("Caulk");
        var ecLayer = LayerMask.NameToLayer("EnemyCaulkOnly");

        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(buildScene.path);
            var scene = EditorSceneManager.GetSceneByPath(buildScene.path);

            var room = FindObjectOfType<Room>();
            if (room)
            {
                if (AddDoorCaulk())
                {
                    EditorSceneManager.MarkAllScenesDirty();
                    EditorSceneManager.SaveScene(scene);
                }
            }

            EditorSceneManager.CloseScene(scene, true);

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Extensions/Get List of Room Linked Achievements")]
    static void RoomAchievements()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("RoomData");
        HashSet<AchievementID> achvs = new HashSet<AchievementID>();
        foreach (var file in files)
        {
            var roomInfo = JsonConvert.DeserializeObject<RoomInfo>(file.text);
            if(roomInfo.requiredAchievements != null)
            {
                foreach (var a in roomInfo.requiredAchievements) { achvs.Add(a); }
            }
        }

        foreach (var a in achvs)
        {
            Debug.Log(a);
        }
    }

    [MenuItem("Extensions/Get List of Enemies Without Champions Linked")]
    static void EnemyNoChamp()
    {
        var assets = PrefabLoader.LoadAllPrefabsOfType<MonsterSpawnPoint>("Assets/Prefabs/SpawnPoints/", true);
        var sb = new StringBuilder();
        foreach (var asset in assets)
        {
            foreach (var s in asset.spawns)
            {
                if(!s.prefab)
                {
                    sb.AppendLine("Null prefab in " + asset.name);
                }
                else if (s.championVariants.Length == 0)
                {
                    sb.AppendLine("No Champions for " + s.prefab.name + " in " + asset.name);
                }
            }
        }

        Debug.Log(sb.ToString());
    }

    [MenuItem("Extensions/FindBadTutShrines")]
    static void FindBadTutShrines()
    {
        float sceneCount = EditorBuildSettings.scenes.Length;
        float progress = 0f;

        foreach (var buildScene in EditorBuildSettings.scenes)
        {            
            EditorSceneManager.OpenScene(buildScene.path);
            var scene = EditorSceneManager.GetSceneByPath(buildScene.path);

            var room = FindObjectOfType<Room>();
            if (room)
            {
                var tutShrines = room.GetComponentsInChildren<MinorItemChanger>().ToArray();
                if (tutShrines.Length > 0 && room.roomInfo.permanentStateObjectCount != tutShrines.Length) { Debug.Log(scene.name); }
            }

            EditorSceneManager.CloseScene(scene, true);

            progress++;
            EditorUtility.DisplayProgressBar("Progress", progress + "//" + sceneCount, progress / sceneCount);
        }

        EditorUtility.ClearProgressBar();
    }
}

