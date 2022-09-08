using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateMenu :MonoBehaviour
{
    [MenuItem("GameObject/Items/MinorItemToggle", false, 0)]
    public static void MinorItemObjectToggle(MenuCommand menuCommand)
    {
        var go = new GameObject("MinorItemToggle");
        var toggle = go.AddComponent<MinorItemObjectToggle>();
        var minorItem = new GameObject("MinorItemExists");
        minorItem.transform.SetParent(go.transform);
        minorItem.transform.localPosition = Vector2.zero;
        var noMinorItem = new GameObject("NoMinorItem");
        noMinorItem.transform.SetParent(go.transform);
        noMinorItem.transform.localPosition = Vector2.zero;
        toggle.minorItemExists = minorItem;
        toggle.minorItemDoesNotExists = noMinorItem;
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/Items/MinorItemBlock", false, 0)]
    public static void CreateMinorItemBlock(MenuCommand menuCommand)
    {
        var go = new GameObject("MinorItemBlock");
        var toggle = go.AddComponent<MinorItemObjectToggle>();
        var minorItem = new GameObject("MinorItemExists");
        minorItem.transform.parent = go.transform;
        minorItem.transform.localPosition = Vector2.zero;
        toggle.minorItemExists = minorItem;
        var prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ItemSpawns/MinorItemSpawnPoint.prefab", typeof(MinorItemSpawnPoint));
        var spawnPoint = PrefabUtility.InstantiatePrefab(prefab) as MinorItemSpawnPoint;
        spawnPoint.transform.parent = minorItem.transform;
        spawnPoint.transform.localPosition = Vector2.zero;

        prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/TilePrefabs/DestructibleGenericTile.prefab", typeof(DestructibleTileBounds));
        var destructable = PrefabUtility.InstantiatePrefab(prefab) as DestructibleTileBounds;
        destructable.transform.parent = minorItem.transform;
        destructable.transform.localPosition = Vector2.zero;

        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

        //this fixes where the layers get set to default
        PrefabUtility.RevertPrefabInstance(spawnPoint.gameObject, InteractionMode.AutomatedAction);
        PrefabUtility.RevertPrefabInstance(destructable.gameObject, InteractionMode.AutomatedAction);

        spawnPoint.linkedDestructable = destructable;

        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/TheFleshening", false, 0)]
    public static void TheFleshening(MenuCommand menuCommand)
    {
        var go = new GameObject("TheFleshening");
        go.AddComponent<TheFleshening>();        
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/Enemies/EnemyGroups", false, 0)]
    public static void EnemyGroups(MenuCommand menuCommand)
    {
        var go = new GameObject("Enemies");
        go.AddComponent<SpawnOneChild>();
        var group1 = new GameObject("Group1");
        group1.transform.SetParent(go.transform);
        group1.transform.localPosition = Vector2.zero;
        var group2 = new GameObject("Group2");
        group2.transform.SetParent(go.transform);
        group2.transform.localPosition = Vector2.zero;
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/Enemies/EnemyMulti", false, 0)]
    public static void EnemyMulti(MenuCommand menuCommand)
    {
        var go = new GameObject("Enemies");
        go.AddComponent<SpawnMultipleChildren>();
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/FleshLump/FleshLump1x1", false, 0)]
    public static void FleshLump1x1(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump1x1", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/FleshLump1x3", false, 0)]
    public static void FleshLump1x3(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump1x3", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/FleshLump1x4", false, 0)]
    public static void FleshLump1x4(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump1x4", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/FleshLump2x1", false, 0)]
    public static void FleshLump2x1(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump2x1", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/FleshLump2x2", false, 0)]
    public static void FleshLump2x2(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump2x2", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/FleshLump2x2Corner", false, 0)]
    public static void FleshLump2x2Corner(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump2x2Corner", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/FleshLump3x1", false, 0)]
    public static void FleshLump3x1(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/FleshLump3x1", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump1x1", false, 0)]
    public static void PaleFleshLump1x1(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump1x1", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump1x3", false, 0)]
    public static void PaleFleshLump1x3(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump1x3", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump1x4", false, 0)]
    public static void PaleFleshLump1x4(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump1x4", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump2x1", false, 0)]
    public static void PaleFleshLump2x1(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump2x1", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump2x2", false, 0)]
    public static void PaleFleshLump2x2(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump2x2", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump2x2Corner", false, 0)]
    public static void PaleFleshLump2x2Corner(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump2x2Corner", menuCommand);
    }

    [MenuItem("GameObject/FleshLump/PaleFleshLump3x1", false, 0)]
    public static void PaleFleshLump3x1(MenuCommand menuCommand)
    {
        InstantiateEnemy("FleshLumps/PaleFleshLump3x1", menuCommand);
    }
    
    public static void InstantiateEnemy(string enemy, MenuCommand menuCommand)
    {
        var prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Enemies/" + enemy + ".prefab", typeof(GameObject));

        if (prefab)
        {
            Debug.Log("Instantiating " + enemy);
            var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            var parentObject = menuCommand.context as GameObject;
            if(parentObject)
            {
                go.transform.SetParent(parentObject.transform, true);
            }
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        else
        {
            Debug.Log("Could not instantiate " + enemy);
        }
    }
}
