using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using CreativeSpore.SuperTilemapEditor;

public class Room : MonoBehaviour
{
    public static Dictionary<string, Tileset> altSets = new Dictionary<string, Tileset>();
    public RoomInfo roomInfo;
    public XorShift random;
    public bool lockDownRoom;
    public bool showBounds;

    private bool _hasFadeAways;
    public bool hasFadeAways { get { return _hasFadeAways; } }

    public Bounds worldBounds
    {
        get
        {
            Bounds wb = new Bounds();
            var min = transform.position;
            min.x -= Constants.roomWidth * 0.5f;
            min.y -= Constants.roomHeight * 0.5f + (roomInfo.size.y - 1) * Constants.roomHeight;
            var max = transform.position;
            max.x += Constants.roomWidth * 0.5f + (roomInfo.size.x - 1) * Constants.roomWidth;
            max.y += Constants.roomHeight * 0.5f;

            wb.SetMinMax(min, max);
            return wb;
        }
    }

    private RoomAbstract _roomAbstract;
    public RoomAbstract roomAbstract
    {
        get
        {
            return _roomAbstract;
        }
    }

    [HideInInspector]
    public RoomTransitionTrigger[] transitionTriggers;

    private GravStarPathFinder _gravityPathFinder;
    public GravStarPathFinder gravityPathFinder
    {
        get
        {
            if (_gravityPathFinder == null)
            {                
                _gravityPathFinder = gameObject.AddComponent<GravStarPathFinder>();
                _gravityPathFinder.tilemaps = GetComponentsInChildren<STETilemap>(false).Where(t => t.ColliderType == eColliderType._2D).ToList();
                _gravityPathFinder.sensitives = new HashSet<IPathFindingSensitive>(gameObject.GetInterfacesInChildren<IPathFindingSensitive>().Where(s => s.pathFindingSensitive));
            }

            return _gravityPathFinder;
        }
    }

    private GridStarPathFinder _gridPathFinder;
    public GridStarPathFinder gridPathFinder
    {
        get
        {
            if (_gridPathFinder == null)
            {
                _gridPathFinder = gameObject.AddComponent<GridStarPathFinder>();
                _gridPathFinder.tilemaps = GetComponentsInChildren<STETilemap>(false).Where(t => t.ColliderType == eColliderType._2D).ToList();
                _gridPathFinder.sensitives = new HashSet<IPathFindingSensitive>(gameObject.GetInterfacesInChildren<IPathFindingSensitive>().Where(s => s.pathFindingSensitive));
            }

            return _gridPathFinder;
        }
    }

    public void Awake()
    {
        if (LayoutManager.instance)
        {
            LayoutManager.instance.RegisterRoom(this);
        }
        else
        {
            transitionTriggers = GetComponentsInChildren<RoomTransitionTrigger>();
        }
    }

    public IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (!LayoutManager.instance)
        {
            random = new XorShift((int)(UnityEngine.Random.value * int.MaxValue));
            var playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
            if (playerStart)
            {
                Player.instance.transform.position = playerStart.transform.position;
            }
        }

        var activeGame = SaveGameManager.activeGame;
        var exterminator = (activeGame != null && activeGame.gameMode == GameMode.Exterminator);
        var bossRush = (activeGame != null && activeGame.gameMode == GameMode.BossRush);
        if (exterminator || bossRush)
        {
            for (int i = 0; i < 2; i++) { yield return null; }
            //Give flesh lumps and other enemies a chance to be destroyed

            if (roomInfo.roomType == RoomType.StartingRoom || 
                (bossRush && roomInfo.roomType == RoomType.None))
            {
                EnemyManager.instance.DestroyAllEnemies();
                var other = GetComponentsInChildren<MonsterSpawnPointTriggered>();
                foreach (var sp in other) { Destroy(sp.gameObject); }
            }
        }

        if ((lockDownRoom || exterminator) && roomInfo.roomType != RoomType.MegaBeast)
        {
            var lockDown = new GameObject("LockDownFight").AddComponent<LockDownFight>();
            lockDown.transform.parent = transform;
            lockDown.enabled = false;

            yield return null;

            var enemy = GetComponentInChildren<Enemy>();
            var spawn = GetComponentInChildren<MonsterSpawnPoint>();
            var spawnT = GetComponentInChildren<MonsterSpawnPointTriggered>();

            if (enemy || spawn || spawnT)
            {
                lockDown.enabled = true;
            }
            else
            {
                Destroy(lockDown.gameObject);
            }
        }
    }

    public void AssignAbstract(RoomAbstract roomAbstract)
    {
        _roomAbstract = roomAbstract;
        random = new XorShift(_roomAbstract.seed);
        //Debug.Log(name + " created its random with the seed " + _roomAbstract.seed);
        transform.position = _roomAbstract.worldPosition;
        var abstractDependantObjects = gameObject.GetInterfacesInChildren<IAbstractDependantObject>(true);
        var sorted = abstractDependantObjects.OrderByDescending(a => a.m_priority);
        foreach (var o in sorted)
        {
            if (o != null && (o as MonoBehaviour)) //o could've been destroyed if its parent was
            {
                o.CompareWithAbstract(_roomAbstract);
            }
        }

        transitionTriggers = GetComponentsInChildren<RoomTransitionTrigger>();
        ExitAbstract exitAbstract;
        foreach (var triggers in transitionTriggers)
        {
            exitAbstract = _roomAbstract.exits.Find(e => e.localGridPosition == triggers.localGridPosition && e.direction == triggers.direction);
            if (exitAbstract != null)
            {
                if (!triggers.AssignExitAbstract(exitAbstract))
                {
                    Debug.LogError("Error assigning exit abstract in room " + _roomAbstract.assignedRoomInfo.sceneName + " at " + _roomAbstract.gridPosition);
                }
            }
            else
            {
                Destroy(triggers.gameObject);
            }
        }

        bool modifyTilemaps = _roomAbstract.useAltTileset || _roomAbstract.environmentalEffect != 0 || _roomAbstract.altPalette > 0;

        var envEffectName = _roomAbstract.environmentalEffect.ToString();

        if (_roomAbstract.environmentalEffect == EnvironmentalEffect.Underwater)
        {
            var water = GetComponentsInChildren<Water>();
            foreach (var w in water)
            {
                Destroy(w.gameObject);
            }

            var underwater = Instantiate(Resources.Load<Water>("EnvironmentalEffects/UnderwaterEffect"), transform, false);
            Vector2 size = roomAbstract.assignedRoomInfo.size.Vector2();
            underwater.transform.localScale = Vector3.one;
            var position = Vector2.zero;
            position.x += (size.x-1) * 12;
            position.y += (size.y-1) * -7;
            underwater.transform.localPosition = position;
            size.x *= 24;
            size.y *= 14;
            size.y += 1;
            underwater.SetSize(size);
        }
        else if (_roomAbstract.environmentalEffect != 0)
        {
            var envEffectPrefab = ResourcePrefabManager.instance.LoadGameObject("EnvironmentalEffects/" + envEffectName + "Effect");
            if (envEffectPrefab != null)
            {
                var envEffect = Instantiate(envEffectPrefab, transform, false);
                envEffect.transform.localScale = Vector3.one;
                envEffect.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("No Environmental Effect Prefab Could Be Found For " + envEffectName);
            }
        }

        var lightColor = Color.white;
        if(SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Spooky)
        {
            _roomAbstract.light = 0.4f;
            lightColor = new Color(0.7f,0,0.1f);
        }
        
        if (_roomAbstract.light != 1 && _roomAbstract.environmentalEffect == EnvironmentalEffect.None || _roomAbstract.environmentalEffect == EnvironmentalEffect.Fog)
        {
            var envEffectPrefab = ResourcePrefabManager.instance.LoadGameObject("EnvironmentalEffects/DarknessEffect");
            if (envEffectPrefab != null)
            {
                var envEffect = Instantiate(envEffectPrefab, transform, false);
                envEffect.transform.localScale = Vector3.one;
                envEffect.transform.localPosition = Vector3.zero;
                var darkness = envEffect.GetComponent<Darkness>();
                darkness.SetLighting(_roomAbstract.light, lightColor);

                var godRays = roomInfo.roomType == RoomType.ItemRoom;
                if (godRays)
                {
                    envEffectPrefab = ResourcePrefabManager.instance.LoadGameObject("EnvironmentalEffects/GodRays");
                    if (envEffectPrefab != null)
                    {
                        envEffect = Instantiate(envEffectPrefab, transform, false);
                        envEffect.transform.localScale = Vector3.one;
                        envEffect.transform.localPosition = Vector3.zero;
                        darkness.SetLighting(_roomAbstract.light - 0.2f, lightColor);
                    }
                }
            }
        }

        if (modifyTilemaps)
        {
            ModifyTilemaps(_roomAbstract.environmentalEffect, _roomAbstract.useAltTileset, _roomAbstract.altPalette);
        }

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            List<LooseItemData> looseItems;
            if(activeGame.looseItems.TryGetValue(_roomAbstract.roomID, out looseItems))
            {
                foreach (var item in looseItems)
                {
                    Instantiate(Resources.Load<ActivatedItemPickUp>("MajorItemPickUps/" + item.item.ToString()), item.position, Quaternion.identity, transform);
                }
            }

            if (!activeGame.roomsVisited.Contains(_roomAbstract.roomID))
            {
                activeGame.roomsVisited.Add(_roomAbstract.roomID);
                SaveGameManager.instance.Save();
            }
        }

        _hasFadeAways = GetComponentInChildren<FadeAwayTilemap>();
        Physics2D.SyncTransforms();
    }

    public void ModifyTilemaps(EnvironmentalEffect environmentalEffect, bool useAltTileset, int altPalette)
    {
        var envEffectName = environmentalEffect.ToString();
        var validTileSets = new string[] { "Tentacles", "Surface", "SurfaceIndoorBG", "ForestSlums", "ForestSlumsBG", "CoolantSewers", "Cave", "CaveBG", "Factory", "BuriedCity", "BuriedCityBG" };
        var tilemaps = GetComponentsInChildren<STETilemap>(true);
        foreach (var tilemap in tilemaps)
        {
            var setName = tilemap.Tileset.name;
            if (validTileSets.Contains(setName))
            {
                var isBackground = setName.Contains("BG");

                if (useAltTileset)
                {
                    var altSetPath = "Tilesets/" + setName + "Alt";

                    Tileset altSet = null;
                    if (!altSets.TryGetValue(altSetPath, out altSet))
                    {
                        var altAtlas = Resources.Load<Texture2D>(altSetPath);
                        if (altAtlas != null)
                        {
                            altSet = Instantiate(tilemap.Tileset);
                            altSet.AtlasTexture = altAtlas;
                            altSets[altSetPath] = altSet;
                        }
                        else
                        {
                            altSets[altSetPath] = null;
                        }
                    }

                    if (altSet)
                    {
                        tilemap.Tileset = altSet;
                    }
                    else
                    {
                        //Debug.LogWarning("Could not find " + altSetPath);
                    }
                }

                if (altPalette > 0 && environmentalEffect != EnvironmentalEffect.Heat)
                {
                    tilemap.Material = ResourcePrefabManager.instance.LoadMaterial("Materials/" + setName + "Alt" + altPalette); ;
                }

                if (!isBackground && environmentalEffect != 0)
                {
                    //these Resource.Load calls should happen once and be saved somewhere (like a static palette manager or something)
                    if (environmentalEffect == EnvironmentalEffect.Heat)
                    {
                        tilemap.Material = ResourcePrefabManager.instance.LoadMaterial("Materials/" + setName + "Heat");
                    }
                    var defaultPalette = ResourcePrefabManager.instance.LoadTexture2D("Palettes/" + setName + "/" + setName + "Palette");
                    var cylceName = setName + envEffectName;
                    var paletteCycle = ResourcePrefabManager.instance.LoadPaletteCycle("PaletteCycles/" + cylceName);
                    if (defaultPalette != null && paletteCycle != null)
                    {
                        var cycling = tilemap.gameObject.AddComponent<TilemapPaletteCycling>();
                        cycling.defaultPalette = defaultPalette;
                        cycling.paletteCycle = paletteCycle;
                        cycling.cycleFrequency = 0.2f;
                    }
                }
            }
        }
    }

    public void SaveRoomInfo()
    {
        if (transform.position != Vector3.zero)
        {
            throw new Exception("Room Position Must Be Vector3.zero!");
        }

        roomInfo.sceneName = SceneManager.GetActiveScene().name;

        var playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
        if (playerStart)
        {
            roomInfo.playerStartOffset = playerStart.transform.localPosition;
            roomInfo.playerStartFacing = playerStart.transform.rotation == Quaternion.identity ? Direction.Right : Direction.Left;
        }

        var data = JsonConvert.SerializeObject(roomInfo, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        var path = Constants.roomDataPath + roomInfo.sceneName + ".txt";
        File.WriteAllText(path, data);
        Debug.Log(roomInfo.sceneName + " data saved!");
    }

#if UNITY_EDITOR
    public void SetPermanentStateObjects()
    {
        var permanentStateObjects = GetComponentsInChildren<PermanentStateObject>(true);
        int i = 0;
        roomInfo.permanentStateObjectCount = permanentStateObjects.Length;

        foreach (var obj in permanentStateObjects)
        {
            obj.localID = i;
            UnityEditor.EditorUtility.SetDirty(obj);
            i++;
        }
    }
#endif

    public void StartLockDown()
    {
        foreach (var trigger in transitionTriggers)
        {
            if (trigger.door)
            {
                trigger.door.locked = true;
            }
        }
    }

    public void EndLockDown()
    {
        foreach (var trigger in transitionTriggers)
        {

            if (trigger.door)
            {
                trigger.door.locked = false;
            }
        }
    }

    public Int2D GetLocalGridPosition(Vector3 worldPosition)
    {
        return new Int2D((int)((worldPosition.x+Constants.roomWidth * 0.5f) / Constants.roomWidth), roomInfo.size.y - 1 + Mathf.RoundToInt(worldPosition.y / Constants.roomHeight));
    }

    public void OnDrawGizmos()
    {
        if(roomInfo.traversalPaths.Count > 0)
        {
            foreach (var path in roomInfo.traversalPaths)
            {
                var fromPositon = new Vector2(path.from.minGridPosition.x * Constants.roomWidth, (-(roomInfo.size.y - 1) - path.from.minGridPosition.y) * Constants.roomHeight);
                fromPositon.x += Constants.roomWidth * -0.5f;
                fromPositon.y += Constants.roomHeight * -0.5f;
                var fromSize = Vector2.one + (path.from.maxGridPosition - path.from.minGridPosition).Vector2();
                fromSize.x *= Constants.roomWidth;
                fromSize.y *= Constants.roomHeight;
                var fromRect = new Rect(fromPositon, fromSize);

                Extensions.DrawSquare(fromRect, path.reciprocal ? Color.yellow : Color.green);

                var toPositon = new Vector2(path.to.minGridPosition.x * Constants.roomWidth, (path.to.minGridPosition.y - (roomInfo.size.y - 1)) * Constants.roomHeight);
                toPositon.x += Constants.roomWidth * -0.5f;
                toPositon.y += Constants.roomHeight * -0.5f;
                var toSize = Vector2.one + (path.to.maxGridPosition - path.to.minGridPosition).Vector2();
                toSize.x *= Constants.roomWidth;
                toSize.y *= Constants.roomHeight;
                var toRect = new Rect(toPositon, toSize);

                Extensions.DrawSquare(toRect, path.reciprocal ? Color.yellow : Color.red);
            }
        }

        if(showBounds)
        {
            Extensions.DrawSquare(new Rect(worldBounds.min, worldBounds.size), Color.white);
        }
    }
}
