using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Text;
using System.IO;
using Rewired;

public class LayoutDebug : MonoBehaviour
{
    public static LayoutDebug instance;
    public RectTransform container;
    public DebugRoom roomUIPrefab;
    public GameMode gameMode;
    public Text roomCount;
    public Text stepCount;
    public string password;
    [Range(0, Constants.maxSeed)]
    public int seed;
    public bool clearLogOnRoomCountTest = true;
    public int highlightExpected = -1;

    public LayoutPattern pattern;

    public int fastSteps;
    public float stepTime = 0.1f;

    public RoomLayout currentLayout;
    public List<DebugRoom> debugRooms = new List<DebugRoom>();

    public Sprite surfaceRoom;
    public Sprite caveRoom;
    public Sprite factoryRoom;
    public Sprite buriedCityRoom;
    public Sprite beastGutsRoom;
    public Sprite glitchRoom;
    public Sprite forestSlumsRoom;
    public Sprite coolantSewersRoom;
    public Sprite crystalMinesRoom;
    public Sprite blank;

    public RectTransform area0Limits;
    public RectTransform area1Limits;
    public RectTransform area2Limits;
    public RectTransform area3Limits;
    public RectTransform beastGutsLimits;
    public RectTransform glitchLimits;
    
    public bool allAchievements;
    public List<AchievementID> achievements;

    public void Awake()
    {
        instance = this;        
    }

    private List<Image> _pathDots = new List<Image>();

    public void Start()
    {
        currentLayout = new RoomLayout();
    }

    public void GenerateLayout()
    {
#if UNITY_EDITOR
        Debug.ClearDeveloperConsole();
        UnityEditor.AssetDatabase.Refresh();
#endif
        StartCoroutine(StepThroughLevelGeneration());
    }

    public void BossRush()
    {
#if UNITY_EDITOR
        Debug.ClearDeveloperConsole();
        UnityEditor.AssetDatabase.Refresh();
#endif
        StartCoroutine(StepThroughBossRush());
    }

    public void ClassicBossRush()
    {
#if UNITY_EDITOR
        Debug.ClearDeveloperConsole();
        UnityEditor.AssetDatabase.Refresh();
#endif
        StartCoroutine(StepThroughClassicBossRush());
    }

    public void Update()
    {
        if(currentLayout != null)
        {
            //var axis = ReInput.controllers.Mouse.GetAxis(2);
            //if (_canvasScaler.scaleFactor < 8 && axis > 0)
            //{
            //    _canvasScaler.scaleFactor += Time.deltaTime * 4;
            //    _canvasScaler.scaleFactor = Mathf.Clamp(_canvasScaler.scaleFactor, 0.01f, 7);
            //}
            //else if (axis < 0 && _canvasScaler.scaleFactor > 0.01f)
            //{
            //    _canvasScaler.scaleFactor += Time.deltaTime * -4;
            //    _canvasScaler.scaleFactor = Mathf.Clamp(_canvasScaler.scaleFactor, 0.01f, 7);
            //}

            var offset = new Vector2(currentLayout.width / 2f, currentLayout.height / 2f);

            while (debugRooms.Count < currentLayout.roomAbstracts.Count)
            {
                var room = Instantiate(roomUIPrefab);
                room.transform.SetParent(container);
                debugRooms.Add(room);
            }

            for (int i = 0; i < debugRooms.Count; i++)
            {
                var room = debugRooms[i];
                if (i < currentLayout.roomAbstracts.Count)
                {
                    room.gameObject.SetActive(true);
                    var roomAbstract = currentLayout.roomAbstracts[i];
                    room.MatchAbstract(roomAbstract, offset);
                    if (highlightExpected >= 0)
                    {
                        room.highlight.gameObject.SetActive(roomAbstract.expectedCapabilitiesIndex == highlightExpected);
                        room.highlight.color = new Color(0, 1, 0, 0.2f);
                    }
                    else
                    {
                        room.highlight.gameObject.SetActive(true);
                        Color color = ExpCapColor(roomAbstract.expectedCapabilitiesIndex);
                        color.a = 0.15f;
                        room.highlight.color = color;
                        var hRect = room.highlight.GetComponent<RectTransform>();

                        if (roomAbstract.specialExpectedCapabilities != null)
                        {
                            room.specialExpected.gameObject.SetActive(true);
                            room.specialExpected.sprite = room.highlight.sprite;

                            float minX = 10000;
                            float maxX = 0;
                            float minY = 10000;
                            float maxY = 0;
                            int exp = 0;
                            foreach (var kvp in roomAbstract.specialExpectedCapabilities)
                            {
                                var pos = kvp.Key;
                                exp = kvp.Value;
                                if (pos.x < minX) { minX = pos.x; }
                                if (pos.x > maxX) { maxX = pos.x; }
                                if (pos.y < minY) { minY = pos.y; }
                                if (pos.y > maxY) { maxY = pos.y; }
                            }

                            color = LayoutDebug.ExpCapColor(exp);
                            color.a = 0.15f;
                            room.specialExpected.color = color;

                            var r = room.specialExpected.GetComponent<RectTransform>();
                            r.anchorMax = new Vector2((maxX+1f) / roomAbstract.width, (maxY+1f) / roomAbstract.height);
                            r.anchorMin = new Vector2(minX / roomAbstract.width, minY / roomAbstract.height);

                            minX = 10000;
                            maxX = 0;
                            minY = 10000;
                            maxY = 0;
                            for (int x = 0; x < roomAbstract.width; x++)
                            {
                                for (int y = 0; y < roomAbstract.height; y++)
                                {
                                    var key = new Int2D(x, y);
                                    if (!roomAbstract.specialExpectedCapabilities.ContainsKey(key))
                                    {
                                        if (x < minX) { minX = x; }
                                        if (x > maxX) { maxX = x; }
                                        if (y < minY) { minY = y; }
                                        if (y > maxY) { maxY = y; }
                                    }
                                }
                            }

                            hRect.anchorMax = new Vector2((maxX + 1f) / roomAbstract.width, (maxY + 1f) / roomAbstract.height);
                            hRect.anchorMin = new Vector2(minX / roomAbstract.width, minY / roomAbstract.height);
                        }
                        else
                        {
                            room.specialExpected.gameObject.SetActive(false);
                            hRect.anchorMin = Vector2.zero;
                            hRect.anchorMax = Vector2.one;
                        }
                    }
                }
                else
                {
                    room.gameObject.SetActive(false);
                }
            }

            if (currentLayout.environmentLimits != null)
            {
                Sprite sprite = null;
                foreach (var limit in currentLayout.environmentLimits)
                {
                    RectTransform rect = null;
                    switch (limit.Key)
                    {
                        case EnvironmentType.Surface:
                            rect = area0Limits;
                            sprite = surfaceRoom;
                            break;
                        case EnvironmentType.ForestSlums:
                            rect = area0Limits;
                            sprite = forestSlumsRoom;
                            break;
                        case EnvironmentType.Cave:
                            rect = area1Limits;
                            sprite = caveRoom;
                            break;
                        case EnvironmentType.CoolantSewers:
                            rect = area1Limits;
                            sprite = coolantSewersRoom;
                            break;
                        case EnvironmentType.Factory:
                            rect = area2Limits;
                            sprite = factoryRoom;
                            break;
                        case EnvironmentType.CrystalMines:
                            rect = area2Limits;
                            sprite = crystalMinesRoom;
                            break;
                        case EnvironmentType.BuriedCity:
                            rect = area3Limits;
                            sprite = buriedCityRoom;
                            break;
                        case EnvironmentType.BeastGuts:
                            rect = beastGutsLimits;
                            sprite = beastGutsRoom;
                            break;
                        case EnvironmentType.Glitch:
                            rect = glitchLimits;
                            sprite = glitchRoom;
                            break;
                    }

                    if (rect != null)
                    {
                        rect.sizeDelta = limit.Value.size * DebugRoom.roomUnit;
                        rect.anchoredPosition = new Vector2(limit.Value.min.x - offset.x, limit.Value.yMax - offset.y - 1) * DebugRoom.roomUnit;
                        if (sprite != null)
                        {
                            var rectImage = rect.GetComponent<Image>();
                            rectImage.sprite = sprite;
                        }
                    }
                }
            }
        }
    }

    public static Color32 ExpCapColor(int expCap)
    {
        switch (expCap)
        {
            case 1:
                return Color.red;
            case 2:
                return new Color(1, 0.45f, 0);
            case 3:
                return Color.yellow;                
            case 4:
                return Color.green;                
            case 5:
                return Color.cyan;                
            case 6:
                return Color.blue;                
            case 7:
                return Color.magenta;
            default:
                return Color.clear;
        }
    }

    public IEnumerator StepThroughLevelGeneration()
    {
        var layoutGenerator = new LayoutGenerator();

        currentLayout = new RoomLayout();
        currentLayout.gameMode = gameMode;

        int steps = 0;
        var achievementList = achievements;

        if (allAchievements)
        {
            achievementList = Enum.GetValues(typeof(AchievementID)).Cast<AchievementID>().ToList();
            achievementList.Remove(AchievementID.None);
        }   

        List<MajorItem> itemOrder = null;

        if(!string.IsNullOrEmpty(password))
        {
            var parameters = SeedHelper.KeyToParameters(password);
            if(parameters != null)
            {
                seed = parameters.seed;
                gameMode = parameters.gameMode;
                itemOrder = parameters.traversalItems;
                achievementList = SeedHelper.MergeAchievements(achievementList, parameters.achievements);
            }
        }

        layoutGenerator.stepByStep = stepTime > 0;
        foreach (var step in layoutGenerator.PopulateLayout(seed, currentLayout, achievementList, itemOrder, null, pattern, 1))
        {
            steps++;
            stepCount.text = "Steps: " + steps;
            if (stepTime > 0 && steps > fastSteps)
            {
                yield return new WaitForSeconds(stepTime);
            }

            if (layoutGenerator.path != null && layoutGenerator.stepByStep)
            {
                var path = layoutGenerator.path;
                var count = _pathDots.Count > path.Count ? _pathDots.Count : path.Count;                
                var offset = new Vector2(-currentLayout.width / 2f, -currentLayout.height / 2f) + new Vector2(0.5f, -0.5f);
                for (int i = 0; i < count; i++)
                {
                    if(i >= _pathDots.Count)
                    {
                        Image dot = new GameObject("Dot " + _pathDots.Count).AddComponent<Image>();
                        dot.rectTransform.SetParent(container);
                        dot.sprite = blank;
                        dot.rectTransform.sizeDelta = Vector2.one * DebugRoom.roomUnit * 0.5f;
                        dot.color = Color.green;
                        _pathDots.Add(dot);
                    }

                    var d = _pathDots[i];
                    if (i < path.Count)
                    {
                        d.rectTransform.SetAsLastSibling();
                        d.gameObject.SetActive(true);
                        d.rectTransform.anchoredPosition = (path[i].Vector2() + offset) * DebugRoom.roomUnit;
                    }
                    else
                    {
                        d.gameObject.SetActive(false);
                    }
                }
            }
        }
        roomCount.text = "Room Count: " + currentLayout.roomAbstracts.Count;

        password = currentLayout.password;
    }

    public IEnumerator StepThroughBossRush()
    {
        var layoutGenerator = new LayoutGenerator();
        currentLayout = new RoomLayout();

        int steps = 0;
        layoutGenerator.stepByStep = stepTime > 0;

        var chevos = allAchievements ? null : achievements;
        foreach (var step in layoutGenerator.BossRush(seed, currentLayout, chevos))
        {
            steps++;
            stepCount.text = "Steps: " + steps;
            if (stepTime > 0 && steps > fastSteps)
            {
                yield return new WaitForSeconds(stepTime);
            }
        }
    }

    public IEnumerator StepThroughClassicBossRush()
    {
        var layoutGenerator = new LayoutGenerator();
        currentLayout = new RoomLayout();

        int steps = 0;
        layoutGenerator.stepByStep = stepTime > 0;

        foreach (var step in layoutGenerator.ClassicBossRush(seed, currentLayout, achievements))
        {
            steps++;
            stepCount.text = "Steps: " + steps;
            if (stepTime > 0 && steps > fastSteps)
            {
                yield return new WaitForSeconds(stepTime);
            }
        }
    }

    public void TestRoomCounts()
    {
        Debug.ClearDeveloperConsole();

        Dictionary<EnvironmentType, Dictionary<string, int>> roomCounts = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<EnvironmentType, int> envCounts = new Dictionary<EnvironmentType, int>();
        Dictionary<EnvironmentType, int> generalEnvCounts = new Dictionary<EnvironmentType, int>();
        Dictionary<string, int> sizeCounts = new Dictionary<string, int>();
        Dictionary<string, int> generalSizeCounts = new Dictionary<string, int>();
        Dictionary<string, int> patternCounts = new Dictionary<string, int>();
        //Dictionary<string, int> generalAllExitCounts = new Dictionary<string, int>();
        Dictionary<EnvironmentType, Dictionary<string, int>> genRoomInfoEnvSizeCounts = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<EnvironmentType, Dictionary<string, int>> genRoomInfoSizeCounts = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<EnvironmentType, int> genRoomInfoCounts = new Dictionary<EnvironmentType, int>();
        Dictionary<EnvironmentType, Dictionary<string, int>> supportAbility = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<EnvironmentType, Dictionary<string, int>> useAbility = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<MajorItem, int> itemCount = new Dictionary<MajorItem, int>();
        Dictionary<BossName, int> bossCounts = new Dictionary<BossName, int>();

        int doublesCount = 0;
        Dictionary<EnvironmentType, int> envDoublesCounts = new Dictionary<EnvironmentType, int>();
        int allExitCount = 0;
        Dictionary<EnvironmentType, int> envAllExitCounts = new Dictionary<EnvironmentType, int>();
        int generalDoublesCount = 0;
        Dictionary<EnvironmentType, int> generalEnvDoubleCounts = new Dictionary<EnvironmentType, int>();
        Dictionary<EnvironmentType, Dictionary<string, int>> generalEnvDoublesBySize = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<EnvironmentType, Dictionary<string, int>> doublesUseAbility = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        Dictionary<EnvironmentType, Dictionary<string, int>> doublesByName = new Dictionary<EnvironmentType, Dictionary<string, int>>();
        int generalAllExitCount = 0;
        Dictionary<EnvironmentType, int> generalEnvAllExitCounts = new Dictionary<EnvironmentType, int>();

        float totalAttempts = 0;


        var jump = "Jump";
        var fly = "Fly";
        var smallGap = "Small Gap";
        var pierceTerrain = "Pierce Terrain";
        //var fire = DamageType.Fire.ToString();
        //var electric = DamageType.Electric.ToString();
        //var explosive = DamageType.Explosive.ToString();
        var velocity = DamageType.Velocity.ToString();
        var phaseWall = "Phase Wall";

        var layoutGenerator = new LayoutGenerator();
        layoutGenerator.Initialize(allAchievements ? null : achievements);

        foreach (var kvp in layoutGenerator.roomInfos)
        {
            if (!genRoomInfoSizeCounts.ContainsKey(kvp.Key)) { genRoomInfoSizeCounts[kvp.Key] = new Dictionary<string, int>(); }
            if (!genRoomInfoCounts.ContainsKey(kvp.Key)) { genRoomInfoCounts[kvp.Key] = 0; }

            if (!supportAbility.ContainsKey(kvp.Key))
            {
                supportAbility[kvp.Key] = new Dictionary<string, int>();
                supportAbility[kvp.Key][jump] = 0;
                supportAbility[kvp.Key][fly] = 0;
                supportAbility[kvp.Key][smallGap] = 0;
                supportAbility[kvp.Key][pierceTerrain] = 0;
                supportAbility[kvp.Key][velocity] = 0;
                supportAbility[kvp.Key][phaseWall] = 0;
            }

            var dictionary = genRoomInfoSizeCounts[kvp.Key];
            foreach (var info in kvp.Value.generalRoomInfos)
            {
                var size = info.size.ToString();
                if (!dictionary.ContainsKey(size)) { dictionary[size] = 0; }

                dictionary[size]++;
                genRoomInfoCounts[kvp.Key]++;

                bool supportJump = false;
                bool supportFly = false;
                bool supportSmallGap = false;
                bool supportPierceTerrain = false;
                bool supportVelocity = false;
                bool supportPhaseWall = false;

                foreach (var exit in info.possibleExits)
                {
                    if (exit.toExit.requiredJumpHeight >= 9)
                    {
                        if (exit.toExit.requiredJumpHeight >= 14) { supportFly = true; }
                        else { supportJump = true; }
                    }

                    if (exit.toExit.supportedDamageTypes.HasFlag(DamageType.Velocity)) { supportVelocity = true; }
                    if (exit.toExit.supportsGroundedSmallGaps || exit.toExit.requiresGroundedSmallGaps) { supportSmallGap = true; }
                    if (exit.toExit.supportsShotIgnoresTerrain || exit.toExit.requiresShotIgnoresTerrain) { supportPierceTerrain = true; }
                    if (exit.toExit.supportsPhaseThroughWalls) { supportPhaseWall = true; }
                }

                foreach (var tp in info.traversalPaths)
                {
                    if (tp.limitations.requiredJumpHeight >= 9)
                    {
                        if (tp.limitations.requiredJumpHeight >= 14) { supportFly = true; }
                        else { supportJump = true; }
                    }

                    if (tp.limitations.supportedDamageTypes.HasFlag(DamageType.Velocity)) { supportVelocity = true; }
                    if (tp.limitations.supportsGroundedSmallGaps || tp.limitations.requiresGroundedSmallGaps) { supportSmallGap = true; }
                    if (tp.limitations.supportsShotIgnoresTerrain || tp.limitations.requiresShotIgnoresTerrain) { supportPierceTerrain = true; }
                    if (tp.limitations.supportsPhaseThroughWalls) { supportPhaseWall = true; }
                }

                if (supportJump) { supportAbility[kvp.Key][jump]++; }
                if (supportFly) { supportAbility[kvp.Key][fly]++; }
                if (supportSmallGap) { supportAbility[kvp.Key][smallGap]++; }
                if (supportPierceTerrain) { supportAbility[kvp.Key][pierceTerrain]++; }
                if (supportVelocity) { supportAbility[kvp.Key][velocity]++; }
                if (supportPhaseWall) { supportAbility[kvp.Key][phaseWall]++; } 
            }
        }

        int totalRooms = 0;
        int totalGeneralRooms = 0;
        int tests = 1000;

        DateTime testStart = DateTime.UtcNow;

        Dictionary<string, int> roomUsed = new Dictionary<string, int>();

        for (int i = 0; i < tests; i++)
        {
            var layout = layoutGenerator.GenerateStandardLayout(Random.Range(0, Constants.maxSeed), allAchievements ? null : achievements);
            totalAttempts += layoutGenerator.populateAttempts;
            var roomChecked = new HashSet<string>();

            var pattern = layout.pattern.ToString();
            if (!patternCounts.ContainsKey(pattern)) { patternCounts[pattern] = 0; }
            patternCounts[pattern]++;

            foreach (var item in layout.allNonTraversalItemsAdded)
            {
                if (!itemCount.ContainsKey(item)) { itemCount[item] = 0; }
                itemCount[item]++;
            }

            foreach (var item in layout.itemOrder)
            {
                if (!itemCount.ContainsKey(item)) { itemCount[item] = 0; }
                itemCount[item]++;
            }

            var usedRooms = new HashSet<string>();
            for (int j = 0; j < layout.roomAbstracts.Count; j++)
            {
                var roomAbstract = layout.roomAbstracts[j];
                var info = roomAbstract.assignedRoomInfo;
                var sceneName = info.sceneName;
                var roomSizeString = info.size.ToString();
                var env = info.environmentType;

                bool isDouble = usedRooms.Contains(info.sceneName);
                if (!isDouble)
                {
                    usedRooms.Add(info.sceneName);
                }
                else
                {
                    if (!envDoublesCounts.ContainsKey(env)) { envDoublesCounts[env] = 0; }
                    envDoublesCounts[info.environmentType]++;
                    doublesCount++;
                }

                if (!useAbility.ContainsKey(info.environmentType))
                {
                    useAbility[env] = new Dictionary<string, int>();
                    useAbility[env][jump] = 0;
                    useAbility[env][fly] = 0;
                    useAbility[env][smallGap] = 0;
                    useAbility[env][pierceTerrain] = 0;
                    useAbility[env][velocity] = 0;
                    useAbility[env][phaseWall] = 0;
                }

                var hasAllExits = info.HasAllPossibleExitsForSize();
                if (hasAllExits)
                {
                    if(!envAllExitCounts.ContainsKey(env)) { envAllExitCounts[env] = 0; }
                    envAllExitCounts[env]++;
                    allExitCount++;
                }

                if (!roomChecked.Contains(sceneName))
                {
                    roomChecked.Add(sceneName);
                    if (!roomUsed.ContainsKey(sceneName)) { roomUsed[sceneName] = 0; }
                    roomUsed[sceneName]++;
                }

                if (!roomCounts.ContainsKey(env)) { roomCounts[env] = new Dictionary<string, int>(); }
                if (!roomCounts[env].ContainsKey(sceneName)) { roomCounts[env][sceneName] = 0; }

                if (!envCounts.ContainsKey(env)) { envCounts[env] = 0; }
                if (!sizeCounts.ContainsKey(roomSizeString)) { sizeCounts[roomSizeString] = 0; }

                bool jumpRoom = false;
                bool flyRoom = false;
                bool smallGapRoom = false;
                bool pierceTerrainRoom = false;
                bool velocityRoom = false;
                bool phaseWallRoom = false;

                foreach (var exit in roomAbstract.exits)
                {
                    if (exit.toExit.maxEffectiveJumpHeight >= 9)
                    {
                        if (exit.toExit.maxEffectiveJumpHeight >= 14) { flyRoom = true; }
                        else { jumpRoom = true; }
                    }

                    if (exit.toExit.requiredDamageType.HasFlag(DamageType.Velocity)) { velocityRoom = true; }
                    if (exit.toExit.requiresGroundedSmallGaps) { smallGapRoom = true; }
                    if (exit.toExit.requiresShotIgnoresTerrain) { pierceTerrainRoom = true; }
                    if (exit.toExit.requiresPhaseThroughWalls) { phaseWallRoom = true; }
                }

                foreach (var tp in roomAbstract.traversalPathRequirements)
                {
                    if (tp.maxEffectiveJumpHeight >= 9)
                    {
                        if (tp.maxEffectiveJumpHeight >= 14) { flyRoom = true; }
                        else { jumpRoom = true; }
                    }

                    if (tp.requiredDamageType.HasFlag(DamageType.Velocity)) { velocityRoom = true; }
                    if (tp.requiresGroundedSmallGaps) { smallGapRoom = true; }
                    if (tp.requiresShotIgnoresTerrain) { pierceTerrainRoom = true; }
                    if (tp.requiresPhaseThroughWalls) { phaseWallRoom = true; }
                }

                if (jumpRoom) { useAbility[env][jump]++; }
                if (flyRoom) { useAbility[env][fly]++; }
                if (smallGapRoom) { useAbility[env][smallGap]++; }
                if (pierceTerrainRoom) { useAbility[env][pierceTerrain]++; }
                if (velocityRoom) { useAbility[env][velocity]++; }
                if (phaseWallRoom) { useAbility[env][phaseWall]++; }

                if (info.roomType == RoomType.None)
                {
                    if (!generalEnvCounts.ContainsKey(env)) { generalEnvCounts[env] = 0; }
                    if (!generalEnvDoublesBySize.ContainsKey(env)) { generalEnvDoublesBySize.Add(env, new Dictionary<string, int>()); }
                    if (!generalSizeCounts.ContainsKey(roomSizeString)) { generalSizeCounts[roomSizeString] = 0; }
                    if (!genRoomInfoEnvSizeCounts.ContainsKey(env)) { genRoomInfoEnvSizeCounts[env] = new Dictionary<string, int>(); }
                    var envSizeDictionary = genRoomInfoEnvSizeCounts[env];
                    var envDoubleSizeDictionary = generalEnvDoublesBySize[env];

                    if (!envSizeDictionary.ContainsKey(roomSizeString)) { envSizeDictionary[roomSizeString] = 0; }

                    envSizeDictionary[roomSizeString]++;
                    generalEnvCounts[env]++;
                    generalSizeCounts[roomSizeString]++;
                    totalGeneralRooms++;

                    if (hasAllExits)
                    {
                        if (!generalEnvAllExitCounts.ContainsKey(env)) { generalEnvAllExitCounts[env] = 0; }
                        generalEnvAllExitCounts[env]++;
                        generalAllExitCount++;
                    }

                    if (isDouble)
                    {
                        if (!doublesByName.ContainsKey(env))
                        {
                            doublesByName[env] = new Dictionary<string, int>();
                        }
                        
                        if(!doublesByName[env].ContainsKey(sceneName))
                        {
                            doublesByName[env][sceneName] = 0;
                        }
                        doublesByName[env][sceneName]++;

                        if (!doublesUseAbility.ContainsKey(env))
                        {
                            doublesUseAbility[env] = new Dictionary<string, int>();
                            doublesUseAbility[env][jump] = 0;
                            doublesUseAbility[env][fly] = 0;
                            doublesUseAbility[env][smallGap] = 0;
                            doublesUseAbility[env][pierceTerrain] = 0;
                            doublesUseAbility[env][velocity] = 0;
                            doublesUseAbility[env][phaseWall] = 0;
                        }

                        if (jumpRoom) { doublesUseAbility[env][jump]++; }
                        if (flyRoom) { doublesUseAbility[env][fly]++; }
                        if (smallGapRoom) { doublesUseAbility[env][smallGap]++; }
                        if (pierceTerrainRoom) { doublesUseAbility[env][pierceTerrain]++; }
                        if (velocityRoom) { doublesUseAbility[env][velocity]++; }
                        if (phaseWallRoom) { doublesUseAbility[env][phaseWall]++; }

                        if (!envDoubleSizeDictionary.ContainsKey(roomSizeString)) { envDoubleSizeDictionary[roomSizeString] = 0; }
                        envDoubleSizeDictionary[roomSizeString]++;
                        if (!generalEnvDoubleCounts.ContainsKey(env)) { generalEnvDoubleCounts[env] = 0; }
                        generalEnvDoubleCounts[env]++;
                        generalDoublesCount++;
                    }
                }

                if(info.roomType == RoomType.BossRoom)
                {
                    if(!bossCounts.ContainsKey(info.boss))
                    {
                        bossCounts[info.boss] = 0;
                    }

                    bossCounts[info.boss]++;
                }

                sizeCounts[roomSizeString]++;
                envCounts[info.environmentType]++;
                roomCounts[info.environmentType][sceneName]++;
                totalRooms++;
            }
        }

        var testTime = DateTime.UtcNow - testStart;

        if (clearLogOnRoomCountTest) { Debug.ClearDeveloperConsole(); }

        var builder = new StringBuilder();

        var rList = roomUsed.ToList();
        rList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        foreach (var room in rList)
        {
            var appearsText = room.Key + " appears in ~" + ((float)roomUsed[room.Key] / tests) * 100 + "% of runs";
            builder.AppendLine(appearsText);
        }

        var sortedSizeCounts = sizeCounts.ToList();
        sortedSizeCounts.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        sortedSizeCounts.Reverse();

        foreach (var kvp in sortedSizeCounts)
        {
            var size = kvp.Key;
            float count = kvp.Value;
            int noTypeCount = 0;
            generalSizeCounts.TryGetValue(size, out noTypeCount);
            var text = "Average " + size + " Rooms: " + count / (float)tests + " (" + (float) noTypeCount / (float) tests+ " general rooms)";
            builder.AppendLine(text);            
        }

        var doubles = (doublesCount / (float)totalRooms) * 100 + "% of rooms repeated in a given run.";
        builder.AppendLine(doubles);
        var allExits = (allExitCount / (float)totalRooms) * 100 + "% of rooms were all exit rooms.";
        builder.AppendLine(allExits);
        var generalDoubles = (generalDoublesCount / (float)totalGeneralRooms) * 100 + "% of general rooms (rooms with no type) repeated in a given run.";
        builder.AppendLine(generalDoubles);
        var generalAllExits = (generalAllExitCount / (float)totalGeneralRooms) * 100 + "% of general rooms (rooms with no type) were all exit rooms.";
        builder.AppendLine(generalAllExits);

        var environments = new List<EnvironmentType>()
        { EnvironmentType.Surface, EnvironmentType.ForestSlums, EnvironmentType.Cave, EnvironmentType.CoolantSewers,
            EnvironmentType.Factory, EnvironmentType.CrystalMines, EnvironmentType.BuriedCity, EnvironmentType.BeastGuts,
            EnvironmentType.Glitch };

        foreach (var env in environments)
        {
            if (envCounts.ContainsKey(env))
            {
                float totalEnvRooms = envCounts[env];
                float totalEnvNoTypeRooms = generalEnvCounts[env];
                float totalRoomInfos = genRoomInfoCounts[env];

                var supportRoomInfos = supportAbility[env];
                var useAbilityEnv = useAbility[env];
                var doublesUseAbilityEnv = doublesUseAbility[env];

                builder.AppendLine("=====" + env.ToString() + "=====");
                var totalText = "Average Total " + env.ToString() + " Rooms: " + totalEnvRooms / (float)tests;
                builder.AppendLine(totalText);
                totalText = "Average Total " + env.ToString() + " General Rooms: " + totalEnvNoTypeRooms / (float)tests + " chosen from " + totalRoomInfos + " RoomInfos";
                builder.AppendLine(totalText);

                if(envAllExitCounts.ContainsKey(env))
                {
                    builder.AppendLine("% of All Exit Rooms: " + (envAllExitCounts[env] / totalEnvRooms) * 100);
                }

                if(envDoublesCounts.ContainsKey(env))
                {
                    builder.AppendLine("% of rooms that repeat: " + (envDoublesCounts[env] / totalEnvRooms) * 100);
                }

                if (envAllExitCounts.ContainsKey(env))
                {
                    builder.AppendLine("% of No Type All Exit Rooms: " + (generalEnvAllExitCounts[env] / totalEnvNoTypeRooms) * 100);
                }

                if (envDoublesCounts.ContainsKey(env))
                {
                    var envDoublesTotal = generalEnvDoubleCounts[env];
                    builder.AppendLine("% of No Type Rooms that repeat: " + (envDoublesTotal / totalEnvNoTypeRooms) * 100);
                    if (generalEnvDoublesBySize.ContainsKey(env))
                    {
                        var sorted = generalEnvDoublesBySize[env].ToList();
                        sorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                        sorted.Reverse();

                        foreach (var kvp2 in sorted)
                        {
                            var size = kvp2.Key;
                            var count = kvp2.Value;
                            var percent = (int)((count / (float)envDoublesTotal) * 100);
                            var text = "Size " + size + "rooms make up " + percent + "% of repeating rooms in " + env.ToString();
                            builder.AppendLine(text);
                        }
                    }

                    foreach (var kvp in doublesUseAbilityEnv)
                    {
                        if (kvp.Value == 0) continue;
                        var text = env.ToString() + " repeating rooms that use " + kvp.Key + ": " + 100 * ((kvp.Value) / (float)envDoublesTotal) + "%";
                        builder.AppendLine(text);
                    }

                    var nameSorted = doublesByName[env].ToList();
                    nameSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                    nameSorted.Reverse();
                    foreach (var kvp in nameSorted)
                    {
                        var text = kvp.Key + " repeated " + kvp.Value + " times over " + tests + " generations";
                        builder.AppendLine(text);
                    }
                }

                foreach (var abil in supportRoomInfos)
                {
                    var text = env.ToString() + " Room Infos that support " + abil.Key + ": " + abil.Value;
                    builder.AppendLine(text);
                }

                foreach (var kvp in useAbilityEnv)
                {
                    if (kvp.Value == 0) continue;
                    var text = "Average " + env.ToString() + " rooms that use " + kvp.Key + ": " + (kvp.Value) / (float)tests + " out of " + supportRoomInfos[kvp.Key] + " room Infos.";
                    builder.AppendLine(text);
                }

                if (genRoomInfoEnvSizeCounts.ContainsKey(env))
                {
                    var sorted = genRoomInfoEnvSizeCounts[env].ToList();
                    sorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                    sorted.Reverse();

                    foreach (var kvp2 in sorted)
                    {
                        var size = kvp2.Key;
                        var count = kvp2.Value;
                        var percent = (int)((count / totalEnvNoTypeRooms) * 100);
                        var text = "Average " + env.ToString() + " " + size + " Rooms: " + (float)count / tests + " (" + percent + "% of general rooms)";

                        if (genRoomInfoSizeCounts.ContainsKey(env))
                        {
                            var gRoomInfos = genRoomInfoSizeCounts[env];
                            if(gRoomInfos.ContainsKey(size))
                            {
                                count = gRoomInfos[size];
                                text += " chosen from " + count + " RoomInfos";
                            }
                        }

                        builder.AppendLine(text);
                    }
                }

                builder.AppendLine("=============================");
            }
        }

        builder.AppendLine("Item Appearance:");
        var sortedItemCounts = itemCount.ToList();
        sortedItemCounts.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        sortedItemCounts.Reverse();
        foreach (var kvp in sortedItemCounts)
        {
            builder.AppendLine(kvp.Key.ToString() + ":" + (float)kvp.Value / (float)tests);
        }

        builder.AppendLine("=============================");

        builder.AppendLine("Boss Appearance:");
        foreach (var kvp in bossCounts)
        {
            builder.AppendLine(kvp.Key.ToString() + ":" + (float)kvp.Value / tests);
        }

        builder.AppendLine("=============================");

        foreach (var kvp in patternCounts)
        {
            var t = kvp.Key + " used for " + (float)kvp.Value / tests + " of runs";
            Debug.Log(t);
            builder.AppendLine(t);
        }

        Debug.Log("Test took: " + testTime);
        builder.AppendLine("Test took: " + testTime);
        Debug.Log("Average Total Room Count: " + totalRooms / tests);
        builder.AppendLine("Average Total Room Count: " + totalRooms / tests);
        Debug.Log("Average Populate Attempts: " + totalAttempts / tests);
        builder.AppendLine("Average Populate Attempts: " + totalAttempts / tests);

        foreach (var kvp in layoutGenerator.roomInfos)
        {
            var list = new List<RoomInfo>();
            list.AddRange(kvp.Value.bossRoomInfos);
            list.AddRange(kvp.Value.generalRoomInfos);
            list.AddRange(kvp.Value.majorItemRoomInfos);
            list.AddRange(kvp.Value.megaBeastRoomInfos);
            list.AddRange(kvp.Value.minorItemRoomInfos);
            list.AddRange(kvp.Value.saveRoomInfos);
            list.AddRange(kvp.Value.shopRoomsInfos);
            list.AddRange(kvp.Value.startingRoomInfos);
            list.AddRange(kvp.Value.transitionRoomInfos);

            foreach (var room in list)
            {
                if (roomCounts.ContainsKey(kvp.Key) && !roomCounts[kvp.Key].ContainsKey(room.sceneName))
                {
                    builder.AppendLine(room.sceneName + " never used!");
                    Debug.LogWarning(room.sceneName + " never used!");
                }
            }
        }

        string path = Application.isEditor ? Application.dataPath + "/../Layout Info/" : Application.dataPath + "/Layout Info/";
        var name = allAchievements ? "LayoutDataAllAchievements.txt" : "LayoutData.txt";
        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
        File.WriteAllText(path+name, builder.ToString());
    }
}