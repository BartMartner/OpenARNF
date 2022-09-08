using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;

public partial class LayoutGenerator
{
    //A seeded random number generator for all methods to use. Set at the start of PopulateLayout
    public MicrosoftRandom random;

    public Dictionary<EnvironmentType, RoomLists> roomInfos = new Dictionary<EnvironmentType, RoomLists>();
    public Dictionary<MajorItem, MajorItemInfo> traversalItemInfos = new Dictionary<MajorItem, MajorItemInfo>();
    public Dictionary<MajorItem, MajorItemInfo> activatedItemInfos = new Dictionary<MajorItem, MajorItemInfo>();
    public Dictionary<MajorItem, MajorItemInfo> orbSmithItemInfos = new Dictionary<MajorItem, MajorItemInfo>();
    public Dictionary<MajorItem, MajorItemInfo> gunSmithItemInfos = new Dictionary<MajorItem, MajorItemInfo>();
    public Dictionary<MajorItem, MajorItemInfo> artificerItemInfos = new Dictionary<MajorItem, MajorItemInfo>();
    public Dictionary<MajorItem, MajorItemInfo> theTraitorItemInfos = new Dictionary<MajorItem, MajorItemInfo>();
    public List<MajorItemInfo> nonTraversalItems = new List<MajorItemInfo>();

    /// <summary>
    /// all available minor items
    /// </summary>
    public List<MinorItemType> availableMinorItems = new List<MinorItemType>();

    /// <summary>
    /// all items list in nonTraversalItems for which the player has the appropriate achievements and no segment includes in specificItems
    /// </summary>
    public List<MajorItemInfo> availableNonTraversalItems = new List<MajorItemInfo>();

    /// <summary>
    /// The number of attempts made by PopulateLayout last time it was called
    /// </summary>
    public int populateAttempts = 0;

    public bool stepByStep;
    private RoomLayout _layout;
    private bool _initialized;
    public List<Int2D> path { get; private set; }

    public LayoutGenerator() { }

    public void Initialize(List<AchievementID> achievements)
    {
        Debug.Log("Initializing Layout Generator!");

        List<MajorItemInfo> loadedItemInfos = new List<MajorItemInfo>();
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/TraversalItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/OtherItems"));
        loadedItemInfos.AddRange(Resources.LoadAll<MajorItemInfo>("MajorItemInfos/ActivatedItems"));

        traversalItemInfos.Clear();
        activatedItemInfos.Clear();
        orbSmithItemInfos.Clear();
        gunSmithItemInfos.Clear();
        artificerItemInfos.Clear();
        theTraitorItemInfos.Clear();
        nonTraversalItems.Clear();

        foreach (var item in loadedItemInfos)
        {
            if (Constants.excludeItems.Contains(item.type)) { continue; }

            if (item.isTraversalItem)
            {
                traversalItemInfos.Add(item.type, item);
            }
            else
            {
                if (item.nontraversalPool)
                {
                    nonTraversalItems.Add(item);
                }

                if (item.isActivatedItem)
                {
                    activatedItemInfos.Add(item.type, item);
                }
    
                if (item.orbSmithPool)
                {
                    orbSmithItemInfos.Add(item.type, item);
                }

                if (item.gunSmithPool)
                {
                    gunSmithItemInfos.Add(item.type, item);
                }
                    
                if(item.artificerPool)
                {
                    artificerItemInfos.Add(item.type, item);
                }                

                if(item.theTraitorPool)
                {
                    theTraitorItemInfos.Add(item.type, item);
                }
            }
        }

        TextAsset[] files = Resources.LoadAll<TextAsset>("RoomData");

        foreach (EnvironmentType env in Enum.GetValues(typeof(EnvironmentType)))
        {
            roomInfos[env] = new RoomLists();
        }

        foreach (var file in files)
        {
            var roomInfo = JsonConvert.DeserializeObject<RoomInfo>(file.text);

            if (achievements != null)
            {
                if (roomInfo.environmentType == EnvironmentType.CoolantSewers && !achievements.Contains(AchievementID.CoolantSewers)) { continue; }
                if (roomInfo.environmentType == EnvironmentType.CrystalMines && !achievements.Contains(AchievementID.CrystalMines)) { continue; }

                if (roomInfo.requiredAchievements != null && roomInfo.requiredAchievements.Count > 0 &&
                    !achievements.ContainsAll(roomInfo.requiredAchievements))
                {
                    continue;
                }
            }

            //var slot = SaveGameManager.activeSlot;
            //if (slot != null && roomInfo.runThreshhold > slot.totalDeaths + slot.victories) { continue; }

            switch (roomInfo.roomType)
            {
                case RoomType.StartingRoom:
                    roomInfos[roomInfo.environmentType].startingRoomInfos.Add(roomInfo);
                    break;
                case RoomType.TransitionRoom:
                    roomInfos[roomInfo.environmentType].transitionRoomInfos.Add(roomInfo);
                    break;
                case RoomType.MegaBeast:
                    roomInfos[roomInfo.environmentType].megaBeastRoomInfos.Add(roomInfo);
                    break;
                case RoomType.BossRoom:
                    roomInfos[roomInfo.environmentType].bossRoomInfos.Add(roomInfo);
                    break;
                case RoomType.SaveRoom:
                    roomInfos[roomInfo.environmentType].saveRoomInfos.Add(roomInfo);
                    break;
                case RoomType.Teleporter:
                    roomInfos[roomInfo.environmentType].teleporterRoomInfos.Add(roomInfo);
                    break;
                case RoomType.ItemRoom:
                    roomInfos[roomInfo.environmentType].majorItemRoomInfos.Add(roomInfo);

                    if (roomInfo.minorItemLocations.Count > 0)
                    {
                        if (roomInfo.minorItemAlongPathOnly)
                        {
                            Debug.LogWarning(roomInfo.sceneName + " has mustBeItemRoom AND minorItemAlongPathOnly set.");
                        }
                        roomInfos[roomInfo.environmentType].minorItemRoomInfos.Add(roomInfo);
                    }
                    break;
                case RoomType.Shop:
                    roomInfos[roomInfo.environmentType].shopRoomsInfos.Add(roomInfo);
                    break;
                case RoomType.Shrine:
                    roomInfos[roomInfo.environmentType].shrineRoomsInfos.Add(roomInfo);
                    break;
                case RoomType.OtherSpecial:
                    roomInfos[roomInfo.environmentType].otherSpecialRoomsInfos.Add(roomInfo);
                    break;
                default:
                    roomInfos[roomInfo.environmentType].generalRoomInfos.Add(roomInfo);
                    if (roomInfo.minorItemLocations.Count > 0 && !roomInfo.minorItemAlongPathOnly)
                    {
                        roomInfos[roomInfo.environmentType].minorItemRoomInfos.Add(roomInfo);
                    }
                    break;
            }
        }

        ////DummyRooms
        //var dummyRooms = new List<DummyRoomInfo>
        //{
        //    //new DummyRoomInfo(EnvironmentType.BeastGuts, new Int2D(1,2), 3),
        //    //new DummyRoomInfo(EnvironmentType.BeastGuts, new Int2D(2,1), 3),
        //    new DummyRoomInfo(EnvironmentType.CrystalMines, new Int2D(2,2), 3),
        //};

        //foreach (var dRoom in dummyRooms)
        //{
        //    for (int i = 0; i < dRoom.amount; i++)
        //    {
        //        var dummyInfo = new RoomInfo() { size = dRoom.size, environmentType = dRoom.environmentType };
        //        dummyInfo.sceneName = dRoom.environmentType + " DummyInfo " + i;
        //        dummyInfo.traversalLimitations = new TraversalLimitations();

        //        for (int x = 0; x < dummyInfo.size.x; x++)
        //        {
        //            var exitLim = new ExitLimitations() { direction = Direction.Up };
        //            exitLim.localGridPosition = new Int2D(x, dummyInfo.size.y - 1);
        //            dummyInfo.possibleExits.Add(exitLim);
        //            exitLim = new ExitLimitations() { direction = Direction.Down };
        //            exitLim.localGridPosition = new Int2D(x, 0);
        //            exitLim.direction = Direction.Down;
        //            dummyInfo.possibleExits.Add(exitLim);
        //        }

        //        for (int y = 0; y < dummyInfo.size.y; y++)
        //        {
        //            var exitLim = new ExitLimitations() { direction = Direction.Right };
        //            exitLim.localGridPosition = new Int2D(dummyInfo.size.x - 1, y);
        //            dummyInfo.possibleExits.Add(exitLim);
        //            exitLim = new ExitLimitations() { direction = Direction.Left };
        //            exitLim.localGridPosition = new Int2D(0, y);
        //            dummyInfo.possibleExits.Add(exitLim);
        //        }

        //        dummyInfo.minorItemAlongPathOnly = true;
        //        roomInfos[dummyInfo.environmentType].generalRoomInfos.Add(dummyInfo);
        //    }
        //}

        _initialized = true;
    }

    public T PickRandom<T>(IList<T> list)
    {
        if(list.Count <= 0)
        {
            Debug.LogError("PickRandom was passed an empty list!");
            return default(T);
        }

        return list[random.Range(0, list.Count)];
    }

    public List<MajorItem> GenerateItemOrder(int seed, LayoutPattern pattern, List<AchievementID> achievements)
    {
        var itemCount = Mathf.Clamp(pattern.traversalItemCount, 1, traversalItemInfos.Count);
        var itemOrder = new List<MajorItem>();
        var unavailableItems = new List<MajorItem>(); //This list is added to as items are picked
        var localRandom = new MicrosoftRandom(seed);
        bool success = false;

        while (!success)
        {
            itemOrder.Clear();
            unavailableItems.Clear();

            for (int i = 0; i < itemCount; i++)
            {
                var environment = pattern.EnvironmentFromItemOrder(i);
                var itemList = new List<MajorItemInfo>();
                foreach (var kvp in traversalItemInfos)
                {
                    var item = kvp.Value;

                    //check if required achievement has been unlocked
                    if (achievements != null && item.requiredAchievement != AchievementID.None &&
                        !achievements.Contains(item.requiredAchievement))
                    {
                        continue;
                    }

                    if (unavailableItems.Contains(item.type)) continue;
                    if (item.restrictedEnvironments.Contains(environment)) continue;
                    if (item.finalItem != (i == itemCount - 1)) continue; //if this is the final item and i isn't or vice-versa continue

                    if (pattern.allowedGameModes == GameMode.MegaMap)
                    {
                        //Don't allow envionmentalResistance items when they might be used for a connection to surface or forest
                        if (i == 3)
                        {
                            if (item.environmentalResistance != EnvironmentalEffect.None) continue;
                        }

                        //avoid picking weapons that drastically deplete the pool in mega map
                        if (item.rendersUseless.Length >= 3)
                        {
                            //Does this item eliminate items that would eliminate fewer items?
                            var betterPicks = ItemManager.items.Values.Where(j => item.rendersUseless.Contains(j.type) && j.rendersUseless.Length < item.rendersUseless.Length).ToArray();
                            //if so continue if the item order doesn't contain any items whose type matches these better picks
                            if (betterPicks.Length > 0 && !itemOrder.Any(k => betterPicks.Any(l => l.type == k)))
                            {
                                continue;
                            }
                        }
                    }

                    itemList.Add(item);
                }

                if (itemList.Count <= 0)
                {
                    Debug.LogWarning("GetItemOrder ran out of items! " + itemOrder.Count + " items added. RETRY");
                    break;
                }

                MajorItemInfo chosenItem = itemList[localRandom.Range(0, itemList.Count)];

                itemOrder.Add(chosenItem.type);
                unavailableItems.Add(chosenItem.type);

                if (chosenItem.rendersUseless != null) { unavailableItems.AddRange(chosenItem.rendersUseless); }

                //Handle Buzzsaw Shell
                if (itemOrder.Contains(MajorItem.Dash) && (itemOrder.Contains(MajorItem.Slide) || itemOrder.Contains(MajorItem.Arachnomorph)))
                {
                    unavailableItems.Add(MajorItem.BuzzsawShell);
                }

                //Handle Kaboomerang
                if (!unavailableItems.Contains(MajorItem.Kaboomerang))
                {
                    var explosiveItems = new MajorItem[] { MajorItem.ExplosiveBolt, MajorItem.RocketLauncher };
                    var phaseItems = new MajorItem[] { MajorItem.Phaserang, MajorItem.RailGun, MajorItem.PhaseShot };
                    if (itemOrder.Any(item => explosiveItems.Contains(item)) &&
                       itemOrder.Any(item => phaseItems.Contains(item)))
                    {
                        unavailableItems.Add(MajorItem.Kaboomerang);
                    }
                }
            }

            success = itemOrder.Count == itemCount;
        }

        return itemOrder;
    }

    public List<TraversalCapabilities> GetTraversalCapabilities(List<MajorItem> itemOrder)
    {
        List<TraversalCapabilities> capabilities = new List<TraversalCapabilities>();
        var previousAbilities = new TraversalCapabilities()
        {
            damageTypes = DamageType.Generic,
            baseJumpHeight = Constants.startingMaxJumpHeight,
            waterJumpMod = Constants.startingWaterJumpMod,
            jumps = 1
        };

        previousAbilities.lastGainedAffordance = new TraversalRequirements();
        capabilities.Add(previousAbilities);

        foreach (var item in itemOrder)
        {
            var itemInfo = traversalItemInfos[item];
            var newCapabilites = new TraversalCapabilities(previousAbilities) { lastGainedAffordance = new TraversalRequirements() };
            
            var damageType = itemInfo.damageType;            
            if(damageType != 0 && !newCapabilites.damageTypes.HasFlag(damageType))
            {
                newCapabilites.damageTypes |= damageType;
                newCapabilites.lastGainedAffordance.requiredDamageType = damageType;
            }

            var envRes = itemInfo.environmentalResistance;
            if(envRes != 0 && !newCapabilites.environmentalResistance.HasFlag(envRes))
            {
                newCapabilites.environmentalResistance |= envRes;
                newCapabilites.lastGainedAffordance.requiredEnvironmentalResistance = envRes;
            }

            if (itemInfo.ignoreTerrain) { newCapabilites.shotIgnoresTerrain = true; }

            if(itemInfo.allowHovering)
            {
                var hoverHeight = itemInfo.hoverMaxVelocity * itemInfo.hoverTime;
                if (hoverHeight > newCapabilites.hoverJumpHeight) { newCapabilites.hoverJumpHeight = hoverHeight; }
            }

            switch(itemInfo.type)
            {
                case MajorItem.DoubleJump:
                    newCapabilites.jumps++;
                    break;
                case MajorItem.Infinijump:
                    newCapabilites.jumps = 10000;
                    break;
                case MajorItem.PowerJump:
                    newCapabilites.baseJumpHeight = 12;
                    break;
                case MajorItem.UpDog:
                    newCapabilites.baseJumpHeight = 13;
                    break;
                case MajorItem.Arachnomorph:
                    newCapabilites.canTraverseElevatedSmallGaps = true;
                    newCapabilites.canTraverseGroundedSmallGaps = true;
                    break;
                case MajorItem.BuzzsawShell:
                case MajorItem.Slide:
                    newCapabilites.canTraverseGroundedSmallGaps = true;
                    break;
                case MajorItem.PhaseShell:
                    newCapabilites.canPhaseThroughWalls = true;
                    break;
                case MajorItem.ViridianShell:
                    newCapabilites.canReverseGravity = true;
                    break;
                case MajorItem.DiveShell:
                    newCapabilites.waterJumpMod = 1;
                    break;
            }

            if(previousAbilities.effectiveJumpHeight != newCapabilites.effectiveJumpHeight)
            {
                newCapabilites.lastGainedAffordance.minEffectiveJumpHeight = previousAbilities.effectiveJumpHeight + 0.25f; //the plus 0.25f makes it require 4 pixels higher than the last
                newCapabilites.lastGainedAffordance.maxEffectiveJumpHeight = newCapabilites.effectiveJumpHeight;
            }

            if (previousAbilities.waterJumpHeight != newCapabilites.waterJumpHeight)
            {
                newCapabilites.lastGainedAffordance.minWaterJumpHeight = previousAbilities.waterJumpHeight + 0.25f; //the plus 0.25f makes it require 4 pixels higher than the last
                newCapabilites.lastGainedAffordance.maxWaterJumpHeight = newCapabilites.waterJumpHeight;
            }

            if (!previousAbilities.canTraverseGroundedSmallGaps && newCapabilites.canTraverseGroundedSmallGaps)
            {
                if (itemInfo.type != MajorItem.BuzzsawShell ||
                    previousAbilities.damageTypes.HasFlag(DamageType.Velocity)) //need better way to deal with dual traversal abilities
                {
                    newCapabilites.lastGainedAffordance.requiresGroundedSmallGaps = true;
                }

                if(previousAbilities.canPhaseThroughWalls)
                {
                    newCapabilites.lastGainedAffordance.requiresPhaseProof = true;
                }
            }

            if (!previousAbilities.shotIgnoresTerrain && newCapabilites.shotIgnoresTerrain)
            {
                newCapabilites.lastGainedAffordance.supportsShotIgnoresTerrain = true;
                if(newCapabilites.lastGainedAffordance.requiredDamageType == 0)
                {
                    newCapabilites.lastGainedAffordance.requiresShotIgnoresTerrain = true;
                }

                if (previousAbilities.canPhaseThroughWalls)
                {
                    newCapabilites.lastGainedAffordance.requiresPhaseProof = true;
                }
            }

            if(!previousAbilities.canPhaseThroughWalls && newCapabilites.canPhaseThroughWalls)
            {
                newCapabilites.lastGainedAffordance.requiresPhaseThroughWalls = true;
            }

            capabilities.Add(newCapabilites);
            previousAbilities = newCapabilites;
        }

        if(capabilities.Count != itemOrder.Count + 1)
        {
            Debug.Log("What the fuck! Capabilities aren't equal to item order count + 1");
        }

        return capabilities;
    }

    public RoomLayout GenerateStandardLayout(int seed, List<AchievementID> achievements = null)
    {
        stepByStep = false;
        foreach (var step in PopulateLayout(seed, new RoomLayout(), achievements, null, null, null, 1)) { }
        return _layout;
    }

    public IEnumerable ClassicBossRush(int seed, RoomLayout layout, List<AchievementID> achievements)
    {
        _layout = layout;
        if (!_initialized) { Initialize(achievements); }

        random = new MicrosoftRandom(seed);
        _layout.seed = seed;
        _layout.gameMode = GameMode.ClassicBossRush;
        var baseAbilities = new TraversalCapabilities() { damageTypes = DamageType.Generic, baseJumpHeight = Constants.startingMaxJumpHeight, jumps = 1 };

        bool layoutCompletedSuccessfully = false;

        while (!layoutCompletedSuccessfully)
        {            
            _layout.Initialize(RoomLayout.standardWidth, RoomLayout.standardHeight, 0, 0);
            layoutCompletedSuccessfully = true;

            var environments = new List<EnvironmentType>() { EnvironmentType.Surface};
            if (achievements.Contains(AchievementID.ForestSlums)) { environments.Add(EnvironmentType.ForestSlums); }
            environments.Add(EnvironmentType.Cave);
            if (achievements.Contains(AchievementID.CoolantSewers)) { environments.Add(EnvironmentType.CoolantSewers); }            
            environments.Add(EnvironmentType.Factory);
            if (achievements.Contains(AchievementID.CrystalMines)) { environments.Add(EnvironmentType.CrystalMines); }
            environments.Add(EnvironmentType.BuriedCity);
            environments.Add(EnvironmentType.BeastGuts);

            _layout.environmentLimits.Clear();
            foreach (EnvironmentType env in environments)
            {
                _layout.environmentLimits.Add(env, new Rect(0, 0, _layout.width, _layout.height));
            }

            RoomAbstract lastRoom = null;

            var items = Enum.GetValues(typeof(MajorItem)).Cast<MajorItem>().ToList().FindAll((i) => i != MajorItem.None && (ItemManager.items != null && ItemManager.items[i].bossRushPool));

            bool factorySr = random.value > 0.5f;
            bool forestMouthMeat = random.value > 0.5f;
            bool coolantWallCreep = random.value > 0.5f;
            bool crystalAdder = random.value > 0.5f;

            //3 bosses for each area
            foreach (var env in environments)
            {
                var bossRooms = new List<RoomInfo>(roomInfos[env].bossRoomInfos);
                var itemRooms = new List<RoomInfo>(roomInfos[env].majorItemRoomInfos);

                if((env == EnvironmentType.Cave && factorySr) || (env == EnvironmentType.Factory && !factorySr))
                {
                    bossRooms.RemoveAll((r) => r.boss == BossName.MouthMeatSenior);
                }
                
                if((env == EnvironmentType.Surface && forestMouthMeat) || (env == EnvironmentType.ForestSlums && !forestMouthMeat))
                {
                    bossRooms.RemoveAll((r) => r.boss == BossName.MouthMeat);
                }

                if ((env == EnvironmentType.Cave && coolantWallCreep) || (env == EnvironmentType.CoolantSewers && !coolantWallCreep))
                {
                    bossRooms.RemoveAll((r) => r.boss == BossName.WallCreep);
                }

                if((env == EnvironmentType.Factory && crystalAdder) || (env == EnvironmentType.CrystalMines && !crystalAdder))
                {
                    bossRooms.RemoveAll((r) => r.boss == BossName.FleshAdder);
                }

                var bosses = bossRooms.Count;

                for (int i = 0; i < bosses; i++)
                {
                    var roomInfo = PickRandom(itemRooms);
                    var item = PickRandom(items);
                    var info = ItemManager.items[item];
                    items.RemoveAll((x) => info.rendersUseless.Contains(x));
                    items.Remove(item);

                    if (_layout.roomAbstracts.Count == 0)
                    {
                        var itemAbstract = new RoomAbstract(roomInfo, new Int2D(_layout.width / 2, _layout.height / 2));
                        itemAbstract.isStartingRoom = true;
                        itemAbstract.majorItem = item;
                        _layout.Add(itemAbstract, random.ZeroToMaxInt());
                        lastRoom = itemAbstract;
                    }
                    else
                    {
                        var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();
                        RoomAbstract room = null;
                        while (room == null && directions.Count > 0)
                        {
                            var direction = PickRandom(directions);
                            directions.Remove(direction);
                            room = TryPlaceOffExistingRoom(lastRoom, direction.ToVector2(), new List<RoomInfo> { roomInfo }, baseAbilities, baseAbilities);
                        }

                        if(room == null)
                        {
                            layoutCompletedSuccessfully = false;
                            break;
                        }

                        room.majorItem = item;
                        lastRoom = room;
                        _layout.Add(room, random.ZeroToMaxInt());
                    }

                    if (!layoutCompletedSuccessfully) break;
                    if (stepByStep) yield return null;

                    //Boss Room
                    {
                        roomInfo = PickRandom(bossRooms);
                        bossRooms.Remove(roomInfo);

                        var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();
                        RoomAbstract room = null;
                        while (room == null && directions.Count > 0)
                        {
                            var direction = PickRandom(directions);
                            directions.Remove(direction);
                            room = TryPlaceOffExistingRoom(lastRoom, direction.ToVector2(), new List<RoomInfo> { roomInfo }, baseAbilities, baseAbilities);
                        }

                        if (room == null)
                        {
                            layoutCompletedSuccessfully = false;
                            break;
                        }

                        lastRoom = room;
                        _layout.Add(room, random.ZeroToMaxInt());
                    }

                    if (stepByStep) yield return null;
                }

                if (!layoutCompletedSuccessfully) break;
            }
        }
    }

    public IEnumerable PopulateLayout(int seed, RoomLayout layout, List<AchievementID> achievements, List<MajorItem> itemOrder = null, HashSet<MajorItem> itemsCollected = null, LayoutPattern chosenPattern = null, float glitchChance = 0)
    {
        _layout = layout;

        if (!_initialized) { Initialize(achievements); }

        if(achievements == null)
        {
            achievements = Enum.GetValues(typeof(AchievementID)).Cast<AchievementID>().ToList();
        }

        random = new MicrosoftRandom(seed);
        _layout.seed = seed;
        var glitchRoll = random.value; //in an attempt to keep random consistent between debug and real game
        _layout.hasGlitchWorld = _layout.gameMode == GameMode.Normal && glitchRoll < glitchChance;

        bool layoutCompletedSuccessfully = false;
        populateAttempts = 0;

        availableMinorItems = Enum.GetValues(typeof(MinorItemType)).Cast<MinorItemType>().Where((i) => i != MinorItemType.None).ToList();
        if (achievements != null)
        {
            if (!achievements.Contains(AchievementID.GlitchModule))
            {
                availableMinorItems.Remove(MinorItemType.GlitchModule);
            }

            if (!achievements.Contains(AchievementID.GlitchScrap))
            {
                availableMinorItems.Remove(MinorItemType.GlitchScrap);
            }
        }

        bool patternPassedIn = chosenPattern != null;

        while (!layoutCompletedSuccessfully)
        {
            populateAttempts++;

            if (populateAttempts % 10 == 0)
            {
                Debug.LogError("OH SHIT!");
                Debug.LogError("Attempted to build layout with seed (" + seed + ") 10+ times. Incrementing Seed!");
                Debug.LogError("OH SHIT!");
                seed++;
                yield return null;
            }

            if (!patternPassedIn)
            {
                var patterns = Resources.LoadAll<LayoutPattern>("LayoutPatterns/").Where(p => p.allowedGameModes.HasFlag(_layout.gameMode)).ToList();
                chosenPattern = patterns[random.Range(0, patterns.Count)];
                if (stepByStep) { Debug.Log("Chosen Pattern: " + chosenPattern.name); }
            }

            //Copy pattern for use
            var pattern = ScriptableObject.Instantiate(chosenPattern);
            if (!patternPassedIn) { chosenPattern = null; }
            _layout.Initialize(pattern);

            if (achievements.Contains(AchievementID.ForestSlums) && random.value > 0.5f)
            {
                pattern.SwapEnvironment(EnvironmentType.Surface, EnvironmentType.ForestSlums);
            }

            if (achievements.Contains(AchievementID.CoolantSewers) && random.value > 0.5f)
            {
                pattern.SwapEnvironment(EnvironmentType.Cave, EnvironmentType.CoolantSewers);
            }

            if (achievements.Contains(AchievementID.CrystalMines) && random.value > 0.5f)
            {
                pattern.SwapEnvironment(EnvironmentType.Factory, EnvironmentType.CrystalMines);
            }

            _layout.SetEnvironmentOrder(pattern);

            if (_layout.itemOrder == null || _layout.itemOrder.Count != pattern.traversalItemCount) //the pattern has changed, generate a new item order
            {
                _layout.itemOrder = itemOrder != null && itemOrder.Count == pattern.traversalItemCount ? itemOrder : GenerateItemOrder(seed, pattern, achievements);
                _layout.traversalCapabilities = GetTraversalCapabilities(_layout.itemOrder);
                _layout.password = SeedHelper.ParametersToKey(_layout.gameMode, _layout.seed, _layout.itemOrder, achievements);
            }

            var slot = SaveGameManager.activeSlot;

            pattern.Initialize(_layout, random, achievements.Contains(AchievementID.RevenantStation));
            _layout.patternID = pattern.id;

            availableNonTraversalItems = nonTraversalItems.FindAll((i) => (i.requiredAchievement == AchievementID.None || achievements.Contains(i.requiredAchievement)));
            var availableStartingItems = availableNonTraversalItems.FindAll(i => i.startingItemPool && (itemsCollected == null || itemsCollected.Contains(i.type)));

            //Remove items that have been assigned to certain segments by the layout pattern for the availableNonTraversalItems pool
            foreach (var seg in _layout.pattern.segments)
            {
                if (seg.specificItems != null) { availableNonTraversalItems.RemoveAll((i) => seg.specificItems.Contains(i.type)); }
            }

            if (_layout.gameMode != GameMode.MegaMap)
            {
                var startingItemsRandom = new MicrosoftRandom(seed - populateAttempts);

                float startingItemChance = (achievements.Contains(AchievementID.TheQuickening)) ? 0.7f : 0.4f;
                bool startingMinorItem = (achievements.Contains(AchievementID.ThoroughBot)) && startingItemsRandom.value < startingItemChance;
                bool startingActivatedItem = (achievements.Contains(AchievementID.FastBot)) && startingItemsRandom.value < startingItemChance;
                bool startingPassiveItem = (achievements.Contains(AchievementID.FightBot)) && startingItemsRandom.value < startingItemChance;
                bool startingFollower = (achievements.Contains(AchievementID.OrbBot)) && startingItemsRandom.value < startingItemChance;
                bool startingWeapon = (achievements.Contains(AchievementID.GunBot)) && startingItemsRandom.value < startingItemChance;

                if (startingMinorItem)
                {
                    _layout.startingMinorItem = availableMinorItems[startingItemsRandom.Range(0, availableMinorItems.Count)];
                }

                if (startingActivatedItem)
                {
                    var possibleItems = availableStartingItems.FindAll((i) => !_layout.allNonTraversalItemsAdded.Contains(i.type) && i.isActivatedItem);
                    if (possibleItems.Count > 0)
                    {
                        var item = possibleItems[startingItemsRandom.Range(0, possibleItems.Count)].type;
                        _layout.startingActivatedItem = item;
                        _layout.allNonTraversalItemsAdded.Add(item);
                        _layout.totalStartingMajorItems++;
                    }
                }

                if (startingPassiveItem)
                {
                    var possibleItems = availableStartingItems.FindAll((i) => !_layout.allNonTraversalItemsAdded.Contains(i.type) && !i.isEnergyWeapon && !i.isActivatedItem && !i.follower);
                    if (possibleItems.Count > 0)
                    {
                        var item = possibleItems[startingItemsRandom.Range(0, possibleItems.Count)].type;
                        _layout.startingPassiveItem = item;
                        _layout.allNonTraversalItemsAdded.Add(item);
                        _layout.totalStartingMajorItems++;
                        //Debug.Log("Starting Passive Item: " + item.ToString());
                    }
                }

                if (startingFollower)
                {
                    var possibleItems = availableStartingItems.FindAll((i) => !_layout.allNonTraversalItemsAdded.Contains(i.type) && i.follower);
                    if (possibleItems.Count > 0)
                    {
                        var item = possibleItems[startingItemsRandom.Range(0, possibleItems.Count)].type;
                        _layout.startingFollower = item;
                        _layout.allNonTraversalItemsAdded.Add(item);
                        _layout.totalStartingMajorItems++;
                        //Debug.Log("Starting Follower Item: " + item.ToString());
                    }
                }

                if (startingWeapon)
                {
                    var possibleItems = availableStartingItems.FindAll((i) => !_layout.allNonTraversalItemsAdded.Contains(i.type) && i.isEnergyWeapon);
                    if (possibleItems.Count > 0)
                    {
                        var item = possibleItems[startingItemsRandom.Range(0, possibleItems.Count)].type;
                        _layout.startingWeapon = item;
                        _layout.allNonTraversalItemsAdded.Add(item);
                        _layout.totalStartingMajorItems++;
                        //Debug.Log("Starting Weapon Item: " + item.ToString());
                    }
                }
            }

            var startingAbilities = _layout.traversalCapabilities[0];

            if (stepByStep) yield return null;

#region Add Layout Segments
            var segmentAddedSuccsessfully = false;
            LayoutGenerationSegment lastSegment = null;

            for (int g = 0; g < pattern.segments.Count; g++)
            {
                var segment = pattern.segments[g];

                SetupLayoutGenerationSegment(segment, lastSegment, startingAbilities);                

                // Where  AddSegment method would start
                foreach (var step in CreateLayoutGenerationSegment(segment))
                {
                    segmentAddedSuccsessfully = step;
                    if (stepByStep) yield return null;
                    if (!segmentAddedSuccsessfully) break;
                }

                lastSegment = segment;
                if (!segmentAddedSuccsessfully) break;
            }

            if (!segmentAddedSuccsessfully)
            {
                Debug.LogError("Layout could not be completed because segment (" + lastSegment.environmentType + ") could not be added. Password: " + _layout.password + " Making another attempt.");
                layoutCompletedSuccessfully = false;
                continue;
            }
            #endregion
            #region FinalBoss
            IEnumerable<bool> bossRoutine = null;
            if (layout.pattern.hasMegaBeast) { bossRoutine = AddMegaBeast(); }
            else if (layout.pattern.hasBeastRemnants) { bossRoutine = AddBeastRemnants(); }

            if (bossRoutine != null)
            {
                var mbSuccess = true;
                foreach (var step in bossRoutine)
                {
                    if (!step)
                    {
                        mbSuccess = false;
                        break;
                    }

                    if (stepByStep) yield return null;
                }

                if (!mbSuccess)
                {
                    Debug.LogError("Layout could not be completed because Mega Beast room could not be added. Making another attempt.");
                    layoutCompletedSuccessfully = false;
                    continue;
                }
            }
#endregion

#region Connect Environments
            var success = true;
            foreach (var connection in pattern.segmentsToConnect)
            {
                foreach (var step in ConnectSegments(connection))
                {
                    if (!step)
                    {
                        success = false;
                        break;
                    }

                    if (stepByStep) yield return null;
                }

                if (stepByStep) yield return null;
                if (!success) break;
            }
#endregion

            if (!success)
            {
                //if (stepByStep) Debug.Break();
                Debug.LogError("Layout could not be completed because environments could not be connected. Making another attempt.");
                layoutCompletedSuccessfully = false;
                continue;
            }

            foreach (var step in AddTeleporters()) { if (stepByStep) yield return null; }
            foreach (var step in AddBonusItems()) { if (stepByStep) yield return null; }
            foreach (var step in AddShops(achievements)) { if (stepByStep) yield return null; }
            foreach (var step in AddShrines()) { if (stepByStep) yield return null; }
            foreach (var step in AddMinorItems()) { if (stepByStep) yield return null; }

            //Double Check Items and Assign Door IDs
#region Final Checks
            layoutCompletedSuccessfully = true;

            if(_layout.allNonTraversalItemsAdded.Distinct().Count() != _layout.allNonTraversalItemsAdded.Count)
            {
                layoutCompletedSuccessfully = false;
                Debug.LogError("!Duplicate Non-traversal Items!");
            }

            if (stepByStep) { Debug.Log("Double Checking Achievements and Items"); }
            foreach (var item in _layout.allNonTraversalItemsAdded)
            {
                var achievement = ItemManager.items[item].requiredAchievement;
                if (achievement != AchievementID.None && !achievements.Contains(achievement))
                {
                    Debug.LogError("Item added to layout without the necessary achievement: " + achievement);
                    layoutCompletedSuccessfully = false;
                    break;
                }
            }

            _layout.roomAbstracts.ForEach(SmartConnectRooms);
            ReplaceRooms();

            if (stepByStep) { Debug.Log("PostProcessing Rooms"); }

            if (layoutCompletedSuccessfully)
            {
                layoutCompletedSuccessfully = FinishLayout();
            }
#endregion

            if (layoutCompletedSuccessfully)
            {
                Debug.Log("Layout completed successfully after " + populateAttempts + " attempts!");
            }
            else if (stepByStep)
            {
                Debug.Break();
            }
        }
    }

    private IEnumerable<bool> AddMegaBeast()
    {
        var megaBeastRooms = roomInfos[_layout.environmentOrder[0]].megaBeastRoomInfos;
        var megaBeastRoomInfo = PickRandom(megaBeastRooms);
        var zone = _layout.pattern.finalBossZone;
        var megaBeastPos = new Int2D((int)random.Range(zone.xMin, zone.xMax), (int)random.Range(zone.yMin, zone.yMax));
        var finalCapabilities = _layout.traversalCapabilities[_layout.traversalCapabilities.Count - 1];

        RoomAbstract megaBeastRoom = null;
        var rect = megaBeastRoomInfo.GetRectAtGridPosition(megaBeastPos);

        if (_layout.MovePositionAwayFromRooms(ref rect, new Buffer2D(0, 2, 1, 1), Vector2.up, _layout.environmentLimits[_layout.environmentOrder[0]]))
        {
            megaBeastPos = Int2D.GetRoomPosFromRect(rect);
            if (_layout.ValidRoomPlacement(megaBeastPos, megaBeastRoomInfo.size))
            {
                megaBeastRoom = _layout.Add(new RoomAbstract(megaBeastRoomInfo, megaBeastPos), random.ZeroToMaxInt());
                megaBeastRoom.expectedCapabilitiesIndex = _layout.traversalCapabilities.Count - 1;
            }
        }

        if (megaBeastRoom == null)
        {
            Debug.LogError("Layout could not be completed because mega beast room could not be placed. Making another attempt.");
            yield return false;
        }

        RoomAbstract megaBeastPathStart = null;
        var center = _layout.environmentLimits[_layout.environmentOrder[0]].center.x;
        var mBRoomCenter = megaBeastRoom.nonDiscreteCenter;
        var left = megaBeastRoom.gridCenter.x < center;
        var pastCenter = false;
        float distance = _layout.width * _layout.height;
        //find the branch room furthest to the right or left
        foreach (var room in _layout.roomAbstracts)
        {
            if (room.assignedRoomInfo.environmentType == _layout.environmentOrder[0] &&
                room.assignedRoomInfo.HasAllPossibleExitsForSize())
            {
                var rCenter = room.nonDiscreteCenter;
                var rDirstance = Vector2.Distance(rCenter, mBRoomCenter);
                var pCent = left ? room.nonDiscreteCenter.x < center : room.nonDiscreteCenter.x > center;
                if (!pastCenter || pCent)
                {
                    if (rDirstance < distance)
                    {
                        distance = rDirstance;
                        megaBeastPathStart = room;
                        if (pCent) { pastCenter = true; }
                    }
                }
            }
        }

        if (stepByStep) yield return true;

        var pathFinder = new PathFinder();
        var e = megaBeastRoom.GetClosestGlobalPositionWithExit(megaBeastPathStart.gridCenter);
        var s = megaBeastPathStart.GetClosestGlobalPositionWithExit(e, 2);

        path = pathFinder.GetPath(_layout, s, e, (r, p) => PathValidate(r, p, megaBeastPathStart, megaBeastRoom));

        if (path == null || path.Count == 0) { Debug.LogError("Pathfinder could not find path to Megabeast Room"); }

        if (stepByStep) { yield return true; }

        bool megaBeastPathSuccess = path != null && path.Count > 0;
        if (megaBeastPathSuccess)
        {
            foreach (var step in AddRoomsToPath(path, _layout.environmentOrder[0], finalCapabilities, true))
            {
                if (!step)
                {
                    Debug.LogError("Couldn't path from " + megaBeastPathStart.gridPosition + " to MegaBeast at " + e);
                    megaBeastPathSuccess = false;
                    break;
                }

                if (stepByStep) yield return true;
            }
        }

        if (!megaBeastPathSuccess)
        {
            //if (stepByStep) Debug.Break();
            Debug.LogError("Layout could not be completed because surface couldn't link to MegaBeast Room. Making another attempt.");
            yield return false;
        }

        //Make Megabeast Door Hard to Enter
        var megaExit = megaBeastRoom.exits.FirstOrDefault();
        var megaConnect = _layout.GetConnectedExit(megaExit);
        var connectRoom = _layout.GetRoomAtPositon(megaConnect.globalGridPosition);
        var megaConInfo = connectRoom.assignedRoomInfo.possibleExits.Find(exit => exit.direction == megaConnect.direction && exit.localGridPosition == megaConnect.localGridPosition);
        if (megaConInfo != null)
        {
            var dTypes = megaConInfo.toExit.supportedDamageTypes;
            var lastCap = _layout.traversalCapabilities.LastOrDefault(c =>
            {
                if (c.lastGainedAffordance == null) return false;
                var d = c.lastGainedAffordance.requiredDamageType;
                return d != 0 && d != DamageType.Generic && dTypes.HasFlag(d);
            });

            if (lastCap != null)
            {
                var dType = lastCap.lastGainedAffordance.requiredDamageType;
                megaConnect.toExit.requiredDamageType = dType;
            }
        }
    }

    private IEnumerable<bool> AddBeastRemnants()
    {
        var env = EnvironmentType.BuriedCity;
        var roomInfo = roomInfos[env].otherSpecialRoomsInfos.Where(r => r.boss == BossName.BeastRemnants).First();
        var zone = _layout.pattern.finalBossZone;
        var position = new Int2D((int)random.Range(zone.xMin, zone.xMax), (int)random.Range(zone.yMin, zone.yMax));
        var finalCapabilities = _layout.traversalCapabilities[_layout.traversalCapabilities.Count - 1];

        RoomAbstract beastRoom = null;
        var rect = roomInfo.GetRectAtGridPosition(position);

        if (_layout.MovePositionAwayFromRooms(ref rect, new Buffer2D(0, 2, 1, 1), Vector2.up, _layout.environmentLimits[env]))
        {
            position = Int2D.GetRoomPosFromRect(rect);
            if (_layout.ValidRoomPlacement(position, roomInfo.size))
            {
                beastRoom = _layout.Add(new RoomAbstract(roomInfo, position), random.ZeroToMaxInt());
                beastRoom.expectedCapabilitiesIndex = _layout.traversalCapabilities.Count - 1;
            }
        }

        if (beastRoom == null)
        {
            Debug.LogError("Layout could not be completed because mega beast room could not be placed. Making another attempt.");
            yield return false;
        }

        RoomAbstract beastPathStart = null;
        var center = _layout.environmentLimits[env].center.x;
        var roomCenter = beastRoom.nonDiscreteCenter;
        float bestDistance = _layout.width * _layout.height;

        //find the branch room closest to position
        foreach (var room in _layout.roomAbstracts)
        {
            if (room.assignedRoomInfo.environmentType == env &&
                room.assignedRoomInfo.HasAllPossibleExitsForSize())
            {
                var rCenter = room.nonDiscreteCenter;
                var distance = Vector2.Distance(rCenter, roomCenter);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    beastPathStart = room;
                }
            }
        }

        if (stepByStep) yield return true;

        var pathFinder = new PathFinder();
        var e = beastRoom.GetClosestGlobalPositionWithExit(beastPathStart.gridCenter);
        var s = beastPathStart.GetClosestGlobalPositionWithExit(e, 2);

        path = pathFinder.GetPath(_layout, s, e, (r, p) => PathValidate(r, p, beastPathStart, beastRoom));

        if (path == null || path.Count == 0) { Debug.LogError("Pathfinder could not find path to Megabeast Room"); }

        if (stepByStep) { yield return true; }

        bool pathSuccess = path != null && path.Count > 0;
        if (pathSuccess)
        {
            foreach (var step in AddRoomsToPath(path, _layout.environmentOrder[0], finalCapabilities, true))
            {
                if (!step)
                {
                    Debug.LogError("Couldn't path from " + beastPathStart.gridPosition + " to MegaBeast at " + e);
                    pathSuccess = false;
                    break;
                }

                if (stepByStep) yield return true;
            }
        }

        if (!pathSuccess)
        {
            //if (stepByStep) Debug.Break();
            Debug.LogError("Layout could not be completed because " + env + " couldn't link to Final Boss Room. Making another attempt.");
            yield return false;
        }
    }

    public bool FinishLayout()
    {
        int permObjID = 0;
        int doorID = 0;

        var layoutCompletedSuccessfully = true;

        foreach (var room in _layout.roomAbstracts)
        {
            var result = room.assignedRoomInfo.MatchesAbstract(room);
            
            if(room.exits.Count == 0)
            {
                throw new Exception ("Seed (" + _layout.seed + "): room.assignedRoomInfo (" + room.assignedRoomInfo.sceneName + ") at " + room.gridPosition + " has no exits!");
            }

            if (result != RoomMatchResult.Success)
            {
                layoutCompletedSuccessfully = false;
                Debug.LogError("Seed (" + _layout.seed + "): room.assignedRoomInfo (" + room.assignedRoomInfo.sceneName + ") at " + room.gridPosition + " has something wrong with it!");
                Debug.LogError(result.ToString());

                if (result == RoomMatchResult.MinorItemMismatch)
                {
                    Debug.LogError("Minor Item Along Path?: " + room.assignedRoomInfo.minorItemAlongPathOnly);
                    Debug.LogError("Minor Item Locations Count? " + room.assignedRoomInfo.minorItemLocations.Count);
                }
            }

            if (room.assignedRoomInfo.permanentStateObjectCount > 0)
            {
                room.permanentStateObjectGlobalIds = new List<int>();
                for (int i = 0; i < room.assignedRoomInfo.permanentStateObjectCount; i++)
                {
                    room.permanentStateObjectGlobalIds.Add(permObjID);
                    permObjID++;
                }
            }

            var environmentType = room.assignedRoomInfo.environmentType;
            if (environmentType == EnvironmentType.BuriedCity && (room.environmentalEffect == EnvironmentalEffect.None || room.environmentalEffect == EnvironmentalEffect.Fog))
            {
                room.light = 0.5f;
            }
            else if (environmentType != EnvironmentType.Surface &&
                environmentType != EnvironmentType.ForestSlums &&
                environmentType != EnvironmentType.BeastGuts &&
                environmentType != EnvironmentType.Glitch)
            {
                room.light = random.Range(0.8f, 1);
            }

            if ((_layout.pattern && _layout.pattern.chanceToConnectNeighborRooms > 0) ||
                environmentType == EnvironmentType.BeastGuts ||
                environmentType == EnvironmentType.Glitch)
            {
                TryConnectRoomToNeighborsHaphazard(room);
            }

            #region Safety Checks
#if DEBUG && !UNITY_SWITCH
            if (room.HasUnusedSpaces())
            {
                if (room.assignedRoomInfo.boss == BossName.WallCreep ||
                    (room.assignedRoomInfo.traversalPaths.Count > 0 && (room.width * room.height) == 2 && room.HasUsableUnusedExitsInUnusedSpaces()))
                {
                    Debug.LogError("Seed (" + _layout.password + "): " + room.assignedRoomInfo.sceneName + " has unused spaces and really shouldn't!");
                    if (room.wasReplaced)
                    {
                        Debug.LogError("Seed (" + _layout.password + "): " + room.assignedRoomInfo.sceneName + " has unused spaces and really shouldn't!");
                    }
                }
            }

            if (room.assignedRoomInfo.environmentType != EnvironmentType.BeastGuts && room.exits.Count <= 1 && room.assignedRoomInfo.roomType == RoomType.None)
            {
                Debug.LogError("Seed (" + _layout.password + "): " + room.assignedRoomInfo.sceneName + " is a dead end! Branch Room: " + room.branchRoom);
            }

            //Check for bad traversal path rooms
            for (int i = 0; i < room.traversalPathRequirements.Count; i++)
            {
                var travPathReq = room.traversalPathRequirements[i];
                if (!travPathReq.CapabilitesSufficient(room.expectedCapabilities))
                {
                    //check to see if there are doors on both sides of the patch that lead to rooms that have the same expected capabilities as this room
                    var p = room.assignedRoomInfo.traversalPaths[i];
                    var toSameExCap = false;
                    var fromSameExCap = false;
                    foreach (var ex in room.exits)
                    {
                        var target = _layout.GetRoomAtPositon(ex.TargetPosition());
                        if (target.expectedCapabilitiesIndex == room.expectedCapabilitiesIndex)
                        {
                            if (!toSameExCap) { toSameExCap = p.to.AllGridPositions().Any(gp => gp == ex.localGridPosition); }
                            if (!fromSameExCap) { fromSameExCap = p.from.AllGridPositions().Any(gp => gp == ex.localGridPosition); }
                        }

                        if (toSameExCap && fromSameExCap)
                        {
                            throw new Exception("Layout with password " + _layout.password + " has expected capabilities that are insufficient in room " + room.assignedRoomInfo.sceneName + " at " + room.gridPosition);
                        }
                    }
                }
            }
#endif
            #endregion

            bool connectedToBossRoom = false;
            bool allExitsToDarkness = true;
            bool allExitsToHeat = true;
            bool allExitsToConfusion = true;
            bool allExitsToWater = true;
            foreach (var exit in room.exits)
            {
                var destination = _layout.GetRoomAtPositon(exit.TargetPosition());
                #region Safety Checks
#if DEBUG && !UNITY_SWITCH
                if (destination == null)
                {
                    throw new Exception("Layout with password " + _layout.password + " has an exit to nowhere in room at " + exit.globalGridPosition);
                }

                if (_layout.gameMode != GameMode.BossRush &&
                    destination.assignedRoomInfo.environmentType != environmentType && destination.assignedRoomInfo.roomType != RoomType.TransitionRoom &&
                    destination.assignedRoomInfo.roomType != RoomType.StartingRoom && room.assignedRoomInfo.roomType != RoomType.TransitionRoom &&
                    room.assignedRoomInfo.roomType != RoomType.StartingRoom)
                {
                    throw new Exception("Layout with password " + _layout.password + " has a room that connects to an environment it shouldn't at " + room.gridPosition + ".");
                }

                if (!destination.HasExistingExitToGridPosition(exit.globalGridPosition))
                {
                    throw new Exception("Layout with password " + _layout.password + " has an exit with no reciprocal exit at " + exit.globalGridPosition);
                }

                foreach (var minorItem in room.minorItems)
                {
                    if (minorItem.spawnInfo.conflictingExits.Any(p => p.direction == exit.direction && p.position == exit.localGridPosition))
                    {
                        throw new Exception("Layout with password " + _layout.password + " has an exit that conflicts with a minor item at " + exit.globalGridPosition);
                    }
                }
#endif
                #endregion

                if (destination != null)
                {
                    if (destination.environmentalEffect == EnvironmentalEffect.Darkness)
                    {
                        room.light = 0.5f;
                    }
                    else
                    {
                        allExitsToDarkness = false;
                    }

                    if (destination.environmentalEffect == EnvironmentalEffect.Heat)
                    {
                        exit.toHeatRoom = true;
                    }
                    else
                    {
                        allExitsToHeat = false;
                    }

                    if (destination.environmentalEffect == EnvironmentalEffect.Confusion)
                    {
                        exit.toConfusionRoom = true;
                    }
                    else
                    {
                        allExitsToConfusion = false;
                    }

                    if (destination.environmentalEffect == EnvironmentalEffect.Underwater)
                    {
                        exit.toWaterRoom = true;
                    }
                    else
                    {
                        allExitsToWater = false;
                    }


                    if (destination.assignedRoomInfo.environmentType != EnvironmentType.Glitch &&
                        (destination.assignedRoomInfo.boss != BossName.None ||
                        destination.assignedRoomInfo.roomType == RoomType.MegaBeast))
                    {
                        exit.toBossRoom = true;
                        if (_layout.gameMode != GameMode.BossRush)
                        {
                            connectedToBossRoom = true;
                            room.preBossRoom = destination.assignedRoomInfo.boss;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No room for exit " + exit.id + " in room " + room.assignedRoomInfo.sceneName + " at " + room.gridPosition);
                }

                exit.id = doorID;
                doorID++;
            }

            if (room.assignedRoomInfo.roomType == RoomType.None ||
                room.assignedRoomInfo.roomType == RoomType.Shrine ||
                room.assignedRoomInfo.roomType == RoomType.TransitionRoom)
            {
                if (allExitsToDarkness) { room.environmentalEffect = EnvironmentalEffect.Darkness; }
                if (allExitsToConfusion) { room.environmentalEffect = EnvironmentalEffect.Confusion; }
                if (allExitsToHeat) { room.environmentalEffect = EnvironmentalEffect.Heat; }
                if (allExitsToWater) { room.environmentalEffect = EnvironmentalEffect.Underwater; }
            }
#if DEBUG && !UNITY_SWITCH
            if (room.preBossRoom != BossName.None && !connectedToBossRoom)
            {
                Debug.LogError("A room not attached to a boss room is flagged as a pre-boss room");
            }
#endif
        }

        return layoutCompletedSuccessfully;
    }

    public void ReplaceRooms()
    {
        //rooms we can replace
        var canReplace = new Dictionary<string, List<RoomAbstract>>();
        //rooms used in the abstract
        var usedRooms = new HashSet<string>();

        //create a list of rooms that can be replaced and a list of all rooms used
        foreach (var a in _layout.roomAbstracts)
        {
            usedRooms.Add(a.assignedRoomInfo.sceneName);

            if (a.assignedRoomInfo.traversalPaths.Count > 1) continue;

            if (a.assignedRoomInfo.environmentType != EnvironmentType.Glitch &&
                a.assignedRoomInfo.roomType == RoomType.None)
            {
                if (!canReplace.ContainsKey(a.assignedRoomInfo.sceneName))
                {
                    canReplace[a.assignedRoomInfo.sceneName] = new List<RoomAbstract>();
                }

                canReplace[a.assignedRoomInfo.sceneName].Add(a);
            }
        }

        //a list of rooms we should replace
        var shouldReplace = new List<RoomAbstract>();
        //a list of rooms added to shouldReplace because they've been used more than once
        var doubles = new HashSet<string>();
        //a list of rooms added to shouldReplace because they're all exit rooms that only use 2 exits
        var blands = new HashSet<string>();
        //create a list of environments used by rooms added to shouldReplace
        var environments = new HashSet<EnvironmentType>();

        //create a list of rooms that 
        foreach (var kvp in canReplace)
        {
            if (kvp.Value.Count > 1)
            {
                //Debug.Log("General Room " + kvp.Key + " appears more than once!");
                var room = kvp.Value[random.Range(0, kvp.Value.Count)];
                shouldReplace.Add(room);
                doubles.Add(room.roomID);
                environments.Add(room.assignedRoomInfo.environmentType);
            }
            else
            {
                var room = kvp.Value.First();

                if (room.exits.Count == 2 &&
                    room.traversalPathRequirements.Count == 0 &&
                    room.assignedRoomInfo.HasAllPossibleExitsForSize())
                {
                    shouldReplace.Add(room);
                    blands.Add(room.roomID);
                    environments.Add(room.assignedRoomInfo.environmentType);
                }
            }
        }

        if (shouldReplace.Count <= 0)
        {
            //Debug.Log("Found no rooms to replace.");
            return;
        }

        //Debug.Log("Found " + shouldReplace.Count + " rooms to potentially replace.");
        foreach (var env in environments)
        {
            //a list of rooms we haven't used
            var replacements = roomInfos[env].generalRoomInfos.Where(r => !usedRooms.Contains(r.sceneName)).ToList();
            //Debug.Log(replacements.Count + " potential replacements for " + env.ToString());

            //rooms that share this environment
            var roomsToReplace = shouldReplace.Where(r => r.assignedRoomInfo.environmentType == env);

            foreach (var roomAbstract in roomsToReplace)
            {
                //create pool;
                var pool = new List<RoomInfo>();
                var capabilities = roomAbstract.expectedCapabilities;
                var bland = blands.Contains(roomAbstract.roomID);

                foreach (var info in replacements)
                {
                    //don't replace a bland room with another bland room just because
                    if (bland && info.traversalPaths.Count == 0 &&
                        !info.traversalLimitations.requiresElevatedSmallGaps &&
                        info.HasAllPossibleExitsForSize())
                    {
                        continue;
                    }

                    if (roomAbstract.traversalPathRequirements.Count == 1 && info.traversalPaths.Count == 1)
                    {
                        var existingPath = roomAbstract.assignedRoomInfo.traversalPaths[0];
                        var newPath = info.traversalPaths[0];
                        if (existingPath.reciprocal != newPath.reciprocal) continue;
                        if (existingPath.to.minGridPosition != newPath.to.minGridPosition) continue;
                        if (existingPath.to.maxGridPosition != newPath.to.maxGridPosition) continue;
                        if (existingPath.from.minGridPosition != newPath.from.minGridPosition) continue;
                        if (existingPath.from.maxGridPosition != newPath.from.maxGridPosition) continue;
                    }

                    var result = info.MatchesAbstract(roomAbstract);

                    //allow traversal path room replacements if they'd work
                    if (result == RoomMatchResult.TraversalPathRequirementMismatch)
                    {                        
                        if (roomAbstract.traversalPathRequirements.Count == 0 &&
                            !roomAbstract.HasUnusedSpaces() &&
                            info.traversalPaths.Count == 1 &&
                            info.traversalPaths[0].limitations.CapabilitesSufficient(capabilities, roomAbstract.environmentalEffect))
                        {
                            result = RoomMatchResult.Success;
                        }
                    }

                    if (result == RoomMatchResult.Success)
                    {
                        if (info.traversalLimitations.CapabilitesSufficient(capabilities, roomAbstract.environmentalEffect))
                        {
                            pool.Add(info);
                        }
                    }
                }

                if (pool.Count > 0)
                {
                    var pick = pool[random.Range(0, pool.Count)];
                    roomAbstract.assignedRoomInfo = pick;
                    roomAbstract.wasReplaced = true;
                    replacements.Remove(pick);

                    //deal with traversal path room replacements
                    if (roomAbstract.traversalPathRequirements.Count == 0 &&
                        pick.traversalPaths.Count == 1 &&
                        pick.traversalPaths[0].limitations.CapabilitesSufficient(capabilities, roomAbstract.environmentalEffect))
                    {
                        roomAbstract.traversalPathRequirements.Add(GetRandomSuitableRequirements(capabilities, pick.traversalPaths[0].limitations, roomAbstract.environmentalEffect));
                    }
                }
            }
        }
    }

    //tries to connect dead ends to neighbors
    public void SmartConnectRooms(RoomAbstract fromRoom)
    {
        if (fromRoom == null || fromRoom.assignedRoomInfo == null) return;
        if (fromRoom.assignedRoomInfo.environmentType == EnvironmentType.Surface) return;
        if (fromRoom.assignedRoomInfo.boss == BossName.MegaBeastCore) return;
        if (fromRoom.assignedRoomInfo.boss == BossName.BeastRemnants) return;

        var layout = fromRoom.layout;

        if (fromRoom.traversalPathRequirements.Count > 0) return;
        if (fromRoom.majorItem != 0 && layout.itemOrder.Contains(fromRoom.majorItem)) return;
        
        //look for rooms with 1 exit connected to other rooms with 1 exit
        if (fromRoom.exits.Count > 1) return;

        var unusedExits = fromRoom.GetAllUnusedExits();
        
        foreach (var exit in unusedExits)
        {
            if (fromRoom.minorItems.Any(i => i.spawnInfo.conflictingExits.Any(e => e == exit.posDir))) continue;
            var targetPos = fromRoom.GetExitTarget(exit);
            var toRoom = layout.GetRoomAtPositon(targetPos);

            if (toRoom == null || toRoom.assignedRoomInfo == null) continue;
            if (toRoom.majorItem != 0) continue;
            if (toRoom.assignedRoomInfo.roomType == RoomType.MegaBeast) continue;
            if (toRoom.assignedRoomInfo.boss == BossName.MegaBeastCore) continue;
            if (toRoom.assignedRoomInfo.boss == BossName.BeastRemnants) continue;

            var fromRoomExpectedCap = fromRoom.GetExpectedCapabilitiesIndex(exit.localGridPosition);
            var toRoomExpectedCap = toRoom.GetExpectedCapabilitiesIndex(toRoom.GetLocalPosition(targetPos));

            if (toRoom.traversalPathRequirements.Count > 0 && toRoomExpectedCap != fromRoomExpectedCap) continue;            
            if (toRoom.assignedRoomInfo.environmentType != fromRoom.assignedRoomInfo.environmentType) continue;
            if (toRoom.environmentalEffect != fromRoom.environmentalEffect && toRoomExpectedCap != fromRoomExpectedCap)
            {
                if (toRoom.environmentalEffect.RequiresTraversalAbility() || fromRoom.environmentalEffect.RequiresTraversalAbility()) continue;
            }
            
            //does this room already have more than 1 exit in the target space?
            if (toRoom.exits.FindAll((e) => e.globalGridPosition == targetPos).Count > 1) continue;

            var toRoomUsableExits = toRoom.GetUsableUnusedExits(fromRoom);
            if (toRoomUsableExits.Count <= 0) continue;

            var entrance = toRoomUsableExits.Find(e => e.direction == exit.direction.Opposite() && e.localGridPosition == toRoom.GetLocalPosition(targetPos));

            if (entrance == null) continue;

            //Find capabilities
            TraversalCapabilities capabilities;
            ExitAbstract exitRequirements;
            ExitAbstract entranceRequirement;

            if (fromRoomExpectedCap == toRoomExpectedCap)
            {
                capabilities = layout.traversalCapabilities[0];
                exitRequirements = new ExitAbstract(exit, fromRoom, GetRandomSuitableRequirements(capabilities, exit.toExit, fromRoom.environmentalEffect));
                entranceRequirement = new ExitAbstract(entrance, toRoom, GetRandomSuitableRequirements(capabilities, entrance.toExit, toRoom.environmentalEffect));
            }
            else if (fromRoomExpectedCap > toRoomExpectedCap)
            {
                capabilities = layout.traversalCapabilities[fromRoomExpectedCap];
                exitRequirements = new ExitAbstract(exit, fromRoom, GetRandomSuitableRequirements(capabilities, exit.toExit, fromRoom.environmentalEffect));
                entranceRequirement = new ExitAbstract(entrance, toRoom, capabilities.lastGainedAffordance);
            }
            else
            {
                capabilities = layout.traversalCapabilities[toRoomExpectedCap];
                exitRequirements = new ExitAbstract(exit, fromRoom, capabilities.lastGainedAffordance);
                entranceRequirement = new ExitAbstract(entrance, toRoom, GetRandomSuitableRequirements(capabilities, entrance.toExit, toRoom.environmentalEffect));
            }

            if (exit.CanSupportExitAbstract(exitRequirements) && entrance.CanSupportExitAbstract(entranceRequirement))
            {
                fromRoom.exits.Add(exitRequirements);
                toRoom.exits.Add(entranceRequirement);
            }
        }
    }

    public void TryConnectRoomToNeighborsHaphazard(RoomAbstract room)
    {
        if (room.assignedRoomInfo == null ||
           (room.assignedRoomInfo.roomType == RoomType.ItemRoom && room.majorItem != 0))
        {
            return;
        }

        var unusedExits = room.GetAllUnusedExits();
        var layout = room.layout;
        var pattern = layout.pattern;
        var environment = room.assignedRoomInfo.environmentType;

        foreach (var exit in unusedExits)
        {
            if (room.minorItems.Any(i => i.spawnInfo.conflictingExits.Any(e => e == exit.posDir))) continue;

            var targetPos = room.GetExitTarget(exit);
            var targetRoom = layout.GetRoomAtPositon(targetPos);
            TraversalCapabilities capabilities;
            if (targetRoom == null || !AllowHaphazardConnection(room, targetRoom, out capabilities)) continue;

            var usableExits = targetRoom.GetUsableUnusedExits(room);
            if (usableExits.Count <= 0) continue;
            var entrance = usableExits.Find(e => e.direction == exit.direction.Opposite() && e.localGridPosition == targetRoom.GetLocalPosition(targetPos));

            bool alwaysConnect = (environment == EnvironmentType.Glitch || environment == EnvironmentType.BeastGuts) &&
                targetRoom.assignedRoomInfo.roomType == RoomType.None &&
                !room.IsConnectedTo(targetRoom);

            if (entrance != null && (alwaysConnect || random.value < pattern.chanceToConnectNeighborRooms))
            {
                //Debug.Log(room.roomID + " connected to " + targetRoom.roomID);
                var exitRequirments = new ExitAbstract(exit, room, capabilities.lastGainedAffordance);
                var entranceRequirements = new ExitAbstract(entrance, targetRoom, capabilities.lastGainedAffordance);
                if (exit.CanSupportExitAbstract(exitRequirments) && entrance.CanSupportExitAbstract(entranceRequirements))
                {
                    room.exits.Add(exitRequirments);
                    targetRoom.exits.Add(entranceRequirements);
                }
            }
        }
    }

    public bool AllowHaphazardConnection(RoomAbstract start, RoomAbstract end, out TraversalCapabilities capabilities)
    {
        capabilities = null;

        if(start.assignedRoomInfo.boss == BossName.MegaBeastCore) { return false; }
        if (end.assignedRoomInfo.boss == BossName.MegaBeastCore) { return false; }

        //This can create back doors that allow traversal items to be grabbed without defeating bosses
        if (end.majorItem != 0) { return false; }

        //This would require per grid space expected capability indices
        if (start.traversalPathRequirements.Count > 0 || end.traversalPathRequirements.Count > 0) { return false; }

        if (start.assignedRoomInfo.environmentType != end.assignedRoomInfo.environmentType) { return false; }

        if (start.expectedCapabilitiesIndex != end.expectedCapabilitiesIndex)
        {
            if (start.expectedCapabilitiesIndex < end.expectedCapabilitiesIndex &&
               end.expectedCapabilities.environmentalResistance == start.expectedCapabilities.environmentalResistance)
            {
                capabilities = end.expectedCapabilities;
                return true;
            }
            else
            {
                return false;
            }
        }

        capabilities = start.layout.traversalCapabilities[0];

        return true;
    }

    public IEnumerable AddShops(List<AchievementID> achievements)
    {
        var envSortedBranches = GetEnvironmentSortedBranches();

        //figure out which environments have which shops
        var twoShops = new List<ShopType> { ShopType.OrbSmith, ShopType.GunSmith, ShopType.Artificer };
        var oneShop = twoShops[random.Range(0, twoShops.Count)];
        twoShops.Remove(oneShop);
        var envShops = new Dictionary<EnvironmentType, List<ShopType>>();

        if (random.value > 0.5f)
        {
            envShops[_layout.environmentOrder[1]] = twoShops;
            envShops[_layout.environmentOrder[2]] = new List<ShopType> { oneShop };
        }
        else
        {
            envShops[_layout.environmentOrder[1]] = new List<ShopType> { oneShop };
            envShops[_layout.environmentOrder[2]] = twoShops;
        }

        if (achievements.Contains(AchievementID.TheTraitor))
        {
            envShops[EnvironmentType.BuriedCity] = new List<ShopType> { ShopType.TheTraitor };
        }

        foreach (var kvp in envShops)
        {
            List<RoomAbstract> potentialShopBranches = envSortedBranches[kvp.Key];

            foreach (var shopType in kvp.Value)
            {
                var attempt = 0;
                var maxAttempts = 3;
                bool shopAdded = false;

                while (!shopAdded && attempt < maxAttempts)
                {
                    attempt++;

                    RoomAbstract branchRoom = null;
                    branchRoom = FindBestSpecialRoomBranch(potentialShopBranches, false);
                    potentialShopBranches.Remove(branchRoom);
                    if (!branchRoom.HasUsableUnusedExits(_layout))
                    {
                        Debug.LogWarning("Shops: Picked a branch room with no usable exits. (another path likely blocked it)");
                        continue;
                    }

                    var environment = branchRoom.assignedRoomInfo.environmentType;
                    var envRoomList = roomInfos[environment];

                    int pathLength;
                    var specialRoom = TryPlaceSpecialRoom(branchRoom, envRoomList.shopRoomsInfos.FindAll((r) => r.shopType == shopType), 1, 3, out pathLength);

                    if (specialRoom != null)
                    {
                        specialRoom.shopOfferings = GetShopOfferings(shopType, kvp.Key, achievements);
                        _layout.CreateUndo();
                        _layout.Add(specialRoom, random.ZeroToMaxInt());

                        specialRoom.expectedCapabilitiesIndex = GetSpecialRoomCapabilitiesIndex(branchRoom, specialRoom, pathLength, 1, 3);

                        if(branchRoom.assignedRoomInfo.traversalPaths.Count > 0)
                        {
                            foreach (var p in branchRoom.assignedRoomInfo.traversalPaths)
                            {
                                if(!p.limitations.CapabilitesSufficient(specialRoom.expectedCapabilities, branchRoom.environmentalEffect))
                                {
                                    Debug.LogError("Traversal path room " + branchRoom.gridPosition.ToString() + " chosen for shop branch room, but capabilites are insufficent!");
                                }
                            }
                        }

                        var pathAddedSuccessfully = true;
                        foreach (var step in PathToSpecialRoom(branchRoom, specialRoom))
                        {
                            if(!step)
                            {
                                Debug.LogWarning("AddRoomsToPath failed for path from " + branchRoom.gridPosition + " to shop at " + specialRoom.gridPosition + " Password: " + _layout.password);
                                pathAddedSuccessfully = false;
                                _layout.Undo();
                                break;
                            }

                            if (stepByStep) yield return null;
                        }

                        if (pathAddedSuccessfully)
                        {
                            shopAdded = true;
                            _layout.allNonTraversalItemsAdded.AddRange(specialRoom.shopOfferings);
                            _layout.shopItemsAdded.AddRange(specialRoom.shopOfferings);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't Place A Shop off of branch at " + branchRoom.gridPosition + ".");
                    }

                    if (stepByStep) yield return null;
                }
            }
        }
    }

    public IEnumerable AddShrines()
    {
        var envSortedBranches = GetEnvironmentSortedBranches();

        //figure out which environments have which shrines and add 6 shrines
        var envShrines = new Dictionary<EnvironmentType, List<ShrineType>>();
        var shrineTypes = Enum.GetValues(typeof(ShrineType)).Cast<ShrineType>().ToList();
        shrineTypes.Remove(ShrineType.None);
        var validEnvs = new HashSet<EnvironmentType>();
        foreach (var env in _layout.environmentOrder)
        {
            if (env == EnvironmentType.BeastGuts || env == EnvironmentType.Glitch) continue;
            validEnvs.Add(env);
        }

        ShrineType shrine;
        foreach (var env in validEnvs)
        {
            shrine = shrineTypes[random.Range(0, shrineTypes.Count)];
            shrineTypes.Remove(shrine);
            envShrines.Add(env, new List<ShrineType>() { shrine });
            if(shrineTypes.Count <= 0) { break; }
        }

        foreach (var kvp in envShrines)
        {
            List<RoomAbstract> potentialShrineBranches = envSortedBranches[kvp.Key];

            foreach (var shrineType in kvp.Value)
            {
                var attempt = 0;
                var maxAttempts = 3;
                bool shrineAdded = false;

                while (!shrineAdded && attempt < maxAttempts)
                {
                    attempt++;

                    RoomAbstract branchRoom = null;
                    branchRoom = FindBestSpecialRoomBranch(potentialShrineBranches, false);
                    potentialShrineBranches.Remove(branchRoom);

                    if (branchRoom == null)
                    {
                        Debug.LogError("Shrines: FindBestSpecialRoomBranch failed to find a valid branch for environment " + kvp.Key);
                        break;
                    }

                    if (!branchRoom.HasUsableUnusedExits(_layout))
                    {
                        Debug.LogWarning("Shrines: Picked a branch room with no usable exits. (another path likely blocked it)");
                        continue;
                    }

                    var environment = branchRoom.assignedRoomInfo.environmentType;
                    var envRoomList = roomInfos[environment];

                    int pathLength;
                    var specialRoom = TryPlaceSpecialRoom(branchRoom, envRoomList.shrineRoomsInfos.FindAll((r) => r.shrineType == shrineType), 1, 3, out pathLength);
                    
                    if (specialRoom != null)
                    {
                        _layout.CreateUndo();
                        _layout.Add(specialRoom, random.ZeroToMaxInt());

                        specialRoom.expectedCapabilitiesIndex = GetSpecialRoomCapabilitiesIndex(branchRoom, specialRoom, pathLength, 1, 3);
                        if (specialRoom.expectedCapabilitiesIndex == branchRoom.expectedCapabilitiesIndex) specialRoom.environmentalEffect = branchRoom.environmentalEffect;

                        var pathAddedSuccessfully = true;
                        foreach (var step in PathToSpecialRoom(branchRoom, specialRoom))
                        {
                            if (!step)
                            {
                                Debug.LogWarning("AddRoomsToPath failed for path from " + branchRoom.gridPosition + " to shop at " + specialRoom.gridPosition + " Password: " + _layout.password);
                                pathAddedSuccessfully = false;
                                _layout.Undo();
                                break;
                            }

                            if (stepByStep) yield return null;
                        }

                        if (pathAddedSuccessfully) { shrineAdded = true; }
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't Place A Shrine off of branch at " + branchRoom.gridPosition + ".");
                    }

                    if (stepByStep) yield return null;
                }
            }
        }
    }

    public int GetSpecialRoomCapabilitiesIndex(RoomAbstract branchRoom, RoomAbstract specialRoom, int pathLength, int minRandIndex, int maxRandIndex)
    {
        var layout = branchRoom.layout;
        List<TraversalCapabilities> possibleCapabilites = layout.traversalCapabilities.GetRange(minRandIndex, maxRandIndex);
        possibleCapabilites.RemoveAll((c) => c.lastGainedAffordance.requiredEnvironmentalResistance != EnvironmentalEffect.None);
        int randomCapabilityIndex;
        if (possibleCapabilites.Count > 0)
        {
            randomCapabilityIndex = layout.traversalCapabilities.IndexOf(PickRandom(possibleCapabilites));
        }
        else
        {
            return branchRoom.expectedCapabilitiesIndex;
        }

        if (pathLength > 0 && randomCapabilityIndex > branchRoom.expectedCapabilitiesIndex &&
            branchRoom.expectedCapabilities.environmentalResistance == EnvironmentalEffect.None)
        {
            int expectedIndex = randomCapabilityIndex;

            //covers special circumstance where the top of the special room is exactly
            //1 space below the bottom of branch room and wants to use a jump ability.                        
            var specialRoomBounds = specialRoom.gridBounds;
            var branchRoomBounds = branchRoom.gridBounds;

            if (specialRoomBounds.yMax == branchRoomBounds.yMin + 1 && 
                specialRoomBounds.xMin >= branchRoomBounds.xMin &&
                specialRoomBounds.xMax <= branchRoomBounds.xMax)
            {
                var targetCapabilities = layout.traversalCapabilities[expectedIndex];
                if (targetCapabilities.lastGainedAffordance.minEffectiveJumpHeight > Constants.startingMaxJumpHeight)
                {
                    var e = expectedIndex;
                    expectedIndex = e > 0 ? e - 1 : e + 1;
                }
            }

            if (branchRoom.expectedCapabilitiesIndex > expectedIndex)
            {
                expectedIndex = branchRoom.expectedCapabilitiesIndex;
            }

            return expectedIndex;
        }
        else
        {
            return branchRoom.expectedCapabilitiesIndex;
        }
    }

    public IEnumerable AddBonusItems()
    {  //rediscover branches (Minor item and shop paths may have made new ones)
        var envSortedBranches = GetEnvironmentSortedBranches();
        var envs = new List<EnvironmentType>();
        foreach (var env in _layout.environmentOrder)
        {
            if (env == EnvironmentType.BeastGuts || env == EnvironmentType.Glitch) continue;
            envs.Add(env);
        }

        for (int i = 0; i < envs.Count; i++)
        {
            var environment = envs[i];

            List<RoomAbstract> potentialBonusItemBranches = envSortedBranches[environment];
            if (potentialBonusItemBranches.Count <= 0)
            {
                Debug.LogError(environment.ToString() + " does not have a branch for a bonus item");
                continue;
            }

            var attempt = 0;
            var maxAttempts = 3;
            bool bonusItemAdded = false;

            while (!bonusItemAdded && attempt < maxAttempts)
            {
                attempt++;

                if (potentialBonusItemBranches.Count <= 0)
                {
                    Debug.LogError(environment.ToString() + " ran out of branches for bonus items");
                    break;
                }

                var possibleItems = availableNonTraversalItems.FindAll((item) => 
                    !_layout.allNonTraversalItemsAdded.Contains(item.type) &&
                    !item.restrictedEnvironments.Contains(environment));
                var majorItem = PickRandom(possibleItems);
                //don't branch off of a room that requires capabilities beyond those of the item's max order...
                var maxCapIndex = _layout.traversalCapabilities.Count - 1;
                if(majorItem.restrictedEnvironments.Length > 0)
                {
                    var r = majorItem.restrictedEnvironments;
                    //Area1 = 0; Area2 = 1,2; Area3 = 3,4; Buried City = 5,6;
                    if (r.Contains(EnvironmentType.Cave) || r.Contains(EnvironmentType.CoolantSewers)) { maxCapIndex = 1; }
                    else if (r.Contains(EnvironmentType.Factory) || r.Contains(EnvironmentType.CrystalMines)) { maxCapIndex = 3; }
                    else if (r.Contains(EnvironmentType.BuriedCity)) { maxCapIndex = 5; }
                }

                RoomAbstract branchRoom = null;

                branchRoom = FindBestSpecialRoomBranch(potentialBonusItemBranches, true, 0, maxCapIndex);
                potentialBonusItemBranches.Remove(branchRoom);
                if (!branchRoom.HasUsableUnusedExits(_layout))
                {
                    Debug.LogWarning("Picked a branch room with no usable exits. (another path likely blocked it)");
                    continue;
                }

                int pathLength;
                var specialRoom = TryPlaceSpecialRoom(branchRoom, roomInfos[environment].majorItemRoomInfos, 1, 4, out pathLength);
                
                if (specialRoom != null)
                {
                    specialRoom.majorItem = majorItem.type;

                    _layout.CreateUndo();
                    _layout.Add(specialRoom, random.ZeroToMaxInt());

                    //you need a random traversal items beyond to first acquired to gain a bonus item
                    specialRoom.expectedCapabilitiesIndex = GetSpecialRoomCapabilitiesIndex(branchRoom, specialRoom, pathLength, 1, maxCapIndex);

                    if (specialRoom.expectedCapabilitiesIndex == branchRoom.expectedCapabilitiesIndex)
                    {
                        specialRoom.environmentalEffect = branchRoom.environmentalEffect;
                    }
                    
                    var pathAddedSuccessfully = true;
                    foreach (var step in PathToSpecialRoom(branchRoom, specialRoom))
                    {
                        if (!step)
                        {
                            Debug.LogWarning("AddRoomsToPath failed for path from " + branchRoom.gridPosition + " to shop at " + specialRoom.gridPosition);
                            pathAddedSuccessfully = false;
                            _layout.Undo();
                            break;
                        }

                        if (stepByStep) yield return null;
                    }

                    if (pathAddedSuccessfully)
                    {
                        _layout.allNonTraversalItemsAdded.Add(specialRoom.majorItem);
                        _layout.bonusItemsAdded.Add(specialRoom.majorItem);
                        bonusItemAdded = true;
                    }
                }
                else
                {
                    Debug.LogWarning("Couldn't Place A Bonus Item room off of branch at " + branchRoom.gridPosition + ".");
                }

                if (stepByStep) yield return null;
            }
        }
    }

    public IEnumerable AddMinorItems()
    {
        //rediscover branches (Shops and Bonus Item paths may have made new ones
        var envSortedBranches = GetEnvironmentSortedBranches();
        Dictionary<EnvironmentType, List<RoomAbstract>> envSortedHidingSpots = new Dictionary<EnvironmentType, List<RoomAbstract>>();
        foreach (var env in _layout.environmentOrder)
        {
            if (env == EnvironmentType.BeastGuts || env == EnvironmentType.Glitch || env == EnvironmentType.GreyBox) continue;
            envSortedHidingSpots.Add(env, new List<RoomAbstract>());
        }

        //discover hiding spots
        foreach (var room in _layout.roomAbstracts)
        {
            if (room.assignedRoomInfo.environmentType == EnvironmentType.BeastGuts || room.assignedRoomInfo.environmentType == EnvironmentType.Glitch)
            {
                continue;
            }

            if (room.majorItem != MajorItem.None || (room.isStartingRoom && room.width == 1 && room.height == 1))
            {
                continue;
            }

            if (room.ValidMinorItemHidingSpot())
            {
                envSortedHidingSpots[room.assignedRoomInfo.environmentType].Add(room);
            }
        }

        var envMinorItems = new Dictionary<EnvironmentType, int>
            {
                { _layout.environmentOrder[0], _layout.minorItemCount > 3 ? 3 : _layout.minorItemCount}, //2 less in surface
            };

        for (int i = 1; i < _layout.environmentOrder.Length; i++)
        {
            var env = _layout.environmentOrder[i];
            if (env == EnvironmentType.BeastGuts || env == EnvironmentType.Glitch) continue;
            envMinorItems.Add(env, 0);
        }

        var remaining = _layout.minorItemCount - 3;
        if (remaining > 0)
        {
            var split = Constants.EvenSplit(remaining, envMinorItems.Count-1);
            var keys = envMinorItems.Keys.ToList();
            for (int i = 0; i < split.Length; i++)
            {
                envMinorItems[keys[i + 1]] = split[i];
            }
        }

        var minorItemPool = GenerateMinorItemPool(_layout.minorItemCount);
        int carryOvers = 0;
        int totalMinorItemsAdded = 0;

        foreach (var kvp in envMinorItems)
        {
            List<RoomAbstract> potentialMinorItemBranches = envSortedBranches[kvp.Key];
            List<RoomAbstract> potentialMinorItemHidingSpots = envSortedHidingSpots[kvp.Key];
            var preferedMinorItemHidingSpots = new List<RoomAbstract>(potentialMinorItemHidingSpots.Where(h => h.HasMinorItemSpotInUnusedSpace()));

            //use 1x2 and 2x1 traversal paths first
            var travPaths = preferedMinorItemHidingSpots.FindAll(r => r.traversalPathRequirements.Count > 0 && (r.width * r.height) == 2);
            if (travPaths.Count > 0) { preferedMinorItemHidingSpots = travPaths; }

            var envMinorItemCount = kvp.Value + carryOvers;

            //try to make ALL the minor items in hiding spots
            int useHidingSpots; // Mathf.Clamp(envMinorItemCount, 0, potentialMinorItemHidingSpots.Count);

            if (envMinorItemCount < 3)
                useHidingSpots = Mathf.Clamp(envMinorItemCount, 0, potentialMinorItemHidingSpots.Count);
            else
                useHidingSpots = Mathf.Clamp(Mathf.CeilToInt(envMinorItemCount * 0.85f), 0, potentialMinorItemHidingSpots.Count);

            int envItemsAdded = 0;
            int attempt = 0;
            int maxAttempts = envMinorItemCount * 3;

            while (envItemsAdded < envMinorItemCount && attempt < maxAttempts)
            {
                attempt++;

                var minorItemData = new MinorItemData(totalMinorItemsAdded);

                //do hiding spots first!
                if (envItemsAdded < useHidingSpots)
                {
                    RoomAbstract hidingSpot = PickRandom(preferedMinorItemHidingSpots.Count > 0 ? preferedMinorItemHidingSpots : potentialMinorItemHidingSpots);

                    var spot = hidingSpot.GetBestMinorItemSpawn();

                    minorItemData.type = PickRandom(minorItemPool);
                    minorItemData.spawnInfo = spot;
                    hidingSpot.minorItems.Add(minorItemData);

                    if (preferedMinorItemHidingSpots.Count > 0)
                    {
                        if (!hidingSpot.HasMinorItemSpotInUnusedSpace())
                        {
                            preferedMinorItemHidingSpots.Remove(hidingSpot);
                        }
                    }

                    //if the room is full up on usable minor item spawns remove it from the pontential hiding spots
                    if (!hidingSpot.ValidMinorItemHidingSpot())
                    {
                        potentialMinorItemHidingSpots.Remove(hidingSpot);
                        preferedMinorItemHidingSpots.Remove(hidingSpot);
                    }

                    envItemsAdded++;
                    totalMinorItemsAdded++;
                    minorItemPool.Remove(minorItemData.type);
                    _layout.minorItemsAdded.Add(minorItemData.type);

                    if (stepByStep) yield return null;

                    continue;
                }

                if (potentialMinorItemBranches.Count <= 0)
                {
                    Debug.LogError("Ran out of minor item branch rooms!");
                    continue;
                }

                RoomAbstract branchRoom = null;
                branchRoom = FindBestSpecialRoomBranch(potentialMinorItemBranches, true);
                potentialMinorItemBranches.Remove(branchRoom);
                if (!branchRoom.HasUsableUnusedExits(_layout))
                {
                    Debug.LogWarning("Picked a branch room with no usable exits. (another path likely blocked it)");
                    continue;
                }

                minorItemData.type = PickRandom(minorItemPool);

                var environment = branchRoom.assignedRoomInfo.environmentType;
                var envRoomList = roomInfos[environment];

                int pathLength;
                var specialRoom = TryPlaceSpecialRoom(branchRoom, envRoomList.minorItemRoomInfos, 1, 4, out pathLength);

                if (specialRoom != null)
                {
                    specialRoom.minorItems = new List<MinorItemData> { minorItemData };

                    _layout.CreateUndo();
                    _layout.Add(specialRoom, random.ZeroToMaxInt());

                    int maxCapabilityIndex = _layout.traversalCapabilities.Count - (pathLength > 2 ? 1 : 2);
                    specialRoom.expectedCapabilitiesIndex = GetSpecialRoomCapabilitiesIndex(branchRoom, specialRoom, pathLength, 1, maxCapabilityIndex);
                    if (specialRoom.expectedCapabilitiesIndex == branchRoom.expectedCapabilitiesIndex) specialRoom.environmentalEffect = branchRoom.environmentalEffect;

                    var pathAddedSuccessfully = true;
                    foreach (var step in PathToSpecialRoom(branchRoom, specialRoom))
                    {
                        if (!step)
                        {
                            Debug.LogWarning("AddRoomsToPath failed for path from " + branchRoom.gridPosition + " to shop at " + specialRoom.gridPosition);
                            pathAddedSuccessfully = false;
                            _layout.Undo();
                            break;
                        }

                        if (stepByStep) yield return null;
                    }

                    if (pathAddedSuccessfully)
                    {
                        envItemsAdded++;
                        totalMinorItemsAdded++;
                        minorItemPool.Remove(minorItemData.type);
                        _layout.minorItemsAdded.Add(minorItemData.type);
                    }
                }
                else
                {
                    Debug.LogWarning("Couldn't Place A Minor Item off of branch at " + branchRoom.gridPosition + ". Seed (" + _layout.password + ")");
                }

                if (stepByStep) yield return null;
            } //end while (envItemsAdded < envMinorItemCount && attempt < maxAttempts)

            carryOvers = envMinorItemCount - envItemsAdded;
        } //end foreach (var kvp in envMinorItems)

        if (totalMinorItemsAdded != _layout.minorItemCount)
        {
            Debug.LogError("minorItemsAdded != layout.minorItemCount");
            _layout.minorItemCount = totalMinorItemsAdded;
        }
    }

    public IEnumerable<bool> PathToSpecialRoom(RoomAbstract branchRoom, RoomAbstract specialRoom)
    {
        path = GetPath(branchRoom, specialRoom);
        var env = branchRoom.assignedRoomInfo.environmentType;

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("Couldn't find path from " + branchRoom.gridPosition + " to shop at " + specialRoom.gridPosition);
            yield return false;
            yield break;
        }

        if(stepByStep) { yield return true; }

        if (path.Count > 4 && (path[0].x == path[path.Count - 1].x || path[0].y == path[path.Count - 1].y))
        {
            ConvolutePath(path, env);
            if (stepByStep) { yield return true; }
        }

        //trim path to get accurate specialExpected
        var furthestIndex = 0;
        for (int i = 0; i < path.Count; i++)
        {
            var room = _layout.GetRoomAtPositon(path[i]);
            if (room != null && room.expectedCapabilitiesIndex == branchRoom.expectedCapabilitiesIndex)
            {
                branchRoom = room;
                furthestIndex = i;
            }
            else
            {
                break;
            }
        }

        if (furthestIndex > 0) { path.RemoveRange(0, furthestIndex); }

        var startPathWithLock = specialRoom.expectedCapabilitiesIndex > branchRoom.expectedCapabilitiesIndex;
        if (branchRoom.specialExpectedCapabilities != null)
        {
            int newExpected;
            var start = path.First();
            if (branchRoom.specialExpectedCapabilities.TryGetValue(branchRoom.GetLocalPosition(start), out newExpected))
            {
                if (newExpected > specialRoom.expectedCapabilitiesIndex)
                {
                    specialRoom.expectedCapabilitiesIndex = newExpected;
                    startPathWithLock = false;
                }
            }
        }
        
        foreach (var step in AddRoomsToPath(path, env, specialRoom.expectedCapabilities, startPathWithLock))
        {
            if (!step)
            {
                Debug.LogWarning("AddRoomsToPath failed to path from " + branchRoom.gridPosition + " to special room at " + specialRoom.gridPosition);
                yield return false;
            }

            if (stepByStep) yield return true;
        }
    }

    public IEnumerable AddTeleporters()
    { 
        var envSortedBranches = GetEnvironmentSortedBranches();
        var forbiddenEnvironments = new EnvironmentType[] { EnvironmentType.Surface, EnvironmentType.ForestSlums, EnvironmentType.BeastGuts, EnvironmentType.Glitch };
        var envs = _layout.environmentOrder.Where((e) => !forbiddenEnvironments.Contains(e));

        foreach (var environment in envs)
        {
            List<RoomAbstract> potentialTeleporterBranches = envSortedBranches[environment];

            //don't add teleporter to connecting paths
            potentialTeleporterBranches.RemoveAll(r => _layout.connectingPaths.Contains(r.parentPath));

            if (potentialTeleporterBranches.Count <= 0)
            {
                Debug.LogError(environment.ToString() + " does not have a branch for a teleporter");
                continue;
            }

            var attempt = 0;
            var maxAttempts = 3;
            bool teleporterAdded = false;

            while (!teleporterAdded && attempt < maxAttempts)
            {
                attempt++;

                if (potentialTeleporterBranches.Count <= 0)
                {
                    Debug.LogError(environment.ToString() + " ran out of branches for teleporter");
                    break;
                }

                RoomAbstract branchRoom = null;

                int maxCapIndex = 0;
                if(environment == _layout.environmentOrder[1])
                {
                    maxCapIndex = 2; //before boss
                }
                else if (environment == _layout.environmentOrder[2])
                {
                    maxCapIndex = 4;
                }
                else
                {
                    maxCapIndex = 6;
                }                

                branchRoom = FindBestSpecialRoomBranch(potentialTeleporterBranches, true, 0, maxCapIndex);

                potentialTeleporterBranches.Remove(branchRoom);
                if (!branchRoom.HasUsableUnusedExits(_layout))
                {
                    Debug.LogWarning("Picked a branch room with no usable exits. (another path likely blocked it)");
                    continue;
                }

                int pathLength;
                var specialRoom = TryPlaceSpecialRoom(branchRoom, roomInfos[environment].teleporterRoomInfos, 1, 4, out pathLength);

                if (specialRoom != null)
                {
                    _layout.CreateUndo();
                    _layout.Add(specialRoom, random.ZeroToMaxInt());
                    specialRoom.expectedCapabilitiesIndex = branchRoom.expectedCapabilitiesIndex;
                    specialRoom.environmentalEffect = branchRoom.environmentalEffect;
                    
                    var pathAddedSuccessfully = true;
                    foreach (var step in PathToSpecialRoom(branchRoom, specialRoom))
                    {
                        if (!step)
                        {
                            Debug.LogWarning("AddRoomsToPath failed for path from " + branchRoom.gridPosition + " to shop at " + specialRoom.gridPosition);
                            pathAddedSuccessfully = false;
                            _layout.Undo();
                            break;
                        }

                        if (stepByStep) yield return null;
                    }

                    if (pathAddedSuccessfully) { teleporterAdded = true; }
                }
                else
                {
                    Debug.LogWarning("Couldn't Place A Teleporter off of branch at " + branchRoom.gridPosition + ".");
                }

                if (stepByStep) yield return null;
            }
        }
    }

    public RoomAbstract FindBestSpecialRoomBranch(List<RoomAbstract> potentialBranches, bool forceBossAndTraversalPaths, int minExpectedCap = 0, int maxExpectedCap = 7)
    {
        if(potentialBranches.Count == 0)
        {
            Debug.LogError("FindBestSpecialRoomBranch passed an empty list of potentialBranches!");
            return null;
        }

        var preferedBranches = potentialBranches.FindAll(r => (r.HasUsableUnusedExitsInUnusedSpaces() || (r.branchRoom && r.exits.Count == 1)) &&
            r.expectedCapabilitiesIndex <= maxExpectedCap && r.expectedCapabilitiesIndex >= minExpectedCap);

        if (forceBossAndTraversalPaths && preferedBranches.Count > 0)
        {
            var wallCreep = preferedBranches.FirstOrDefault(r => r.assignedRoomInfo.boss == BossName.WallCreep);
            if (wallCreep != null) { return wallCreep; }

            var travPaths = preferedBranches.FindAll(r => r.assignedRoomInfo.traversalPaths.Count > 0 && (r.width * r.height) == 2);
            if(travPaths.Count > 0)
            {
                return PickRandom(travPaths);
            }
        }

        if(preferedBranches.Count == 0)
        {
            preferedBranches = potentialBranches;
        }

        float bestExitRatio = 0f;
        var bestRooms = new List<RoomAbstract>();
        bool tripleExits = false;

        foreach (var room in preferedBranches)
        {
            var expectedCap = room.expectedCapabilitiesIndex;
            if (expectedCap > maxExpectedCap || expectedCap < minExpectedCap) continue;

            var exits = room.GetUsableUnusedExits();
            float usableExits = exits.Count;
            float usableExitRatio = usableExits / room.assignedRoomInfo.possibleExits.Count;
            var hasUnusedEndSpace = false;
            
            if(room.branchRoom && room.exits.Count == 1 && usableExits > 0)
            {
                if(bestExitRatio != 1)
                {
                    bestExitRatio = 1;
                    bestRooms.Clear();
                }
                bestRooms.Add(room);
                continue;
            }

            if(usableExits >= 3 && room.width != room.height && (room.width == 1 || room.height == 1))
            {
                if(room.width > room.height)
                {
                    hasUnusedEndSpace = exits.FindAll(e => e.localGridPosition.x == 0).Count == 3 || exits.FindAll(e => e.localGridPosition.x == room.width - 1).Count == 3;
                }
                else
                {
                    hasUnusedEndSpace = exits.FindAll(e => e.localGridPosition.y == 0).Count == 3 || exits.FindAll(e => e.localGridPosition.y == room.height - 1).Count == 3;
                }

                if (hasUnusedEndSpace && !tripleExits)
                {
                    bestRooms.Clear();
                    bestExitRatio = usableExitRatio;
                    tripleExits = true;
                }
            }

            if (!tripleExits || hasUnusedEndSpace)
            {
                if (usableExitRatio > bestExitRatio)
                {
                    bestExitRatio = usableExitRatio;
                    bestRooms.Clear();
                    bestRooms.Add(room);
                }
                else if (usableExitRatio == bestExitRatio)
                {
                    bestRooms.Add(room);
                }
            }
        }

        return PickRandom(bestRooms.Count > 0 ? bestRooms : preferedBranches);
    }

    public Dictionary<EnvironmentType, List<RoomAbstract>> GetEnvironmentSortedBranches()
    {
        var envSortedBranches = new Dictionary<EnvironmentType, List<RoomAbstract>>()
        {
            { EnvironmentType.Surface, new List<RoomAbstract>() },
            { EnvironmentType.ForestSlums, new List<RoomAbstract>() },
            { EnvironmentType.Cave, new List<RoomAbstract>() },
            { EnvironmentType.CoolantSewers, new List<RoomAbstract>() },
            { EnvironmentType.Factory, new List<RoomAbstract>() },
            { EnvironmentType.CrystalMines, new List<RoomAbstract>() },
            { EnvironmentType.BuriedCity, new List<RoomAbstract>() },
        };

        foreach (var room in _layout.roomAbstracts)
        {
            var info = room.assignedRoomInfo;
            if (info.environmentType == EnvironmentType.BeastGuts || info.environmentType == EnvironmentType.Glitch) continue;
            if (info.roomType == RoomType.MegaBeast || info.boss == BossName.BeastRemnants) continue;
            if (room.majorItem != MajorItem.None || (room.isStartingRoom && room.width == 1 && room.height == 1)) continue;            

            if (room.GetUsableUnusedExits().Count > 0)
            {
                envSortedBranches[room.assignedRoomInfo.environmentType].Add(room);
            }
        }

        return envSortedBranches;
    }

    public List<MajorItem> GetShopOfferings(ShopType shopType, EnvironmentType envType, List<AchievementID> achievements)
    {
        var offerings = new List<MajorItem>();
        Dictionary<MajorItem, MajorItemInfo> pool = null;
        switch(shopType)
        {
            case ShopType.OrbSmith:
                pool = orbSmithItemInfos;
                break;
            case ShopType.GunSmith:
                pool = gunSmithItemInfos;
                break;
            case ShopType.Artificer:
                pool = artificerItemInfos;
                break;
            case ShopType.TheTraitor:
                pool = theTraitorItemInfos;
                break;
        }

        var possibleItems = pool.Values.Where((i) => !i.restrictedEnvironments.Contains(envType) && !_layout.allNonTraversalItemsAdded.Contains(i.type) &&
                    (i.requiredAchievement == AchievementID.None || achievements == null || achievements.Contains(i.requiredAchievement))).ToList();

        var itemCount = (achievements == null || achievements.Contains(AchievementID.AllyBot)) ? 3 : random.value > 0.5f ? 3 : 2;

        for (int i = 0; i < itemCount; i++)
        {
            var pick = PickRandom(possibleItems);
            possibleItems.Remove(pick);
            offerings.Add(pick.type);
        }

        return offerings;
    }

    public RoomAbstract TryPlaceSpecialRoom(RoomAbstract branchRoom, List<RoomInfo> roomInfos, int minPathLength, int maxPathLength, out int pathLength)
    {
        var layout = branchRoom.layout;

        var unusedExits = branchRoom.GetUsableUnusedExits();
        var preferredExits = new List<ExitLimitations>();

        foreach (var exit in unusedExits)
        {
            if (!branchRoom.exits.Any(e => e.localGridPosition == exit.localGridPosition))
            {
                preferredExits.Add(exit);
            }
        }

        //There's no guarantee that CreatePathBetweenRooms will use this exit, after the target is set by the for loop below.
        var chosenExit = PickRandom(preferredExits.Count > 0 ? preferredExits : unusedExits);

        //don't move the item room back towards where it came from
        var possibleDirections = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList().FindAll(d => d != Direction.None && d != chosenExit.direction.Opposite());
        var preferedDirection = chosenExit.direction; // try and move away from the room first
        var lastDirection = Direction.None;
        int desiredPathLength = random.Range(minPathLength, maxPathLength+1); //max Exclusive
        var roomPosition = branchRoom.GetExitTarget(chosenExit);

        pathLength = 0;

        for (int p = 0; p < desiredPathLength; p++)
        {
            var tryDirections = possibleDirections.ToList(); //copyList
            var newPostion = roomPosition;

            bool success = false;
            bool forcePrefered = lastDirection != preferedDirection;
            while (!success && tryDirections.Count > 0)
            {
                Direction tryDirection;
                if (forcePrefered)
                {
                    forcePrefered = false;
                    tryDirection = preferedDirection;
                }
                else
                {
                    tryDirection = PickRandom(tryDirections);
                }

                newPostion += tryDirection.ToInt2D();
                if (layout.ContainsPosition(newPostion, branchRoom.assignedRoomInfo.environmentType) && layout.GetRoomAtPositon(newPostion) == null)
                {
                    pathLength++;
                    lastDirection = tryDirection;
                    roomPosition = newPostion;
                    success = true;
                }
                else
                {
                    newPostion = roomPosition;
                    tryDirections.Remove(tryDirection);
                }
            }
        }

        var usableRoomInfos = roomInfos.FindAll(r => r.HasPossibleExitInDirection(chosenExit.direction.Opposite()));

        RoomAbstract specialRoom = null;
        while (specialRoom == null && usableRoomInfos.Count > 0)
        {
            var specialRoomInfo = PickRandom(usableRoomInfos);
            usableRoomInfos.Remove(specialRoomInfo);

            if (layout.ValidRoomPlacement(roomPosition, specialRoomInfo.size))
            {
                specialRoom = new RoomAbstract(specialRoomInfo, roomPosition);
                var usableExits = specialRoom.GetUsableUnusedExits(layout, branchRoom, (e) => e.direction == chosenExit.direction.Opposite());
                if(usableExits == null || usableExits.Count == 0)
                {
                    specialRoom = null;
                    continue;
                }
            }
        }

        return specialRoom;
    }

    public IEnumerable<bool> ConnectSegments(SegmentConnection connection)
    {
        var segments = _layout.pattern.segments;
        var segment1 = segments[connection.from];
        var segment2 = segments[connection.to];

        var s1key = segment1.id + segment2.environmentType.ToString();
        var s2key = segment2.id + segment1.environmentType.ToString();
        Debug.Log("Connecting " + s1key + " to " + s2key);

        var startRoom = _layout.roomAbstracts[_layout.connectorBranches[s1key]];
        var endRoom = _layout.roomAbstracts[_layout.connectorBranches[s2key]];
        var connectCapabilities = _layout.traversalCapabilities[segment2.minCapabilitiesIndex];
        var roomsToConnect = new List<RoomAbstract>() { startRoom };
        var validEnvs = new HashSet<EnvironmentType> { startRoom.assignedRoomInfo.environmentType, endRoom.assignedRoomInfo.environmentType };

        //code for transition rooms
        var transitionRooms = roomInfos[segment1.environmentType].transitionRoomInfos.FindAll(r => r.transitionsTo == segment2.environmentType && r.transitionStartDirection == connection.direction);
        if(transitionRooms.Count == 0) //check for the inverse
        {
            transitionRooms = roomInfos[segment2.environmentType].transitionRoomInfos.FindAll(r => r.transitionsTo == segment1.environmentType && r.transitionStartDirection == connection.direction.Opposite());
        }

        bool transitionRoomFound = false;
        RoomAbstract transitionRoom;

        if (transitionRooms.Count > 0)
        {
            var pathFinder = new PathFinder();
            var s = startRoom.GetClosestGlobalPositionWithExit(endRoom.gridCenter);
            var e = endRoom.GetClosestGlobalPositionWithExit(startRoom.gridCenter);
            var fullPath = pathFinder.GetPath(_layout, s, e, (room, pos) => PathValidate(room, pos, startRoom, endRoom) && validEnvs.Contains(room.assignedRoomInfo.environmentType), 1.5f);

            if (fullPath == null || fullPath.Count == 0)
            {
                Debug.LogError("Path Finder could path between start " + startRoom.gridPosition + " and end " + endRoom.gridPosition + " to place transition room! Restarting!");
                yield return false;
                yield break;
            }

            var position = fullPath[(int)Mathf.Lerp(0, fullPath.Count, 0.5f)];
            var info = PickRandom(transitionRooms);

            var direction = (endRoom.gridCenter - startRoom.gridCenter).Vector2().normalized;
            //var direction = info.transitionStartDirection.ToVector2();
            //var vertical = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);
            var vertical = !info.transitionStartDirection.isHorizontal();

            var boundry = new Rect();
            var startBounds = startRoom.gridBounds;
            var endBounds = endRoom.gridBounds;
            if(vertical)
            {
                boundry.xMin = _layout.environmentLimits[segment1.environmentType].xMin;
                boundry.xMax = _layout.environmentLimits[segment1.environmentType].xMax;
                boundry.yMin = startBounds.yMin < endBounds.yMin ? startBounds.yMin : endBounds.yMin;
                boundry.yMax = startBounds.yMax > endBounds.yMax ? startBounds.yMax : endBounds.yMax;
            }
            else
            {
                boundry.xMin = startBounds.xMin < endBounds.xMin ? startBounds.xMin : endBounds.xMin;
                boundry.xMax = startBounds.xMax > endBounds.xMax ? startBounds.xMax : endBounds.xMax;
                boundry.yMin = _layout.environmentLimits[segment1.environmentType].yMin;
                boundry.yMax = _layout.environmentLimits[segment1.environmentType].yMax;
            }

            var buffer = Buffer2D.zero;
            if(connectCapabilities.lastGainedAffordance.minEffectiveJumpHeight > Constants.startingMaxJumpHeight)
            {
                if (vertical)
                {
                    if (startRoom.gridPosition.y > endRoom.gridPosition.y)
                    {
                        buffer.top = 2;
                    }
                    else
                    {
                        buffer.bottom = 2;
                    }
                }
                else if (startRoom.gridPosition.x < endRoom.gridPosition.x)
                {
                    buffer.left = 1;
                }
                else
                {
                    buffer.right = 1;
                }
            }

            var rect = info.GetRectAtGridPosition(position);
            if(_layout.MovePositionAwayFromRooms(ref rect, buffer, direction, boundry))
            {
                position = Int2D.GetRoomPosFromRect(rect);
            }

            if (!_layout.ValidRoomPlacement(position,info.size))
            {
                Debug.LogError("ConnectSegments couldn't place transition room! Restarting!");
                yield return false;
                yield break;
            }

            transitionRoom = _layout.Add(new RoomAbstract(info, position), random.ZeroToMaxInt());
            transitionRoom.expectedCapabilitiesIndex = segment2.minCapabilitiesIndex;
            roomsToConnect.Add(transitionRoom);
            transitionRoomFound = true;
            if (stepByStep) yield return true;
        }

        roomsToConnect.Add(endRoom);

        yield return true;

        for (int r = 0; r < roomsToConnect.Count - 1; r++)
        {
            var start = roomsToConnect[r];
            var end = roomsToConnect[r + 1];
            var startPathWithLock = (_layout.traversalItemsAdded > 0 && start == startRoom);
            var env = transitionRoomFound ? end.assignedRoomInfo.environmentType : segment1.environmentType;
            var pathfinder = new PathFinder();
            var endToRight = end.gridPosition.x > start.gridPosition.x;

            Int2D s;
            if (start.assignedRoomInfo.roomType == RoomType.TransitionRoom && end.assignedRoomInfo.transitionStartDirection == Direction.Down)
            {
                var b = start.gridBounds;
                s = start.GetClosestGlobalPositionWithExit(new Int2D((int)(endToRight ? b.max.x : b.min.x), (int)b.min.y));
            }
            else
            {
                s = start.GetClosestGlobalPositionWithExit(end.gridCenter);
            }

            Int2D e;
            if (end.assignedRoomInfo.roomType == RoomType.TransitionRoom && end.assignedRoomInfo.transitionStartDirection == Direction.Down)
            {
                var b = end.gridBounds;
                e = end.GetClosestGlobalPositionWithExit(new Int2D((int)(endToRight ? b.min.x : b.max.x), (int)b.max.y));
            }
            else
            {
                e = end.GetClosestGlobalPositionWithExit(s);
            }

            //Don't allow the path to cross back through the pre transition room environment.
            var validEnv = r == 0 ? startRoom.assignedRoomInfo.environmentType : endRoom.assignedRoomInfo.environmentType;

            path = pathfinder.GetPath(_layout, s, e, (room, pos) => PathValidate(room, pos, start, end) && 
                (room == start || room == end || room.assignedRoomInfo.environmentType == validEnv));
            if (path == null || path.Count == 0)
            {
                Debug.LogError("PathFinder failed to find path between " + start.roomID + " and " + end.roomID);
                yield return false;
            }

            if(stepByStep) { yield return true; }
            ConvolutePath(path, env);
            if (stepByStep) { yield return true; }

            foreach (var step in AddRoomsToPath(path, env, connectCapabilities, startPathWithLock))
            {
                if (!step)
                {
                    Debug.LogError("AddRoomsToPath failed");
                    yield return false;
                    break;
                }

                if (stepByStep) yield return true;
            }
        }
    }

    public void ConvolutePath(List<Int2D> path, EnvironmentType env)
    {
        //explore starting with second node all the way to the next til last node
        for (int i = 1; i < path.Count-2; i++)
        {
            var start = path[i - 1];
            var middle = path[i];
            var end = path[i + 1];
            var extension = path[i + 2];

            //looking for 4 positions in a straight line
            if (_layout.GetRoomAtPositon(start) != null) continue;
            if (_layout.GetRoomAtPositon(middle) != null) continue;
            if (_layout.GetRoomAtPositon(end) != null) continue;
            if (_layout.GetRoomAtPositon(extension) != null) continue;

            Func<Int2D, bool> canShift = (Int2D p) => 
            {
                if (path.Contains(p) || !_layout.ContainsPosition(p, env) || _layout.GetRoomAtPositon(p) != null) return false;
                return true;
            };

            Action<Int2D> shift = (Int2D dir) =>
            {
                // [0]   [1]   [2]
                //(1,1) (1,2) (1,3)
                path[i] = middle + dir;
                //(1,1) (2,2) (1,3)
                path.Insert(i, start + dir);
                //(1,1) (2,1), (2,2) (1,3)
                path.Insert(i + 2, end + dir);
                // [0]   [1]    [2]    [3]    [4]
                //(1,1) (2,1), (2,2), (2,3), (1,3)
                i = i + 3; //i was 1 and is now 4;
            };

            if(start.x == middle.x && middle.x == end.x && end.x == extension.x)
            {
                bool right = canShift(start + Int2D.right) &&
                    canShift(middle + Int2D.right) &&
                    canShift(end + Int2D.right);
                bool left = canShift(start + Int2D.left) &&
                    canShift(middle + Int2D.left) &&
                    canShift(end + Int2D.left);
                if(right && (!left || random.value > 0.5f))
                {
                    shift(Int2D.right);
                    continue;
                }
                else if (left)
                {
                    shift(Int2D.left);
                    continue;
                }
            }

            if (start.y == middle.y && middle.y == end.y && end.y == extension.y)
            {
                var up = canShift(start + Int2D.up) &&
                    canShift(middle + Int2D.up) &&
                    canShift(end + Int2D.up);
                var down = canShift(start + Int2D.down) &&
                    canShift(middle + Int2D.down) &&
                    canShift(end + Int2D.down);
                if (up && (!down || random.value > 0.5f))
                {
                    shift(Int2D.up);
                    continue;
                }
                else if (down)
                {
                    shift(Int2D.down);                    
                    continue;
                }
            }
        }
    }

    public bool PathValidate(RoomAbstract r, Int2D pos, RoomAbstract start, RoomAbstract end)
    {
        //don't let dead end branch rooms path through old rooms
        if(start.exits.Count == 1 && start.branchRoom && r.exits.Contains(start.layout.GetConnectedExit(start.exits.First())))
        {
            return false;
        }

        var rEnv = r.assignedRoomInfo.environmentType;
        var sEnv = start.assignedRoomInfo.environmentType;
        var eEnv = end.assignedRoomInfo.environmentType;
        
        if (rEnv != sEnv && rEnv != eEnv) { return false; }

        var acceptableRoomTypes = new HashSet<RoomType> { RoomType.None, RoomType.SaveRoom };
        var rExpected = r.GetExpectedCapabilitiesIndex(r.GetLocalPosition(pos));
        var ecIndexValid = (rExpected == start.expectedCapabilitiesIndex || rExpected == end.expectedCapabilitiesIndex);
        var startValid = r == start && rExpected == start.expectedCapabilitiesIndex;
        var endValid = r == end && rExpected == end.expectedCapabilitiesIndex;
        return startValid || endValid || (ecIndexValid && acceptableRoomTypes.Contains(r.assignedRoomInfo.roomType) ||
               (r.assignedRoomInfo.roomType == RoomType.StartingRoom && r.assignedRoomInfo.environmentType == EnvironmentType.Glitch));
    }

    public void SetupLayoutGenerationSegment(LayoutGenerationSegment segment, LayoutGenerationSegment lastSegment, TraversalCapabilities startingAbilities)
    {
        if(segment.sourcePosition != Int2D.negOne)
        {
            RoomInfo sourceInfo;
            if (segment.sourceIsEnvironmentStart || segment.sourceIsStartingRoom)
            {
                var list = roomInfos[segment.environmentType].startingRoomInfos.Where((r) => r.transitionStartDirection == segment.startingRoomDirection).ToList();
                if (list.Count == 0) { Debug.LogError("Env type " + segment.environmentType + " had no starting rooms: " + _layout.password); }
                sourceInfo = PickRandom(list);
            }
            else
            {
                var list = roomInfos[segment.environmentType].generalRoomInfos.Where((r) => r.SuitableBranchingRoom(startingAbilities) && r.transitionStartDirection == segment.startingRoomDirection).ToList();
                sourceInfo = PickRandom(list);
            }

            segment.sourceRoom = new RoomAbstract(sourceInfo, segment.sourcePosition)
            {
                isEnvironmentStart = segment.sourceIsEnvironmentStart,
                isStartingRoom = segment.sourceIsStartingRoom,
            };
        }

        if(segment.branchRoomsNeeded < 0)
        {
            segment.CalculateBranchesNeeded();
        }

        if (segment.sourceRoom == null && lastSegment != null)
        {
            segment.sourceRoom = lastSegment.finalBranch;
        }
    }

    /// <summary>
    /// generates a hub/spoke layout segment based on the instructions specified.
    /// </summary>
    /// <param name="_layout">The layout to add the segment to</param>
    /// <param name="segment">The segment to add</param>
    /// <param name="stepByStep">whether or not to pause per step for debugging</param>
    /// <returns>whether or not the loop should continue</returns>
    public IEnumerable<bool> CreateLayoutGenerationSegment(LayoutGenerationSegment segment)
    {
        bool pathSuccessful = false;

        if (segment.sourceRoom == null)
        {
            Debug.LogError("Layout Segment passed to has unassigned sourceRoom and cannot continue!");
            Debug.LogError("AddLayoutGenerationSegment cannot continue! Password: " + _layout.password);
            yield return false; yield break;
        }

        if (!_layout.roomAbstracts.Contains(segment.sourceRoom))
        {
            if (_layout.ValidRoomPlacement(segment.sourceRoom.gridPosition, segment.sourceRoom.assignedRoomInfo.size))
            {
                _layout.Add(segment.sourceRoom, random.ZeroToMaxInt());
            }
            else
            {
                Debug.LogError("Layout Segment source room has an invalid position for its size and can't be added!");
                Debug.LogError("AddLayoutGenerationSegment cannot continue! Password: " + _layout.password);
                yield return false; yield break;
            }
        }

        RoomAbstract currentPathStart = segment.sourceRoom;
        var currentCapabilities = _layout.traversalCapabilities[_layout.traversalItemsAdded];
        var roomList = roomInfos[segment.environmentType];

        //This all for the stupid Cave Loop back :(
        if (segment.finalTraversalStart)
        {
            var newStartDirection = segment.hubDirection;

            if (segment.hubDirectionDeviation != 0)
            {
                newStartDirection = random.ZAngle(-segment.hubDirectionDeviation, segment.hubDirectionDeviation) * newStartDirection;
            }

            TryCorrectPathDirection(currentPathStart, ref newStartDirection, segment.environmentType, false);

            var entranceCapabilities = _layout.traversalCapabilities.Last();

            RoomAbstract newStart = null;

            //place the starting room in the closest space that is in the appropriate environment.
            var startingEnv = currentPathStart.assignedRoomInfo.environmentType;
            var oldEnvBounds = _layout.environmentLimits[startingEnv];
            var newEnvBounds = _layout.environmentLimits[segment.environmentType];

            //find the closest point in the currentPath Start to the new environment
            var closestGlobalInStart = currentPathStart.GetLocalPosition(newEnvBounds.center.Int2D());
            closestGlobalInStart.x = Mathf.Clamp(closestGlobalInStart.x, 0, currentPathStart.width - 1);
            closestGlobalInStart.y = Mathf.Clamp(closestGlobalInStart.y, 0, currentPathStart.height - 1);
            closestGlobalInStart = currentPathStart.GetGridPosition(closestGlobalInStart);
            //Debug.Log("Closest point in starting room to " + segment.environmentType + " = " + closestGlobalInStart + " local / " + closestGlobalInStart + " global.");

            var newEnvTopLeft = new Int2D((int)newEnvBounds.min.x, (int)newEnvBounds.max.y);
            var closestPointInBounds = new Int2D(closestGlobalInStart.x - newEnvTopLeft.x, (int)newEnvBounds.height - newEnvTopLeft.y + closestGlobalInStart.y);
            closestPointInBounds.x = Mathf.Clamp(closestPointInBounds.x, 0, (int)newEnvBounds.width - 1);
            closestPointInBounds.y = Mathf.Clamp(closestPointInBounds.y, 0, (int)newEnvBounds.height - 1);
            closestPointInBounds = new Int2D(newEnvTopLeft.x + closestPointInBounds.x, newEnvTopLeft.y - ((int)newEnvBounds.height - 1 - closestPointInBounds.y));
            //Debug.Log("Closest point in " + segment.environmentType + " to starting room = " + closestPointInBounds);

            var validStarts = roomList.startingRoomInfos.Where(r => r.transitionsTo == startingEnv).ToList();
            var roomInfo = PickRandom(validStarts);
            var rect = roomInfo.GetRectAtGridPosition(closestPointInBounds);
            if (!_layout.MovePositionAwayFromRooms(ref rect, Buffer2D.zero, (newEnvBounds.center - oldEnvBounds.center).normalized, _layout.environmentLimits[segment.environmentType]))
            {
                Debug.LogError(segment.environmentType + " Starting Room position could not be placed far enough from other rooms! Seed (" + _layout.password + ")");
                yield return false; yield break;
            }

            closestPointInBounds = Int2D.GetRoomPosFromRect(rect);

            //create starting room and add it to layout at closest position                
            if (_layout.ValidRoomPlacement(closestPointInBounds, roomInfo.size))
            {
                newStart = _layout.Add(new RoomAbstract(roomInfo, closestPointInBounds), random.ZeroToMaxInt());
                newStart.expectedCapabilitiesIndex = _layout.traversalCapabilities.IndexOf(currentCapabilities);
            }
            else
            {
                Debug.LogError("Couldn't place starting room! (layout.ValidRoomPlacement failed)");
                Debug.LogError("Starting room could not be placed at" + closestPointInBounds);
                yield return false; yield break;
            }

            var pathFinder = new PathFinder();
            var newStartEntrance = newStart.assignedRoomInfo.possibleExits.First(ex => ex.toExit.CapabilitesSufficient(entranceCapabilities));
            var e = newStart.GetGridPosition(newStartEntrance.localGridPosition);
            var s = currentPathStart.GetClosestGlobalPositionWithExit(e);
            //Debug.Log("newStart.gridPosition: " + newStart.gridPosition);
            //Debug.Log("currentPathStart.gridPosition: " + currentPathStart.gridPosition);
            Func<RoomAbstract, Int2D, bool> validate = (r, pos) =>
            {
                var acceptableRoomTypes = new HashSet<RoomType> { RoomType.None, RoomType.BossRoom };
                var expected = r.GetExpectedCapabilitiesIndex(r.GetLocalPosition(pos));
                var ecIndexValid = (expected == currentPathStart.expectedCapabilitiesIndex);
                return (r == currentPathStart || r == newStart || (ecIndexValid && acceptableRoomTypes.Contains(r.assignedRoomInfo.roomType)));
            };

            path = pathFinder.GetPath(_layout, s, e, validate);

            if (path == null || path.Count == 0)
            {
                Debug.LogError("Couldn't find path from " + currentPathStart.gridPosition + " to at " + newStart.gridPosition);
                yield return false; yield break;
            }

            //path between the rooms.
            foreach (var step in AddRoomsToPath(path, startingEnv, currentCapabilities, false))            
            {
                if (!step)
                {
                    Debug.LogError("Couldn't path from " + currentPathStart.gridPosition + " to at " + newStart.gridPosition);
                    yield return false; yield break;
                }

                if (stepByStep) yield return true;
            }

            if (newStart.assignedRoomInfo.requiredExits.Count > 0)
            {
                var actualExit = newStart.exits.First();
                var reqExit = newStart.assignedRoomInfo.requiredExits.First();
                if (actualExit.direction != reqExit.direction || actualExit.localGridPosition != reqExit.localGridPosition)
                {
                    Debug.LogError("Linked up to bad starting room exit. Password: " + _layout.password);
                    yield return false; yield break;
                }
            }

            if (newStart == null)
            {
                Debug.LogError("AddLayoutGenerationSegment could not place a startingRoom.");
                yield return false; yield break;
            }
            else
            {
                newStart.isStartingRoom = _layout.roomAbstracts.Count == 0;
                newStart.isEnvironmentStart = true;
                currentPathStart = newStart;
                if (segment.environmentType == EnvironmentType.Cave)
                {
                    newStart.altPalette = random.Range(0, 4);
                    newStart.useAltTileset = random.value > 0.5f;
                }
                else if (segment.environmentType == EnvironmentType.CoolantSewers)
                {
                    newStart.altPalette = random.Range(0, 2);
                }
            }
        }

        segment.minCapabilitiesIndex = _layout.traversalItemsAdded;
        if (!segment.sourceRoom.branchRoom || segment.sourceIsBranchRoom)
        {
            segment.sourceRoom.expectedCapabilitiesIndex = _layout.traversalItemsAdded;
        }

        var hubDirection = segment.hubDirection;
        
        if (segment.hubDirectionDeviation != 0)
        {
            hubDirection = random.ZAngle(-segment.hubDirectionDeviation, segment.hubDirectionDeviation) * hubDirection;
        }

        var branchRooms = new List<RoomAbstract>();
        RoomAbstract saveRoomBranch = null;
        //one for each item and main path if no item at end
        var totalPathsInSegment = segment.itemAtEndOfHubPath ? segment.numberOfItems : segment.numberOfItems + 1;
        //one for each dead end
        if (segment.specificBranchEnds != null) { totalPathsInSegment += segment.specificBranchEnds.Count; }
        totalPathsInSegment += segment.deadEnds;

        //if no items after bosses, each boss needs their own path
        if (segment.bossPaths.Count > 0) 
        {
            if (segment.noItemsAfterBosses)
            {
                var bossOnMainPath = segment.bossPaths.Contains(0); //The boss is at the end of the hub path (the 0 index path)
                totalPathsInSegment += bossOnMainPath ? segment.bossPaths.Count - 1 : segment.bossPaths.Count;

                if (segment.itemAtEndOfHubPath) //noItemsAfterBosses override itemAtEndOfPath if bossPaths contains(0)
                {
                    Debug.LogWarning("itemAtEndOfHubPath set on a segment (" + segment.environmentType + " " + segment.id + " that has noItemsAfterBosses and a boss at the end of the hub path.");
                    segment.itemAtEndOfHubPath = false; //there can't be an item at the end of the main hub.
                    totalPathsInSegment++;
                }
            }

            //Check that bossPaths doesn't want to use a path index greater or eqaul to the total number of paths
            segment.bossPaths.Sort();
            var maxBossPath = segment.bossPaths.Last();
            if(maxBossPath >= totalPathsInSegment)
            {
                throw new Exception("Unhandled Exception! Segment " + segment.id + " wants to put bosses at the end of paths that have an index >= the total number of paths.");
            }
        }

        var direction = hubDirection.normalized;

        var deadEndStartIndex = totalPathsInSegment - segment.deadEnds;
        var specificStartRoomIndex = deadEndStartIndex - (segment.specificBranchEnds != null ? segment.specificBranchEnds.Count : 0);

        //segmentPath 0 will be the hub path and the rest will be paths branching off the hub path
        for (int currentSegmentPath = 0; currentSegmentPath < totalPathsInSegment; currentSegmentPath++)
        {
            var pathAttempts = 0;
            var specificRoom = currentSegmentPath >= specificStartRoomIndex && currentSegmentPath < deadEndStartIndex;
            var deadEnd = currentSegmentPath >= deadEndStartIndex;

            pathSuccessful = false;
            while (!pathSuccessful && pathAttempts < 4)
            {
                _layout.CreateUndo();
                pathSuccessful = true;
                pathAttempts++;

                var roomsToConnect = new List<RoomAbstract>() { currentPathStart };
                currentCapabilities = _layout.traversalCapabilities[_layout.traversalItemsAdded];

                bool isBossPath = segment.bossPaths.Contains(currentSegmentPath);

#region Pick Item at End of Path
                MajorItemInfo item = null;
                if (!deadEnd && !specificRoom && segment.numberOfItems > 0 && 
                    (segment.itemAtEndOfHubPath || currentSegmentPath > 0) && 
                    (!segment.noItemsAfterBosses || !isBossPath))
                {
                    var specificItems = segment.specificItems == null ? null : segment.specificItems.Where((i) => !_layout.allNonTraversalItemsAdded.Contains(i)).ToArray();
                    if (specificItems != null && specificItems.Length > 0)
                    {
                        item = ItemManager.items[specificItems[random.Range(0, specificItems.Length)]];
                        _layout.allNonTraversalItemsAdded.Add(item.type);
                    }
                    else if (segment.noTraversalItems)
                    {
                        var possibleItems = availableNonTraversalItems.FindAll((i) => !_layout.allNonTraversalItemsAdded.Contains(i.type));
                        item = possibleItems[random.Range(0, possibleItems.Count)];
                        _layout.allNonTraversalItemsAdded.Add(item.type);
                    }
                    else
                    {
                        item = traversalItemInfos[_layout.itemOrder[_layout.traversalItemsAdded]];
                    }
                }
#endregion

#region Set Path Length
                int pathLength = 0;
                if(currentSegmentPath == 0)
                {
                    pathLength = segment.hubPathLength;
                }
                else
                {
                    if (currentCapabilities.lastGainedAffordance.requiredEnvironmentalResistance != EnvironmentalEffect.None)
                    {
                        pathLength = segment.maxSpokeLength;
                    }
                    else
                    {
                        pathLength = segment.GetRandomBranchPathLength(random);
                    }
                }
#endregion

#region Place End Room
                //if this path is a spoke, set the direction perpendicular to the hub
                if (currentSegmentPath != 0)
                {
                    direction = Vector3.Cross(hubDirection, Vector3.forward).normalized;
                    direction = (Quaternion.Euler(0, 0, random.Range(-15, 15)) * direction).normalized;
                    if (random.value > 0.5f) { direction *= -1; }

                    if (!TryCorrectPathDirection(currentPathStart, ref direction, segment.environmentType, true))
                    {
                        Debug.LogError("Branch Room has no usable exits! Restarting Generation! Password: " + _layout.password);
                        yield return false; yield break;
                    }
                }

                //Find all usable exits that fit the specified direction and lead to the same environment (sometimes branch rooms cross over into the bounds of other environments)
                var usableExits = currentPathStart.GetUsableUnusedExits((e) => { return e.FitsDirection(direction); });

                if (usableExits == null || usableExits.Count <= 0)
                {
                    Debug.LogWarning("currentPathStart (" + currentPathStart.assignedRoomInfo.sceneName + ") had no usable exits that fit direction " + direction + "! Seed (" + _layout.password + ")");
                    Debug.LogWarning("Trying to use any usable exit");
                    usableExits = currentPathStart.GetUsableUnusedExits();

                    if (usableExits == null || usableExits.Count <= 0)
                    {
                        Debug.LogError("currentPathStart (" + currentPathStart.assignedRoomInfo.sceneName + ") had no usable exits! Seed (" + _layout.password + ")");
                        yield return false; yield break;
                    }
                }

                //using currentPathStart.gridPosition to set the end position is bad for branches that get used multiple times!
                var startRoomBranchPos = currentPathStart.GetGridPosition(PickRandom(usableExits).localGridPosition);

                //if this is the hub path in the first area, reduce the path length by the distance between the top-left gridPosition 
                //and the branch position chosen in the room. These rooms can be 1x5 or 5x1 rooms
                if (currentSegmentPath == 0 && segment.finalTraversalStart)
                {
                    pathLength = Mathf.Clamp(pathLength - (int)Int2D.Distance(currentPathStart.gridPosition, startRoomBranchPos), 2, pathLength);
                }

                RoomAbstract connectRoom = null;
                RoomAbstract endRoom = null; //the room at the end of the hub
                List<RoomInfo> endRoomInfos = null;

                Int2D pathEndPosition;
                //get a path end position;
                if(!GetValidPathEndPosition(out pathEndPosition, startRoomBranchPos.Vector2(), pathLength, ref direction, segment.environmentType))
                {
                    Debug.LogWarning("A valid path end position could not be placed! Seed (" + _layout.password + ")");
                    yield return false; yield break;
                }

                //Pick a Room
                if (!segment.noItemsAfterBosses || totalPathsInSegment > 0)
                {
                    if (item != null)
                    {
                        endRoomInfos = roomList.majorItemRoomInfos.FindAll(r => r.itemRoomLimitations.Count == 0 || r.itemRoomLimitations.Contains(item.type));
                    }
                    else if(specificRoom && segment.specificBranchEnds != null)
                    {
                        var list = roomList.GetList(segment.specificBranchEnds[specificStartRoomIndex].type);
                        var validRooms = segment.specificBranchEnds[specificStartRoomIndex].sceneNames;
                        endRoomInfos = list.FindAll(r => validRooms.Contains(r.sceneName));
                    }
                    else
                    {
                        endRoomInfos = roomList.generalRoomInfos.FindAll(r => r.SuitableBranchingRoom(currentCapabilities));
                    }
                }

                endRoomInfos = endRoomInfos.Where(r => r.traversalPaths.Count == 0).ToList();

                if(endRoomInfos == null || endRoomInfos.Count == 0)
                {
                    Debug.LogError("No Valid End Room Infos could be found. Seed (" +_layout.password + ")");
                    yield return false; yield break;
                }

                RoomInfo preBossRoomInfo = null;
                RoomInfo bossRoomInfo = null;
                RoomInfo endRoomInfo = null;
                Rect rect;
                if (isBossPath)
                {
                    bossRoomInfo = PickRandom(roomList.bossRoomInfos.FindAll(r => !_layout.bossesAdded.Contains(r.boss)));
                    if (!string.IsNullOrEmpty(segment.preBossRoom))
                    {
                        preBossRoomInfo = roomList.otherSpecialRoomsInfos.FirstOrDefault(r => r.sceneName == segment.preBossRoom);
                        rect = preBossRoomInfo.GetRectAtGridPosition(pathEndPosition);
                    }
                    else
                    {
                        rect = bossRoomInfo.GetRectAtGridPosition(pathEndPosition);
                    }
                }
                else
                {
                    endRoomInfo = PickRandom(endRoomInfos);
                    rect = endRoomInfo.GetRectAtGridPosition(pathEndPosition);
                }

                //Move it away from other rooms                      
                var buffer = Buffer2D.one;

                if (segment.environmentType == EnvironmentType.BeastGuts)
                {
                    buffer.bottom = direction.y > 0 ? 1 : 0;
                    buffer.top = direction.y < 0 ? 1 : 0;
                    buffer.left = direction.x > 0 ? 1 : 0;
                    buffer.right = direction.x < 0 ? 1 : 0;
                }

                if (!_layout.MovePositionAwayFromRooms(ref rect, buffer, direction, _layout.environmentLimits[segment.environmentType]))
                {
                    Debug.LogWarning("Path end position could not be placed far enough from other rooms! Seed (" + _layout.password + ")");
                    yield return false; yield break;
                }

                pathEndPosition = Int2D.GetRoomPosFromRect(rect);
                direction = Int2D.Direction(startRoomBranchPos, pathEndPosition);

                if (bossRoomInfo != null)
                {
                    _layout.bossesAdded.Add(bossRoomInfo.boss);
                    RoomAbstract bossRoom;

                    if(preBossRoomInfo != null)
                    {
                        var preBossRoom = new RoomAbstract(preBossRoomInfo, pathEndPosition);
                        preBossRoom.expectedCapabilitiesIndex = _layout.traversalItemsAdded;
                        _layout.Add(preBossRoom, random.ZeroToMaxInt());
                        bossRoom = TryPlaceOffExistingRoom(preBossRoom, direction, new List<RoomInfo> { bossRoomInfo }, currentCapabilities, currentCapabilities);
                        connectRoom = preBossRoom;
                    }
                    else
                    {    
                        bossRoom = new RoomAbstract(bossRoomInfo, pathEndPosition);
                        connectRoom = bossRoom;
                    }

                    bossRoom.expectedCapabilitiesIndex = _layout.traversalItemsAdded;

                    if (segment.noItemsAfterBosses)
                    {
                        //layout.Add(bossRoom) will get called below at end of region
                        endRoom = bossRoom;
                    }
                    else
                    {
                        _layout.Add(bossRoom, random.ZeroToMaxInt());
                        var potentialRooms = endRoomInfos.Where((r) => r.size.y == 1).ToList(); //this is only used at the end of the surface currently?
                        endRoom = TryPlaceOffExistingRoom(bossRoom, direction, potentialRooms, currentCapabilities, currentCapabilities);
                        if (segment.hasSaveRoom) { saveRoomBranch = currentPathStart; }
                    }
                }
                else if (endRoomInfos.Count > 0)
                {
                    endRoom = new RoomAbstract(endRoomInfo, pathEndPosition);
                }

                if(endRoom == null)
                {
                    Debug.LogError("End Room was null for seed " + _layout.password);
                    yield return false; yield break;
                }
                else
                {
                    endRoom.expectedCapabilitiesIndex = deadEnd ? currentPathStart.expectedCapabilitiesIndex : _layout.traversalItemsAdded;
                }

                if (item != null) { endRoom.majorItem = item.type; }

                //if this is the hub path, update the segment's main direction in case it was altered
                if (currentSegmentPath == 0) { hubDirection = direction; }

                _layout.Add(endRoom, random.ZeroToMaxInt());
#endregion
                if (stepByStep) yield return true;

                //is there a way to move this outside the for loop? the trouble is, the while loop may change mainDirection if the path fails.
#region create branch rooms and set finalBranch for hub
                if (currentSegmentPath == 0)
                {
                    var endRoomIsBranchRoom = !segment.itemAtEndOfHubPath && (!segment.noItemsAfterBosses || endRoom.assignedRoomInfo.roomType == RoomType.None);

                    if (segment.branchRoomsNeeded > 0)
                    {
                        branchRooms.Clear();
                        var branchRoomsToCreate = segment.branchRoomsNeeded;

                        if (segment.sourceIsBranchRoom)
                        {
                            currentPathStart.branchRoom = true;
                            branchRooms.Add(currentPathStart);
                            branchRoomsToCreate -= currentPathStart.assignedRoomInfo.possibleExits.Count > 6 ? 2 : 1;
                        }

                        //if there's no item at the end of the hub path, the end room will be a branch room (added below loop so its last in list)
                        if (endRoomIsBranchRoom)
                        {
                            branchRoomsToCreate -= endRoom.assignedRoomInfo.possibleExits.Count > 6 ? 2 : 1;
                        }

                        var closestGridPosition = currentPathStart.GetClosestGlobalGridPosition(endRoom.gridPosition);

                        for (int j = 0; j < branchRoomsToCreate; j++)
                        {
                            var t = 1f / (branchRoomsToCreate + 1) * (j + 1);
                            //for t(groupCount, j) 
                            //t(1, 0) = 0.5f
                            //t(2, 0) = 0.33f and t(2, 1) = 0.66f
                            //t(3, 0) = 0.25f and t(3, 1) = 0.5f, t(3, 2) = 0.75

                            var branchRoom = TryPlaceBranchRoom(closestGridPosition, pathEndPosition, t, roomList, currentCapabilities);

                            if (branchRoom == null)
                            {
                                Debug.LogError("Layout could not find a valid branch room");
                                yield return false; yield break;
                            }

                            //count rooms with more that 6 exits as two branches (anything larger than 1x2 or 2x1)
                            if (branchRoom.assignedRoomInfo.possibleExits.Count > 6)
                            {
                                j++;
                            }

                            branchRooms.Add(branchRoom);
                            _layout.Add(branchRoom, random.ZeroToMaxInt());
                            roomsToConnect.Add(branchRoom);
                        }

                        if (endRoomIsBranchRoom)
                        {
                            endRoom.branchRoom = true;
                            branchRooms.Add(endRoom);
                        }

                        if (segment.neededConnectorBranches != null && segment.neededConnectorBranches.Count > 0)
                        {
                            //Potential problem: the closest branch room is the same for two environments
                            //one environment has a viable alternative but the other does not
                            //the environment with the alternative taks the closest match, leaving the other one SOL
                            //For now I'm checking buried city first, and it seems okay
                            foreach (var env in segment.neededConnectorBranches)
                            {
                                var target = _layout.environmentLimits[env].center;
                                RoomAbstract closest = null;
                                var closestDistance = 0f;
                                foreach (var branch in branchRooms)
                                {
                                    var distance = Vector2.Distance(branch.gridPosition.Vector2(), target);
                                    if (closest == null || distance < closestDistance)
                                    {
                                        closest = branch;
                                        closestDistance = distance;
                                    }
                                }

                                var connectorKey = segment.id + env.ToString();
                                //don't let this branch room be counted as a connector twice AND used in the normal branch rooms
                                if (closest.assignedRoomInfo.possibleExits.Count <= 6 || _layout.connectorBranches.Values.Contains(closest.index))
                                {
                                    branchRooms.Remove(closest);
                                }

                                _layout.connectorBranches[connectorKey] = closest.index;
                            }
                        }

                 

                        if (segment.saveLastBranchForNextSegment)
                        {
                            if(branchRooms.Count == 0)
                            {
                                Debug.LogWarning("Segment " + segment.environmentType + " " + segment.id + " has no branch rooms, or all branch rooms were claimed as connectorBranches.");
                                throw new Exception("Unhandled Exception! Segment " + segment.id + " has saveLastBranchForNextSegment but has no branches left in branchRooms!");
                            }

                            segment.finalBranch = branchRooms.Last();
                            //don't let any spokes use the final branch
                            if (segment.finalBranch.assignedRoomInfo.possibleExits.Count <= 6)
                            {
                                branchRooms.Remove(segment.finalBranch);
                            }
                        }
                        else
                        {
                            if (branchRooms.Count > 0)
                            {
                                segment.finalBranch = PickRandom(branchRooms);
                            }
                            else
                            {
                                segment.finalBranch = null;
                                Debug.LogWarning("Segment " + segment.environmentType + " " + segment.id + " has no branch rooms, or all branch rooms were claimed as connectorBranches. segment.finalBranch will be null!");
                            }
                        }
                    }
                    else if (segment.saveLastBranchForNextSegment && endRoomIsBranchRoom)
                    {
                        segment.finalBranch = endRoom;
                    }
                }                
#endregion

                roomsToConnect.Add(connectRoom ?? endRoom); //the end room or boss room is the last room to connect to.

                if (stepByStep) yield return true;

                //connect all the required rooms
                for (int r = 0; r < roomsToConnect.Count - 1; r++)
                {
                    var start = roomsToConnect[r];
                    var end = roomsToConnect[r + 1];
                    var noPathToNumber = deadEnd || start.assignedRoomInfo.environmentType == EnvironmentType.BeastGuts || start.assignedRoomInfo.environmentType == EnvironmentType.Glitch;
                    var needsLockTowardsEnd = (_layout.traversalItemsAdded > 0 && start == currentPathStart && !noPathToNumber);
                    var capabilities = deadEnd ? start.expectedCapabilities : currentCapabilities;

                    Func<RoomAbstract, Int2D, bool> customValidate = null;

                    //avoids skippable boss on surface
                    if (end.assignedRoomInfo.roomType == RoomType.BossRoom && segment.environmentType == EnvironmentType.Surface)
                    {
                        customValidate = (room, pos) =>
                        {
                            return room.assignedRoomInfo.environmentType == end.assignedRoomInfo.environmentType &&
                                   (room == start || room == end || room.assignedRoomInfo.roomType == RoomType.BossRoom);
                        };
                    }

                    path = GetPath(start, end, customValidate);
                    if (path == null || path.Count == 0)
                    {
                        Debug.LogError("PathFinder failed to find path between " + start.roomID + " and " + end.roomID);
                        pathSuccessful = false;
                        break;
                    }

                    if (path.Count > 5)
                    {
                        ConvolutePath(path, segment.environmentType);
                    }

                    foreach (var step in AddRoomsToPath(path, segment.environmentType, capabilities, needsLockTowardsEnd))
                    {
                        if (!step)
                        {
                            Debug.LogError("AddRoomsToPath failed to path between " + start.roomID + " and " + end.roomID + " Password: " + _layout.password);
                            pathSuccessful = false;
                            break;
                        }

                        if (stepByStep) yield return true;
                    }
                }

                if (!pathSuccessful)
                {
                    _layout.Undo();
                    continue;
                }

                if (item != null && !segment.noTraversalItems)
                {
                    _layout.traversalItemsAdded++;
                }

                //need to figure out the next place to start if we need more items and have branch rooms left
                if (_layout.traversalItemsAdded < _layout.traversalItemCount || segment.noTraversalItems)
                {
                    currentPathStart = null;

                    if (branchRooms.Count > 0)
                    {
                        currentPathStart = PickRandom(branchRooms);

                        var finalBranch = segment.finalBranch == currentPathStart && segment.saveLastBranchForNextSegment;
                        //rooms with more than 6 exits can be used twice
                        if (currentPathStart.assignedRoomInfo.possibleExits.Count <= 6 || currentPathStart.exits.Count >= 3 ||
                            (currentPathStart.exits.Count >= 2 && (finalBranch || _layout.connectorBranches.ContainsValue(currentPathStart.index))))
                        {
                            branchRooms.Remove(currentPathStart);
                        }
                    }
                    else if (segment.finalBranch != null)
                    {
                        currentPathStart = segment.finalBranch;
                    }
                }
            }

            yield return pathSuccessful;
        }

#region hasLinkingPaths
        if(segment.hasLinkingPaths)
        {
            var segmentPaths = _layout.pathInfos.Where((p) => p.environment == segment.environmentType);

            var parallelPaths = new Dictionary<Direction, List<LayoutPathInfo>>();
            var hubHorizontal = Mathf.Abs(segment.hubDirection.x) > Mathf.Abs(segment.hubDirection.y);

            foreach (var path in segmentPaths)
            {
                if(path.primaryDirection.isHorizontal() == hubHorizontal)
                {
                    continue;
                }

                if(!parallelPaths.ContainsKey(path.primaryDirection))
                {
                    parallelPaths.Add(path.primaryDirection, new List<LayoutPathInfo>());
                }

                parallelPaths[path.primaryDirection].Add(path);
            }

            var multiples = parallelPaths.Values.Where((l) => l.Count > 1);

            var pathAbstracts = new Dictionary<Direction, List<RoomAbstract[]>>();
            foreach (var pathList in multiples)
            { 
                var pathDirection = pathList.First().primaryDirection;
                var pathRooms = new List<RoomAbstract[]>();
                pathAbstracts.Add(pathDirection, pathRooms);

                foreach (var path in pathList)
                {
                    pathRooms.Add(_layout.roomAbstracts.Where((a) =>
                    {
                        return a.parentPath == path.index && 
                               a.assignedRoomInfo.traversalPaths.Count == 0 &&
                               a.assignedRoomInfo.boss == BossName.None && 
                               a.majorItem == MajorItem.None;
                    }).ToArray());
                }
            }

            foreach (var pathGroup in pathAbstracts)
            {
                var groupDirection = pathGroup.Key;
                var paths = pathGroup.Value;

                var roomsToConnect = new List<RoomAbstract>();

                foreach (var path in paths)
                {
                    if(path.Length <= 0)
                    {
                        continue;
                    }

                    switch(groupDirection)
                    {
                        case Direction.Down:
                            roomsToConnect.Add(path.MinBy((r) => r.gridCenter.y));
                            break;
                        case Direction.Up:
                            roomsToConnect.Add(path.MaxBy((r) => r.gridCenter.y));
                            break;
                        case Direction.Left:
                            roomsToConnect.Add(path.MinBy((r) => r.gridCenter.x));
                            break;
                        case Direction.Right:
                            roomsToConnect.Add(path.MaxBy((r) => r.gridCenter.x));
                            break;
                    }
                }

                switch(groupDirection)
                {
                    case Direction.Up:
                    case Direction.Down:
                        roomsToConnect = roomsToConnect.OrderBy(r => r.gridCenter.x).ToList();
                        break;
                    case Direction.Left:
                    case Direction.Right:
                        roomsToConnect = roomsToConnect.OrderBy(r => r.gridCenter.y).ToList();
                        break;
                }

                if(roomsToConnect.Count < 2)
                {
                    continue;
                }

                for (int r = 0; r < roomsToConnect.Count - 1; r++)
                {                    
                    _layout.CreateUndo();
                    var start = roomsToConnect[r];
                    var end = roomsToConnect[r + 1];

                    if (start.expectedCapabilitiesIndex > end.expectedCapabilitiesIndex)
                    {
                        start = roomsToConnect[r+1];
                        end = roomsToConnect[r];
                    }

                    //Debug.Log("Connecting Start " + start.gridPosition + " with expectedAbilities " + start.expectedCapabilitiesIndex + " to End " + end.gridPosition + " with expected abilities " + end.expectedCapabilitiesIndex);

                    var sameCapIndex = start.expectedCapabilitiesIndex == end.expectedCapabilitiesIndex;
                    var startWithLock = !sameCapIndex && segment.environmentType != EnvironmentType.Glitch && segment.environmentType != EnvironmentType.BeastGuts;
                    var steps = 0;
                    var maxSteps = Int2D.Distance(start.gridCenter, end.gridCenter) * 3;

                    var capabilities = startWithLock ? end.expectedCapabilities : start.expectedCapabilities;

                    //Debug.Log("Start With Lock = " + startWithLock);

                    path = GetPath(start, end);
                    if (path == null || path.Count == 0)
                    {
                        Debug.LogError("PathFinder failed to find path between " + start.roomID + " and " + end.roomID);
                        pathSuccessful = false;
                        break;
                    }

                    foreach (var step in AddRoomsToPath(path, segment.environmentType, capabilities, startWithLock))
                    {
                        steps++;

                        if (!step || steps > maxSteps)
                        {
                            Debug.LogError("AddRoomsToPath failed to path between " + start.roomID + " and " + end.roomID + ". Password: " + _layout.password);
                            pathSuccessful = false;
                            break;
                        }

                        if (stepByStep) yield return true;
                    }

                    if(!pathSuccessful)
                    {
                        Debug.LogWarning("Couldn't path between " + start.gridPosition.ToString() + " and " + end.gridPosition.ToString() + ". Password: " + _layout.password);
                        _layout.Undo();
                        pathSuccessful = true;
                    }
                }

                yield return pathSuccessful;
            }
        }
#endregion

        if (segment.hasSaveRoom)
        {
            if (saveRoomBranch == null)
            {
                saveRoomBranch = PickRandom(branchRooms);
            }

            var exits = saveRoomBranch.GetUsableUnusedExits();
            if (exits.Count > 0)
            {
                var saveDirection = PickRandom(exits).direction;
                var saveRoom = TryPlaceOffExistingRoom(saveRoomBranch, saveDirection.ToVector2(), roomInfos[segment.environmentType].saveRoomInfos, currentCapabilities, currentCapabilities);
                if(saveRoom != null)
                {                     
                    _layout.Add(saveRoom, random.ZeroToMaxInt());
                }
                else
                {
                    Debug.LogWarning("Could not place save room, TryPlaceOffExistingRoom failed");
                }
            }
            else
            {
                Debug.LogWarning("Could not place save room, no free exits in branch");
            }

            yield return pathSuccessful;
        }
    }

    public RoomAbstract TryPlaceOffExistingRoom(RoomAbstract existingRoom, Vector2 direction, List<RoomInfo> roomInfos, TraversalCapabilities exitCapabilities, TraversalCapabilities entranceCapabilities)
    {
        RoomAbstract room = null;

        var possibleExits = existingRoom.GetAllUnusedExits();
        var preferredExits = possibleExits.FindAll(e => e.FitsDirection(direction));
        
        if(preferredExits.Count == 0)
        {
            preferredExits.AddRange(possibleExits);
            possibleExits.Clear();
        }

        while (preferredExits.Count > 0)
        {
            var inUnusedSpaces = preferredExits.FindAll((e) => !existingRoom.HasExitInLocalGridPosition(e.localGridPosition));

            ExitLimitations bestExit;

            if (inUnusedSpaces != null && inUnusedSpaces.Count > 0)
            {
                bestExit = PickRandom(inUnusedSpaces);
            }
            else
            {
                bestExit = PickRandom(preferredExits);
            }

            var entranceDireciton = bestExit.direction.Opposite();

            preferredExits.Remove(bestExit);

            if(preferredExits.Count == 0 && possibleExits.Count != 0)
            {
                preferredExits.AddRange(possibleExits);
                possibleExits.Clear();
            }

            var bestExitTarget = existingRoom.GetExitTarget(bestExit);
            var validRooms = roomInfos.FindAll(e => e.requiredExits.Count > 0 ? e.HasRequiredExitInDirection(entranceDireciton) : e.HasPossibleExitInDirection(entranceDireciton));

            if(validRooms.Count <= 0)
            {
                continue;
            }

            var roomInfo = PickRandom(validRooms);
            var bestEntrance = FindBestEntrance(roomInfo, entranceDireciton, direction);

            if (bestEntrance == null) continue;
            room = TryPlaceRoomByEntrance(roomInfo, bestExitTarget, bestEntrance, null, true);

            if (room != null)
            {
                var exitReq = GetRandomSuitableRequirements(exitCapabilities, bestExit.toExit, existingRoom.environmentalEffect);
                existingRoom.exits.Add(new ExitAbstract(bestExit, existingRoom, exitReq));
                room.exits.Add(new ExitAbstract(bestEntrance, room, GetRandomSuitableRequirements(entranceCapabilities, bestEntrance.toExit, room.environmentalEffect)));

                room.parentPath = existingRoom.parentPath;
                var minIndex = _layout.GetIndexOfFirstSuitableCapabilities(exitReq);
                room.expectedCapabilitiesIndex = existingRoom.expectedCapabilitiesIndex > minIndex ? existingRoom.expectedCapabilitiesIndex : minIndex;

                if(roomInfo.traversalPaths.Count > 0)
                {
                    for (int i = 0; i < roomInfo.traversalPaths.Count; i++)
                    {
                        var limitations = roomInfo.traversalPaths[i].limitations;
                        room.traversalPathRequirements.Add(GetRandomSuitableRequirements(entranceCapabilities, limitations, room.environmentalEffect));
                    }
                }

                break;
            }
        }

        return room;
    }

    public RoomAbstract TryPlaceBranchRoom(Int2D start, Int2D end, float t, RoomLists roomList, TraversalCapabilities currentCapabilities)
    {
        RoomAbstract branchingRoom = null;

        //var direction = (end - start).Vector2().normalized;
        var branchingPosition = Int2D.Lerp(start, end, t);

        if(!_layout.MovePositionAwayFromRooms(ref branchingPosition, Buffer2D.one, Int2D.Direction(start, end)))
        {
            return null;
        }

        var branchingRoomInfos = roomList.generalRoomInfos.FindAll(r => _layout.ValidRoomPlacement(branchingPosition, r.size) &&  r.SuitableBranchingRoom(currentCapabilities));

        while (branchingRoom == null && branchingRoomInfos.Count > 0)
        {
            var branchingRoomInfo = _layout.WeightedRoomPick(branchingRoomInfos, random, true);
            branchingRoomInfos.Remove(branchingRoomInfo);

            if (_layout.ValidRoomPlacement(branchingPosition, branchingRoomInfo.size, 1))
            {
                branchingRoom = new RoomAbstract(branchingRoomInfo, branchingPosition) { branchRoom = true };
                branchingRoom.expectedCapabilitiesIndex = _layout.traversalCapabilities.IndexOf(currentCapabilities);

                for (int i = 0; i < branchingRoomInfo.traversalPaths.Count; i++)
                {
                    var limitations = branchingRoomInfo.traversalPaths[i].limitations;
                    branchingRoom.traversalPathRequirements.Add(GetRandomSuitableRequirements(currentCapabilities, limitations, branchingRoom.environmentalEffect));
                }
            }
        }

        return branchingRoom;
    }

    public bool GetValidPathEndPosition(out Int2D pathEndPosition, Vector2 startPosition, float pathLength, ref Vector2 direction, EnvironmentType targetEnvironment)
    {
        pathEndPosition = (startPosition + direction * pathLength).Int2D();
        if (_layout.ContainsPosition(pathEndPosition, targetEnvironment)) return true;

        //try to angle up
        var angle = 0;
        Int2D newPosition = pathEndPosition;
        var originalDirection = direction;
        while (angle <= 36)
        {
            angle += 3;
            direction = (Quaternion.Euler(0, 0, angle) * originalDirection).normalized;
            newPosition = (startPosition + direction * pathLength).Int2D();
            if (_layout.ContainsPosition(newPosition, targetEnvironment))
            {
                pathEndPosition = newPosition;
                return true;
            }
        }

        //try to angle down
        while (angle >= -36)
        {
            angle -= 3;
            direction = (Quaternion.Euler(0, 0, angle) * originalDirection).normalized;
            newPosition = (startPosition + direction * pathLength).Int2D();
            if (_layout.ContainsPosition(newPosition, targetEnvironment))
            {
                pathEndPosition = newPosition;
                return true;
            }
        }

        direction = originalDirection;
        Debug.LogWarning("GetValidPathEndPosition could not get a valid end position!");
        return false;
    }

    /// <summary>
    /// Flips directions if currentPathStart is close to the edge of the map
    /// Examines exits in the pathStart to see what exits it has available
    /// If the direction's y value is greater than its x, it will match the direction's sign to the available exits.
    /// For instance, if there's no up exit in the room and the direction's y is positive, it will set it negative if down is available. (If neither are available it will set it to 0)
    /// It will then flip the x if no suitable exit exists
    /// If the direction's x value is greater than its y, it applies the same logic as about with emphasis on the x.
    /// </summary>
    /// <param name="pathStart"></param>
    /// <param name="direction"></param>
    /// <param name="currentEnvironement"></param>
    /// <returns></returns>
    public bool TryCorrectPathDirection(RoomAbstract pathStart, ref Vector2 direction, EnvironmentType currentEnvironement, bool leaveEnvBuffer)
    {
        if(pathStart == null)
        {
            Debug.LogError("TryCorrectPathDirection passed null pathStart!");
            return false;
        }

        var layout = pathStart.layout;
        var limits = leaveEnvBuffer ? pathStart.layout.environmentLimits[currentEnvironement] : new Rect() { xMin = 0, yMin = 0, xMax = layout.width, yMax = layout.height };
        var startRect = pathStart.gridBounds;
        var nearXMin = pathStart.gridBounds.xMin <= limits.xMin + 3;
        var nearXMax = pathStart.gridBounds.xMax >= limits.xMax - 3;
        var nearYMin = pathStart.gridBounds.yMin <= limits.yMin + 3;
        var nearYMax = pathStart.gridBounds.yMax >= limits.yMax - 3;

        direction.Normalize();

        //if we have a tall room in a narrow vertical bounds and are trying to go up or down, go left or right instead
        //if we have a wide room in a narrow horizontal bounds and are trying to go left or right, go up or down, instead
        if ((nearYMax && nearYMin && Mathf.Abs(direction.y) > 0.66f) ||
            (nearXMax && nearXMin && Mathf.Abs(direction.x) > 0.66f))
        {
            var oldX = direction.x;
            direction.x = direction.y;
            direction.y = oldX;
        }

        //flip any directions that move closer to a nearby limit
        if ((direction.x < 0 && nearXMin) || (direction.x > 0 && nearXMax)) { direction.x *= -1; }
        if ((direction.y < 0 && nearYMin) || (direction.y > 0 && nearYMax)) { direction.y *= -1; }

        var up = false;
        var down = false;
        var left = false;
        var right = false;

        var usableExits = pathStart.GetUsableUnusedExits();
        if (usableExits.Count == 0) { return false; }

        foreach (var exit in usableExits)
        {
            if (layout.ContainsPosition(pathStart.GetExitTarget(exit), currentEnvironement))
            {
                switch (exit.direction)
                {
                    case Direction.Up:
                        up = true;
                        break;
                    case Direction.Down:
                        down = true;
                        break;
                    case Direction.Left:
                        left = true;
                        break;
                    case Direction.Right:
                        right = true;
                        break;
                }
            }
        }

        if (!up && !down && !left && !right)
        {
            return false;
        }

        bool greaterY = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);
        
        if (greaterY)
        {
            if (direction.y > 0)
            {
                direction.y = up ? direction.y : (down && !nearYMin ? -direction.y : 0);
            }
            else
            {
                //if we're not near the bottom limit we could go down instead
                direction.y = down ? direction.y : (up && !nearYMax ? -direction.y : 0);
            }

            //go ahead and make x better if we can, but don't set it to 0
            if ((direction.x > 0 && !right && left && !nearXMin) || 
                (direction.x < 0 && !left && right && !nearXMax))
            {
                direction.x = -direction.x;
            }
        }
        else
        {
            if (direction.x > 0)
            {
                direction.x = right ? direction.x : (left && !nearXMin ? -direction.x : 0);
            }
            else
            {
                direction.x = left ? direction.x : (right && !nearXMax ? -direction.x : 0);
            }

            //go ahead and make y better if we can, but don't set it to 0
            if ((direction.y > 0 && !up && down && !nearYMin) ||
                (direction.y < 0 && !down && up && !nearYMax))
            {
                direction.y = -direction.y;
            }
        }

        direction.Normalize();

        return true;
    }

    public List<MinorItemType> GenerateMinorItemPool(int size)
    {
        var pool = new List<MinorItemType>();

        var minorItems = new List<MinorItemType>(availableMinorItems);
        minorItems.Remove(MinorItemType.BlueScrap);
        minorItems.Remove(MinorItemType.RedScrap);
        minorItems.Remove(MinorItemType.GreenScrap);
        minorItems.Remove(MinorItemType.GlitchScrap);

        var scrapCount = 3;
        var allowGlitchScrap = availableMinorItems.Contains(MinorItemType.GlitchScrap);
        for (int i = 0; i < scrapCount; i++)
        {
            var scrapTypes = new MinorItemType[] { MinorItemType.RedScrap, MinorItemType.BlueScrap, MinorItemType.GreenScrap };
            if (allowGlitchScrap && random.value > 0.5f) scrapTypes[random.Range(0, scrapTypes.Length)] = MinorItemType.GlitchScrap;
            foreach (var scrap in scrapTypes) { pool.Add(scrap); }
        }

        var unevenSplit = Constants.UnevenSplit(size-(scrapCount*3), minorItems.Count, random);
        for (int i = 0; i < minorItems.Count; i++)
        {
            for (int j = 0; j < unevenSplit[i]; j++)
            {
                pool.Add(minorItems[i]);
            }
        }
        return pool;
    }

    public IEnumerable<bool> AddRoomsToPath(List<Int2D> path, EnvironmentType environmentType, TraversalCapabilities currentCapabilities, bool startPathWithLock = false)
    {
        var originalStartPos = path.First();
        var originalEndPos = path.Last();
        RoomAbstract start = _layout.GetRoomAtPositon(originalStartPos);
        RoomAbstract end = _layout.GetRoomAtPositon(originalEndPos);
        var connectedToEndRoom = false;

        int startExpectedCapabilitiesIndex = start.GetExpectedCapabilitiesIndex(start.GetLocalPosition(originalStartPos));

        /*If the path needs to start with a lock because the start and end room have different expected capabilities
         * we need to explore the path and change the start room to the room at the furthest point on the path
         * with the same expected capabilities */
        if (startPathWithLock && startExpectedCapabilitiesIndex != end.expectedCapabilitiesIndex)
        {
            int furthestPoint = 0;
            for (int i = 0; i < path.Count; i++)
            {
                var room = _layout.GetRoomAtPositon(path[i]);
                
                if (room != null)
                {
                    var expectedCap = room.GetExpectedCapabilitiesIndex(room.GetLocalPosition(path[i]));
                    if (expectedCap == startExpectedCapabilitiesIndex)
                    {
                        start = room;
                        furthestPoint = i;
                    }
                }
            }

            if (furthestPoint > 0) { path.RemoveRange(0, furthestPoint + 1); }
        }

        var expectedCapabilitiesIndex = _layout.traversalCapabilities.IndexOf(currentCapabilities);
        if (startExpectedCapabilitiesIndex > expectedCapabilitiesIndex) { expectedCapabilitiesIndex = startExpectedCapabilitiesIndex; }
        var previousCapabilitiesIndex = startExpectedCapabilitiesIndex;

        var mainDirection = (end.gridPosition - start.gridPosition).Vector2().normalized;
        var mainDirectionHorizontal = Mathf.Abs(mainDirection.x) > Mathf.Abs(mainDirection.y);

        var newPath = new LayoutPathInfo()
        {
            index = _layout.pathInfos.Count,
            primaryDirection = mainDirectionHorizontal ? (mainDirection.x > 0 ? Direction.Right : Direction.Left) : (mainDirection.y > 0 ? Direction.Up : Direction.Down),
            environment = environmentType,
        };

        start.parentPath = start.parentPath < 0 ? newPath.index : start.parentPath;
        end.parentPath = newPath.index;
        _layout.pathInfos.Add(newPath);

        bool lastRoomWasLockTowardsEnd = false;
        bool needsLockTowardsEnd = false;
        var roomList = roomInfos[environmentType];

        Int2D endPoint = path.Last();

        //Get a list of permitted rooms and also trim the start and exit rooms
        start = GetNewPathStart(start, ref path);

        if (path.Count == 0)
        {
            if (start == end)
            {
                //reset start because start and end are connected
                start = _layout.GetRoomAtPositon(originalStartPos);
            }
            else
            {
                Debug.LogError("GetNewPathStart deleted all the nodes!");
                yield return false;
            }
        }

        var finishedCropping = false;
        Int2D endMinPos;
        Int2D endMaxPos;
        while (!finishedCropping)
        {
            finishedCropping = true;
            if(CropPathNodes(end, ref path, out endMinPos, out endMaxPos))
            {
                endPoint = endMinPos;
            }

            var connectedRooms = end.GetConnectedRooms();
            foreach (var point in path)
            {
                var room = _layout.GetRoomAtPositon(point);
                if (connectedRooms.Contains(room))
                {
                    finishedCropping = false;
                    end = room;
                    break;
                }
            }

            if(stepByStep) { yield return true; }
        }

        if (!path.Contains(endPoint)) { path.Add(endPoint); }

        if (stepByStep) { yield return true; }

        //rooms that might be pathed through + end room
        var permittedRooms = new List<RoomAbstract>();
        //var boundary = new Rect() { xMin = path[0].x, xMax = path[0].x, yMin = path[0].y, yMax = path[0].y };
        foreach (var point in path)
        {
            //if (point.x < boundary.xMin) { boundary.xMin = point.x; }
            //if (point.x > boundary.xMax) { boundary.xMax = point.x; }
            //if (point.y < boundary.yMin) { boundary.yMin = point.y; }
            //if (point.y > boundary.yMax) { boundary.yMax = point.y; }

            var room = _layout.GetRoomAtPositon(point);
            if (room != null && !permittedRooms.Contains(room)) permittedRooms.Add(room);
        }

        //Set environment effects for new start and end        
        EnvironmentalEffect environmentalEffect;
        bool useAltTileset;
        int altPalette;
        GetEnvironmentalEffect(start, end, currentCapabilities, environmentType, out environmentalEffect, out useAltTileset, out altPalette);

        bool lastRoomSolvedWithTraversalPath = false;
        ExitLimitations startRoomBestExit = null;
        while (!connectedToEndRoom)
        {
            //Likely unnecessary here. Was for when CreatePathBetweenRooms pathed through other environments
            if (start.assignedRoomInfo.roomType != RoomType.TransitionRoom && start.assignedRoomInfo.environmentType != environmentType)
            {
                environmentType = start.assignedRoomInfo.environmentType;
                roomList = roomInfos[start.assignedRoomInfo.environmentType];
            }

            bool bestExitUseLastAffordance = lastRoomWasLockTowardsEnd && !lastRoomSolvedWithTraversalPath;

            if (startRoomBestExit == null) //find the first exit
            {
                if (startPathWithLock)
                {
                    startPathWithLock = false;
                    startRoomBestExit = FindPathExit(start, ref path, currentCapabilities, true, permittedRooms);

                    if (startRoomBestExit != null) //A door off the starting room can be the needed lock!
                    {
                        lastRoomWasLockTowardsEnd = true;
                        bestExitUseLastAffordance = true;
                    }
                    else
                    {
                        needsLockTowardsEnd = true;
                        startRoomBestExit = FindPathExit(start, ref path, currentCapabilities, false, permittedRooms);
                        if (startRoomBestExit == null)
                        {
                            Debug.LogError("AddRoomsToPath couldn't find a valid exit in the room where it started at " + start.gridPosition);
                            yield return false;
                        }
                    }
                }
                else
                {
                    startRoomBestExit = FindPathExit(start, ref path, currentCapabilities, bestExitUseLastAffordance, needsLockTowardsEnd ? null : permittedRooms);

                    if (startRoomBestExit == null)
                    {
                        var error = "AddRoomsToPath couldn't find a valid exit in the last room added at " + start.gridPosition.ToString() + ".";
                        if(start.assignedRoomInfo != null)
                        {
                            error += " " + start.assignedRoomInfo.sceneName;
                        }
                        Debug.LogError(error);
                        yield return false;
                    }
                }
            }

            //This seems to work for assigning the relevant info to traversal paths in CreatePathBetweenRooms...
            //but it seems like an odd place to do it and it may not handle multiple traversal paths in the same room well
            var useLastAff = lastRoomWasLockTowardsEnd && lastRoomSolvedWithTraversalPath;
            if (!AddTraversalPathIfNeeded(start, useLastAff, currentCapabilities, previousCapabilitiesIndex, startRoomBestExit.localGridPosition))
            {
                Debug.LogError("Seed (" + _layout.password + ") has a traversal path room at " + start.gridPosition + " that can't GetRandomSuitableRequirements for passed in capabilities.");
                Debug.LogError("Error occured while pathing from " + originalStartPos + " to " + originalEndPos);
                Debug.LogError(currentCapabilities.ToString());
                yield return false;
            }

            TraversalRequirements bestExitRequirements;
            if (bestExitUseLastAffordance)
            {
                bestExitRequirements = currentCapabilities.lastGainedAffordance;
            }
            else
            {
                bestExitRequirements = GetRandomSuitableRequirements(currentCapabilities, startRoomBestExit.toExit, start.environmentalEffect);
            }
            
            if (bestExitRequirements == null)
            {
                Debug.LogError("AddRoomsToPath could not find valid exit requirements for last room added at " + start.gridPosition.ToString() + ".");
                yield return false;
            }

            Direction bestExitOpposite = startRoomBestExit.direction.Opposite();
            Int2D bestExitTarget = start.GetExitTarget(startRoomBestExit);
            var roomAtBestExit = _layout.GetRoomAtPositon(bestExitTarget);

            //if connected to a permitted room move on
            if (permittedRooms.Contains(roomAtBestExit))
            {
                if (!start.IsConnectedTo(roomAtBestExit))
                {
                    var exit = new ExitAbstract(startRoomBestExit, start, bestExitRequirements);
                    start.exits.Add(exit);
                    var entrance = roomAtBestExit.GetReciprocalExit(exit);
                    if (entrance == null)
                    {
                        Debug.LogError("AddRoomsToPath could not find valid entrance in room at " + roomAtBestExit.gridPosition.ToString() + ".");
                        yield return false;
                    }

                    TraversalRequirements eRequirements = null;

                    //For that damned area 1 starting room
                    if (roomAtBestExit.assignedRoomInfo.roomType == RoomType.StartingRoom &&
                        roomAtBestExit.assignedRoomInfo.environmentType == _layout.environmentOrder[1] &&
                        start.assignedRoomInfo.environmentType == _layout.environmentOrder[0])
                    {
                        var finalCapabilities = _layout.traversalCapabilities[_layout.traversalCapabilities.Count - 1].lastGainedAffordance;
                        if (entrance.toExit.CanSatisfyRequirements(finalCapabilities)) { eRequirements = finalCapabilities; }
                    }

                    if (eRequirements == null)
                    {
                        if (needsLockTowardsEnd && !bestExitUseLastAffordance)
                        {
                            if (entrance.toExit.CanSatisfyRequirements(currentCapabilities.lastGainedAffordance))
                            {
                                eRequirements = currentCapabilities.lastGainedAffordance;
                                bestExitUseLastAffordance = true;
                            }
                            else
                            {
                                Debug.LogError("AddRoomsToPath could not find valid exit in room at " + roomAtBestExit.gridPosition.ToString() + ".");
                                yield return false;
                            }
                        }
                        else
                        {
                            eRequirements = GetRandomSuitableRequirements(currentCapabilities, entrance.toExit, environmentalEffect);
                        }
                    }

                    roomAtBestExit.exits.Add(new ExitAbstract(entrance, roomAtBestExit, eRequirements));
                }

                if (needsLockTowardsEnd && !bestExitUseLastAffordance)
                {
                    Debug.LogWarning("AddRoomsToPath arrived at end room before lock could be placed.");
                    yield return false;
                }

                connectedToEndRoom = roomAtBestExit == end;
                if (!connectedToEndRoom) { start = GetNewPathStart(roomAtBestExit, ref path); }
                startRoomBestExit = null;
                permittedRooms.Remove(roomAtBestExit);
                yield return true;
                continue;
            }

            List<RoomAbstract> potentialNewRooms = new List<RoomAbstract>();
            List<ExitLimitations> bestEntrances = new List<ExitLimitations>();
            List<ExitLimitations> bestExits = new List<ExitLimitations>();
            List<TraversalPath> tPathSolutions = new List<TraversalPath>();

            //find ways to extend the path
            lastRoomSolvedWithTraversalPath = false;
            foreach (var roomInfo in roomList.generalRoomInfos)
            {
                if (roomInfo.traversalLimitations != null && !roomInfo.traversalLimitations.CapabilitesSufficient(currentCapabilities, environmentalEffect)) continue;

                //Don't use rooms with more than 2 requiredExits to complete path. ReplaceRooms can handle that
                if (roomInfo.requiredExits.Count > 2) continue;

                TraversalPath traversalPathLock = null;
                bool validTraversalPaths = true;
                foreach (var tPath in roomInfo.traversalPaths)
                {
                    if (!tPath.limitations.CapabilitesSufficient(currentCapabilities))
                    {
                        validTraversalPaths = false;
                        break;
                    }

                    if (needsLockTowardsEnd)
                    {
                        if (tPath.limitations.CanSatisfyRequirements(currentCapabilities.lastGainedAffordance))
                        {
                            traversalPathLock = tPath;
                        }
                        else
                        {
                            validTraversalPaths = false;
                            break;
                        }
                    }
                }

                if (!validTraversalPaths) continue;

                var requiredEntrance = roomInfo.requiredExits.FirstOrDefault((e) => e.direction == bestExitOpposite);
                //if the room has 2 required exits and neither can be used as an entrance skip this room
                if (roomInfo.requiredExits.Count > 1 && requiredEntrance == null) continue;

                bool usable = false;
                int bestIndexReached = 0;
                ExitLimitations bestEntrance = null;
                RoomAbstract bestRoomAbstract = null;
                ExitLimitations newBestExit = null;

                foreach (var entrance in roomInfo.possibleExits)
                {
                    if (entrance.direction != bestExitOpposite) continue;

                    //don't allow entrances we can't use
                    if (!entrance.toExit.CapabilitesSufficient(currentCapabilities, environmentalEffect)) continue;

                    //if a required entrance was found, we have to use it
                    if (requiredEntrance != null && !entrance.MatchesPosAndDir(requiredEntrance)) continue;

                    //only look at corner exits
                    if (roomInfo.size.x > 1 && !(entrance.localGridPosition.x == 0 || entrance.localGridPosition.x == roomInfo.size.x - 1)) continue;
                    if (roomInfo.size.y > 1 && !(entrance.localGridPosition.y == 0 || entrance.localGridPosition.y == roomInfo.size.y - 1)) continue;

                    if (traversalPathLock != null)
                    {
                        if (traversalPathLock.reciprocal)
                        {
                            if (!traversalPathLock.from.ContainsLocalGridPosition(entrance.localGridPosition) &&
                                !traversalPathLock.to.ContainsLocalGridPosition(entrance.localGridPosition)) continue;
                        }
                        else if (!traversalPathLock.from.ContainsLocalGridPosition(entrance.localGridPosition))
                        {
                            continue;
                        }
                    }

                    var roomAbstract = TryPlaceRoomByEntrance(roomInfo, bestExitTarget, entrance, path, permittedRooms);
                    if (roomAbstract == null) continue;

                    //Reject rooms that would skip over path points created by convolution
                    bool roomEntered = false;
                    bool roomExited = false;
                    bool reject = false;
                    foreach (var p in path)
                    {
                        var inRoom = roomAbstract.ContainsGridPosition(p);
                        if (!roomEntered)
                        {
                            roomEntered = inRoom;
                        }
                        else if (!roomExited)
                        {
                            roomExited = !inRoom;
                        }
                        else if(inRoom)
                        {
                            reject = true;
                            break;
                        }
                    }

                    if (reject) { continue; }

                    //are any of the exits towards the next room usable and does this placement put the room in the best location?
                    foreach (var exit in roomInfo.possibleExits)
                    {
                        //we can't use the entrance as the exit
                        if (exit == entrance) continue;

                        var target = roomAbstract.GetExitTarget(exit);
                        if (!path.Contains(target)) continue;

                        //If this room has one required exit and its not being used as the entrance we have to use it
                        if (roomInfo.requiredExits.Count == 1 && requiredEntrance == null && !roomInfo.HasRequiredExitMatch(exit)) continue;

                        var roomAtTarget = _layout.GetRoomAtPositon(target);
                        if (roomAtTarget != null)
                        {
                            if (!permittedRooms.Contains(roomAtTarget)) continue;
                            if (!roomAtTarget.CanConnectToExit(target, exit.direction)) continue;
                        }

                        //don't use exits we can't use!
                        if (!exit.toExit.CapabilitesSufficient(currentCapabilities, environmentalEffect)) continue;

                        //If this room has 2 required exits, one of them is the entrance, the other we have to use it
                        if (roomInfo.requiredExits.Count > 1 && !roomInfo.HasRequiredExitMatch(exit)) continue;

                        var satisfiesLastAffordance = exit.toExit.CanSatisfyRequirements(currentCapabilities.lastGainedAffordance);

                        //unless a room only has 2 possible exits /////(or is in the beast guts or glitch)
                        if (roomInfo.possibleExits.Count > 2)
                            //&&
                            //roomInfo.environmentType != EnvironmentType.BeastGuts &&
                            //roomInfo.environmentType != EnvironmentType.Glitch)
                        {
                            //don't move vertically with a wide room
                            if(roomInfo.size.x > 1)
                            {
                                if (exit.localGridPosition.x == entrance.localGridPosition.x) continue;
                                //only use far exits for tunnels
                                if (roomInfo.size.y == 1 && exit.localGridPosition.x != 0 && exit.localGridPosition.x != roomInfo.size.x-1) continue; 
                            }

                            //don't move horizontally with tall room
                            var notRequired = !satisfiesLastAffordance || !needsLockTowardsEnd || currentCapabilities.lastGainedAffordance.minEffectiveJumpHeight < 11;
                            if (roomInfo.size.y > 1 && notRequired)
                            {
                                if (exit.localGridPosition.y == entrance.localGridPosition.y) continue;
                                //only use far exits for tunnels
                                if (roomInfo.size.x == 1 && exit.localGridPosition.y != 0 && exit.localGridPosition.y != roomInfo.size.y - 1) continue;
                            }
                        }

                        //this exits needs to satisfy the last gained affordance if we're expecting a lock (and a traversal path didn't satisfy it)
                        if (needsLockTowardsEnd)
                        {
                            if (traversalPathLock == null)
                            {
                                if (!satisfiesLastAffordance) continue;
                            }
                            else //exit must be on the side of the path the entrance is not on
                            {
                                if (traversalPathLock.to.ContainsLocalGridPosition(entrance.localGridPosition) &&
                                    !traversalPathLock.from.ContainsLocalGridPosition(exit.localGridPosition)) continue;

                                if (traversalPathLock.from.ContainsLocalGridPosition(entrance.localGridPosition) &&
                                    !traversalPathLock.to.ContainsLocalGridPosition(exit.localGridPosition)) continue;
                            }
                        }

                        var i = path.IndexOf(target);
                        if (i >= bestIndexReached)
                        {
                            bestIndexReached = i;
                            bestEntrance = entrance;
                            newBestExit = exit;
                            bestRoomAbstract = roomAbstract;
                            usable = true;
                        }
                    }
                }

                if(usable)
                {
                    potentialNewRooms.Add(bestRoomAbstract);
                    bestEntrances.Add(bestEntrance);
                    bestExits.Add(newBestExit);
                    tPathSolutions.Add(traversalPathLock);
                }
            }

            if (potentialNewRooms.Count == 0)
            {
                var warning = "AddRoomsToPath from " + originalStartPos + " to " + originalEndPos + " could not proceed past " + bestExitTarget;
                if (needsLockTowardsEnd) { warning += " using " + currentCapabilities.lastGainedAffordance.ToString(); }
                Debug.LogWarning(warning);
                yield return false;
            }

            if (potentialNewRooms.Count > 1)
            {
                var repeat = potentialNewRooms.FirstOrDefault(r => r.assignedRoomInfo.sceneName == start.assignedRoomInfo.sceneName);
                if (repeat != null)
                {
                    var indexOf = potentialNewRooms.IndexOf(repeat);
                    potentialNewRooms.RemoveAt(indexOf);
                    bestEntrances.RemoveAt(indexOf);
                    bestExits.RemoveAt(indexOf);
                    tPathSolutions.RemoveAt(indexOf);
                }
            }

            var newRoom = _layout.WeightedRoomPick(potentialNewRooms, random, true);
            var index = potentialNewRooms.IndexOf(newRoom);
            var newEntrance = bestEntrances[index];
            lastRoomSolvedWithTraversalPath = tPathSolutions[index] != null;

            newRoom.environmentalEffect = environmentalEffect;
            newRoom.altPalette = altPalette;
            newRoom.useAltTileset = useAltTileset;
            newRoom.expectedCapabilitiesIndex = needsLockTowardsEnd ? previousCapabilitiesIndex : expectedCapabilitiesIndex;

            start.exits.Add(new ExitAbstract(startRoomBestExit, start, bestExitUseLastAffordance ? currentCapabilities.lastGainedAffordance : bestExitRequirements));
            var entranceRequirements = GetRandomSuitableRequirements(currentCapabilities, newEntrance.toExit, newRoom.environmentalEffect);
            if (entranceRequirements == null)
            {
                Debug.LogError("Entrance " + newEntrance.localGridPosition + " " + newEntrance.direction.ToString() + " in " + newRoom.assignedRoomInfo.sceneName + " can't generate appropriate requirements. Password: " + _layout.password);
            }
            newRoom.exits.Add(new ExitAbstract(newEntrance, newRoom, entranceRequirements));
            newRoom.parentPath = newPath.index;
            _layout.Add(newRoom, random.ZeroToMaxInt());
            
            start = newRoom;
            startRoomBestExit = bestExits[index];
            lastRoomWasLockTowardsEnd = needsLockTowardsEnd;
            needsLockTowardsEnd = false;

            var newTarget = start.GetExitTarget(startRoomBestExit);
            var tIndex = path.IndexOf(newTarget);
            if (tIndex > 0) { path.RemoveRange(0, tIndex); }

            yield return true;
        }
    }

    /// <returns>returns true if path not needed or path successfully added</returns>
    public bool AddTraversalPathIfNeeded(RoomAbstract room, bool useLastGainedAffordance, TraversalCapabilities capabilities, int previousCapabilitiesIndex, Int2D exitLocale)
    {
        bool success = true;

        if (room.assignedRoomInfo.traversalPaths.Count != room.traversalPathRequirements.Count)
        {
            for (int i = 0; i < room.assignedRoomInfo.traversalPaths.Count; i++)
            {
                if (i < room.traversalPathRequirements.Count) { continue; }
                var tp = room.assignedRoomInfo.traversalPaths[i];

                if (useLastGainedAffordance && tp.limitations.CanSatisfyRequirements(capabilities.lastGainedAffordance))
                {
                    room.traversalPathRequirements.Add(capabilities.lastGainedAffordance);
                    room.expectedCapabilitiesIndex = previousCapabilitiesIndex;
                    var gridPos = tp.to.ContainsLocalGridPosition(exitLocale) ? tp.to.AllGridPositions() : tp.from.AllGridPositions();
                    var capIndex = room.layout.traversalCapabilities.IndexOf(capabilities);
                    if (capIndex != room.expectedCapabilitiesIndex)
                    {
                        if (room.specialExpectedCapabilities == null)
                        {
                            room.specialExpectedCapabilities = new Dictionary<Int2D, int>();
                        }

                        foreach (var g in gridPos)
                        {
                            room.specialExpectedCapabilities.Add(g, capIndex);
                        }
                    }
                }
                else
                {
                    var suitableRequirements = GetRandomSuitableRequirements(capabilities, tp.limitations, room.environmentalEffect);
                    if (suitableRequirements != null) { room.traversalPathRequirements.Add(suitableRequirements); }
                    else
                    {
                        Debug.LogWarning("AddTraversalPathIfNeeded could not get suitable requirements for following limitations");
                        Debug.LogWarning(tp.limitations.ToString());
                        success = false;
                        break;
                    }
                }
            }
        }
        return success;
    }

    public void GetEnvironmentalEffect(RoomAbstract start, RoomAbstract end, TraversalCapabilities currentCapabilities, EnvironmentType environmentType, out EnvironmentalEffect environmentalEffect, out bool useAltTileset, out int altPalette)
    {
        environmentalEffect = currentCapabilities.lastGainedAffordance.requiredEnvironmentalResistance;
        var prohibittedEnvTypes = new HashSet<EnvironmentType> { EnvironmentType.Surface, EnvironmentType.ForestSlums, EnvironmentType.BeastGuts, EnvironmentType.Glitch };

        if (prohibittedEnvTypes.Contains(environmentType))
        {
            if (environmentalEffect != EnvironmentalEffect.None)
            {
                Debug.LogWarning("WOAH! PROHIBITTED AREA ASSIGNED ENVIRONMENTAL EFFECT!!!!");
                environmentalEffect = EnvironmentalEffect.None;
                if (start.assignedRoomInfo.environmentType == environmentType) start.environmentalEffect = EnvironmentalEffect.None;
                if (end.assignedRoomInfo.environmentType == environmentType) end.environmentalEffect = EnvironmentalEffect.None;
            }
        }
        else if (environmentalEffect == EnvironmentalEffect.None)
        { 
            if (end.environmentalEffect != EnvironmentalEffect.None)
            {
                environmentalEffect = end.environmentalEffect;
            }
            else if (currentCapabilities.environmentalResistance != 0 && //Can the current capabilities support any env effects?
                end.assignedRoomInfo.roomType != RoomType.Shop && //Robots wouldn't set of shops beyond paths that are underwater, in darkness, or in extreme heat
                currentCapabilities.effectiveJumpHeight < 16 && //Don't set Env Effects for high jump paths, water can cause trouble
                random.value > 0.66f) // 1 in 3 chance to add environmental effects to paths after the main one
            {
                environmentalEffect = PickRandom(currentCapabilities.environmentalResistance.GetFlags().ToList());

                //Assign environmental effect to end if it supports it
                if (end.assignedRoomInfo.roomType != RoomType.MegaBeast &&
                    end.assignedRoomInfo.roomType != RoomType.BossRoom)
                {
                    end.environmentalEffect = environmentalEffect;
                }
            }
        }

        useAltTileset = false;

        //use alt factory tileset (ruins) for areas with heat, darkness, confusion, water, etc.
        if(environmentType == EnvironmentType.Factory && environmentalEffect.RequiresTraversalAbility())
        {
            useAltTileset = true;
        }

        if (environmentalEffect == EnvironmentalEffect.None)
        {
            if (environmentType == EnvironmentType.Cave || environmentType == EnvironmentType.Factory && random.value > 0.5)
            {
                useAltTileset = true;

                //Always add funky lightning to ruined factory if it doesn't have another effect already
                if (environmentType == EnvironmentType.Factory && environmentalEffect == EnvironmentalEffect.None)
                {
                    environmentalEffect = random.value > 0.5f ? EnvironmentalEffect.Strobe : EnvironmentalEffect.Pulse;
                }
            }

            //Add fog to random Cave and Buried City paths
            if ((environmentType == EnvironmentType.Cave || environmentType == EnvironmentType.BuriedCity) &&
                (environmentalEffect == EnvironmentalEffect.None && random.value > 0.5f))
            {
                environmentalEffect = EnvironmentalEffect.Fog;
            }
        }

        altPalette = 0;
        if (environmentalEffect != EnvironmentalEffect.Heat)
        {
            switch (environmentType)
            {
                case EnvironmentType.ForestSlums:
                    altPalette = random.Range(0, 2);
                    break;
                case EnvironmentType.CoolantSewers:
                case EnvironmentType.Surface:
                    altPalette = random.Range(0, 3);
                    break;
                case EnvironmentType.Factory:
                case EnvironmentType.Cave:
                case EnvironmentType.BuriedCity:
                    altPalette = random.Range(0, 4);
                    break;
            }
        }

        //if this is a brand new start room (likely at the beginning of a hub), make it match the rest of the path.
        if (start.exits.Count == 0)
        {
            start.environmentalEffect = environmentalEffect;
            start.altPalette = altPalette;
            start.useAltTileset = useAltTileset;
        }

        end.altPalette = altPalette;
        end.useAltTileset = useAltTileset;
    }

    public RoomAbstract TryPlaceRoomByEntrance(RoomInfo roomInfo, Int2D entrancePosition, ExitLimitations entrance, RoomAbstract pathTarget = null, bool allowAnyConnections = false)
    {
        var newRoomPosition = entrancePosition;
        var bestEntranceGridPosition = new Int2D(newRoomPosition.x + entrance.localGridPosition.x, newRoomPosition.y - (roomInfo.size.y - 1 - entrance.localGridPosition.y));
        newRoomPosition += entrancePosition - bestEntranceGridPosition;

        if (_layout.ValidRoomPlacement(newRoomPosition,roomInfo.size))
        {
            var roomAbstract = new RoomAbstract(roomInfo, newRoomPosition);

            bool requiredExitBlocked = false;
            foreach (var exit in roomInfo.requiredExits)
            {
                if (exit == entrance) { continue; }

                var exitTarget = roomAbstract.GetExitTarget(exit);
                var room = _layout.GetRoomAtPositon(exitTarget);
                if (room != null && (pathTarget == null || room != pathTarget || !pathTarget.CanConnectToExit(exitTarget, exit.direction)))
                {
                    if (!allowAnyConnections || !room.CanConnectToExit(exitTarget, exit.direction))
                    {
                        requiredExitBlocked = true;
                        break;
                    }
                };
            }

            if (requiredExitBlocked) { return null; }

            return roomAbstract;
        }
        
        return null;        
    }

    public RoomAbstract TryPlaceRoomByEntrance(RoomInfo roomInfo, Int2D entrancePosition, ExitLimitations entrance, List<Int2D> permittedReqExitPositions, List<RoomAbstract> permittedRequiredExitRooms)
    {
        var newRoomPosition = entrancePosition;
        var bestEntranceGridPosition = new Int2D(newRoomPosition.x + entrance.localGridPosition.x, newRoomPosition.y - (roomInfo.size.y - 1 - entrance.localGridPosition.y));
        newRoomPosition += entrancePosition - bestEntranceGridPosition;

        if (_layout.ValidRoomPlacement(newRoomPosition, roomInfo.size))
        {
            var roomAbstract = new RoomAbstract(roomInfo, newRoomPosition);            

            foreach (var exit in roomInfo.requiredExits)
            {
                if (exit.MatchesPosAndDir(entrance)) { continue; }

                var exitTarget = roomAbstract.GetExitTarget(exit);

                if (!permittedReqExitPositions.Contains(exitTarget)) return null;

                var room = _layout.GetRoomAtPositon(exitTarget);

                if (room != null && !permittedRequiredExitRooms.Contains(room)) return null;
            }

            return roomAbstract;
        }

        return null;
    }

    /// <summary>
    /// Finds an entrance with the specified direction, in the specified room, furthest away from the towardsDirection.
    /// For instance, if this room is to be placed on a path going up off of a right exit, this method with find the bottom-most, left exit
    /// to serve as an entrance into the room.
    /// </summary>
    /// <param name="roomInfo">The room being explored for exits.</param>
    /// <param name="entranceDirection">The direction the returned exit need to face to connect to the previous room.</param>
    /// <param name="towardsDirection">The direction the path this room is to be added to, is heading.</param>
    /// <returns>An entrance with the specified direction, in the specified room, furthest away from the towardsDirection.</returns>
    public ExitLimitations FindBestEntrance(RoomInfo roomInfo, Direction entranceDirection, Vector2 towardsDirection)
    {
        ExitLimitations bestEntrance = null;

        for (int j = 0; j < 2; j++)
        {
            //check required exits first
            var listToCheck = j == 0 ? roomInfo.requiredExits : roomInfo.possibleExits;

            float bestValue = 0;

            foreach (var entrance in listToCheck)
            {
                if (entrance.direction != entranceDirection)
                {
                    continue;
                }

                if (entranceDirection.isHorizontal())
                {
                    if (bestEntrance == null)
                    {
                        bestEntrance = entrance;
                        bestValue = entrance.localGridPosition.y;
                        continue;
                    }

                    if (towardsDirection.y > 0) //looking for the lowest local y
                    {
                        if (entrance.localGridPosition.y < bestValue)
                        {
                            bestEntrance = entrance;
                            bestValue = entrance.localGridPosition.y;
                        }
                    }
                    else //looking for the highest local y
                    {
                        if (entrance.localGridPosition.y > bestValue)
                        {
                            bestEntrance = entrance;
                            bestValue = entrance.localGridPosition.y;
                        }
                    }
                }
                else
                {
                    if (bestEntrance == null)
                    {
                        bestEntrance = entrance;
                        bestValue = entrance.localGridPosition.x;
                        continue;
                    }

                    if (towardsDirection.x > 0)
                    {
                        if (entrance.localGridPosition.x < bestValue)
                        {
                            bestEntrance = entrance;
                            bestValue = entrance.localGridPosition.x;
                        }
                    }
                    else
                    {
                        if (entrance.localGridPosition.x > bestValue)
                        {
                            bestEntrance = entrance;
                            bestValue = entrance.localGridPosition.x;
                        }
                    }
                }
            }

            //if an exit was found among the required exits, it must be used
            if (bestEntrance != null)
            {
                break;
            }
        }

        return bestEntrance;
    }

    public void CropPathNodes (RoomAbstract room, ref List<Int2D> path)
    {
        //Trim path points in starting room;
        var minIndex = path.Count-1;
        var maxIndex = 0;
        bool needsCrop = false;
        for (int i = 0; i < path.Count; i++)
        {
            if (room.ContainsGridPosition(path[i]))
            {
                needsCrop = true;
                if (i > maxIndex) maxIndex = i;
                if (i < minIndex) minIndex = i;
            }
        }

        if (needsCrop)
        {
            path.RemoveRange(minIndex, (maxIndex - minIndex) + 1);
        }
    }

    public bool CropPathNodes(RoomAbstract room, ref List<Int2D> path, out Int2D min, out Int2D max)
    {
        //Trim path points in starting room;
        var minIndex = path.Count - 1;
        var maxIndex = 0;
        bool needsCrop = false;
        for (int i = 0; i < path.Count; i++)
        {
            if (room.ContainsGridPosition(path[i]))
            {
                needsCrop = true;
                if (i > maxIndex) maxIndex = i;
                if (i < minIndex) minIndex = i;
            }
        }

        if (needsCrop)
        {
            min = path[minIndex];
            max = path[maxIndex];
            path.RemoveRange(minIndex, (maxIndex - minIndex) + 1);
            return true;
        }
        else
        {
            min = Int2D.zero;
            max = Int2D.zero;
            return false;
        }
    }

    public bool GetCropPathNodesIndices(RoomAbstract room, ref List<Int2D> path, out int minIndex, out int maxIndex)
    {
        //Trim path points in starting room;
        minIndex = path.Count - 1;
        maxIndex = 0;
        bool needsCrop = false;
        for (int i = 0; i < path.Count; i++)
        {
            if (room.ContainsGridPosition(path[i]))
            {
                needsCrop = true;
                if (i > maxIndex) maxIndex = i;
                if (i < minIndex) minIndex = i;
            }
        }
        return needsCrop;
    }

    public List<Int2D> GetPath(RoomAbstract start, RoomAbstract end, Func<RoomAbstract, Int2D, bool> customValidate = null)
    {
        var pathFinder = new PathFinder();
        Func<RoomAbstract, Int2D, bool> validate = customValidate != null ? customValidate : (r, p) => PathValidate(r, p, start, end);

        Int2D s;
        if (!start.GetClosestGlobalPositionWithExit(end.gridPosition, out s, validate))
        {
            s = start.GetClosestGlobalPositionWithExit(end.gridCenter);
        }

        Int2D e;
        if(!end.GetClosestGlobalPositionWithExit(s, out e, validate))
        {
            e = end.GetClosestGlobalPositionWithExit(s);
        }

        var path = pathFinder.GetPath(_layout, s, e, validate);
        return path;
    }

    public RoomAbstract GetNewPathStart(RoomAbstract start, ref List<Int2D> path)
    {
        bool finishedCropping = false;
        int minIndex = 0;
        int maxIndex = 0;
        while (!finishedCropping)
        {
            finishedCropping = true;
            if (GetCropPathNodesIndices(start, ref path, out minIndex, out maxIndex))
            {
                path.RemoveRange(0, maxIndex + 1);
            }

            var connectedRooms = start.GetConnectedRooms();
            foreach (var point in path)
            {
                var room = _layout.GetRoomAtPositon(point);
                if (connectedRooms.Contains(room))
                {
                    finishedCropping = false;
                    start = room;
                    break;
                }
            }
        }

        return start;
    }

    public ExitLimitations FindPathExit(RoomAbstract start, ref List<Int2D> path, TraversalCapabilities capabilities, bool useLastAffordance, List<RoomAbstract> permittedRooms)
    {
        var target = path.First();

        //requiredExits;
        var needUseRequiredExit = start.assignedRoomInfo.requiredExits.Count == 2;

        var bestExits = start.GetUsableUnusedExits(permittedRooms, (e) =>
        {
            var exitTarget = start.GetExitTarget(e);
            if (exitTarget != target) return false;
            if (useLastAffordance && !e.toExit.CanSatisfyRequirements(capabilities.lastGainedAffordance)) return false;
            if (needUseRequiredExit && !start.assignedRoomInfo.requiredExits.Contains(e)) return false;
            return true;
        });

        var exit = bestExits.FirstOrDefault();
        if (exit != null)
        {
            var exitTarget = start.GetExitTarget(exit);
            var roomAtPosition = _layout.GetRoomAtPositon(exitTarget);

            if (roomAtPosition == null) return exit;
            else
            {
                var usableExits = roomAtPosition.GetUsableUnusedExits(start, (e) =>
                {
                    return e.direction == exit.direction.Opposite() && roomAtPosition.GetGridPosition(e.localGridPosition) == exitTarget;
                });

                var entrance = usableExits.FirstOrDefault();
                if (entrance != null)
                {
                    var exitRequirements = (useLastAffordance) ? capabilities.lastGainedAffordance : GetRandomSuitableRequirements(capabilities, exit.toExit, start.environmentalEffect);
                    if(exitRequirements == null)
                    {
                        Debug.LogError("Couldn't get valid requirements for exit to " + exitTarget + "!");
                        return null;
                    }

                    TraversalRequirements entranceRequirements;
                    if (roomAtPosition.assignedRoomInfo.roomType == RoomType.StartingRoom && roomAtPosition.assignedRoomInfo.environmentType == _layout.environmentOrder[1])
                    {
                        Debug.Log("Handling unique area1 starting room circumstances");
                        var finalCapabilities = _layout.traversalCapabilities[_layout.traversalCapabilities.Count - 1];
                        entranceRequirements = GetRandomSuitableRequirements(finalCapabilities, entrance.toExit, roomAtPosition.environmentalEffect);
                    }
                    else
                    {
                        entranceRequirements = GetRandomSuitableRequirements(capabilities, entrance.toExit, roomAtPosition.environmentalEffect);
                    }

                    if (entranceRequirements == null)
                    {
                        Debug.LogError("Couldn't get valid requirements for exit to " + start.GetGridPosition(exit.localGridPosition) + "!");
                        return null;
                    }

                    start.exits.Add(new ExitAbstract(exit, start, exitRequirements));
                    roomAtPosition.exits.Add(new ExitAbstract(entrance, roomAtPosition, entranceRequirements));

                    return exit;
                }
            }
        }

        return null;        
    }

    public ExitLimitations FindBestExit(RoomAbstract start, RoomAbstract end, Int2D entranceLocalGridPosition, TraversalCapabilities capabilities, bool useLastAffordance)
    {
        return FindBestExit(start, end, entranceLocalGridPosition, capabilities, useLastAffordance, new List<RoomAbstract> { end });
    }

    public ExitLimitations FindBestExit(RoomAbstract start, RoomAbstract end, Int2D entranceLocalGridPosition, TraversalCapabilities capabilities, bool useLastAffordance, List<RoomAbstract> permittedRooms = null)
    {
        ExitLimitations bestExit = null;
        ExitLimitations tiedExit = null;

        var shortestDistanceToEnd = float.MaxValue;
        var shortestAllDistances = float.MaxValue; //in the case of a tie, the exit with the shortest cumulative distance to all usable end room exits will be chosen
        var usableStartExits = start.GetUsableUnusedExits(permittedRooms); //it seems like required exits don't match these?
        var usableEndExits = end.GetUsableUnusedExits(start);
        var usableEndExitPositions = new HashSet<Int2D>();

        //get a list of all exit target positions for the usable exits in the end room.
        foreach (var exit in usableEndExits)
        {
            var position = end.GetExitTarget(exit);
            if (!usableEndExitPositions.Contains(position)) { usableEndExitPositions.Add(position); }
        }

        List<Int2D> validTraversalPathGridPositions = new List<Int2D>();
        if (useLastAffordance && !capabilities.lastGainedAffordance.JustEnvironmental())
        {
            for (int i = 0; i < start.assignedRoomInfo.traversalPaths.Count; i++)
            {
                var path = start.assignedRoomInfo.traversalPaths[i];
                var requirements = start.traversalPathRequirements.Count > i ? start.traversalPathRequirements[i] : null;
                //If the rooms requirements have already been set, compare them to capabilities, otherwise see if the path can satisfy them
                var canSatisfy = requirements == null ? path.limitations.CanSatisfyRequirements(capabilities.lastGainedAffordance) : requirements == capabilities.lastGainedAffordance;

                if (canSatisfy)
                {
                    if (!path.to.ContainsLocalGridPosition(entranceLocalGridPosition))
                    {
                        validTraversalPathGridPositions.AddRange(path.to.AllGridPositions());
                    }

                    if (path.reciprocal && !path.from.ContainsLocalGridPosition(entranceLocalGridPosition))
                    {
                        validTraversalPathGridPositions.AddRange(path.from.AllGridPositions());
                    }
                }
            }
        }

        //check the required exits in the starting room then the possible exits
        for (int j = 0; j < 2; j++)
        {
            var listToCheck = j == 0 ? start.assignedRoomInfo.requiredExits : start.assignedRoomInfo.possibleExits;
            foreach (var exit in listToCheck)
            {
                var validTraversalPathGridPosition = validTraversalPathGridPositions.Contains(exit.localGridPosition);

                if(validTraversalPathGridPositions.Count > 0 && !validTraversalPathGridPosition)
                {
                    continue; //don't explore exits that aren't valid if this a traversalPathRoom
                }

                if (useLastAffordance && !exit.toExit.CanSatisfyRequirements(capabilities.lastGainedAffordance) && !validTraversalPathGridPosition)
                {
                    if (listToCheck == start.assignedRoomInfo.requiredExits && usableStartExits.Contains(exit))
                    {
                        Debug.LogError("A room with a required exit that can't satisfy made it to FindBestExit");
                        return null;
                    }
                    continue;
                }

                if (start.minorItems.Any(i => i.spawnInfo.conflictingExits.Any(e => e.direction == exit.direction && e.position == exit.localGridPosition))) { continue; }

                var exitTarget = start.GetExitTarget(exit);
                var roomAtPosition = _layout.GetRoomAtPositon(exitTarget);

                if (roomAtPosition == null)
                {
                    var allDistances = 0f;
                    bool possibleTie = false;
                    bool newBestExit = false;
                    foreach (var position in usableEndExitPositions)
                    {
                        var distance = Int2D.Distance(position, exitTarget);
                        allDistances += distance;
                        if (distance < shortestDistanceToEnd)
                        {
                            shortestDistanceToEnd = distance;
                            bestExit = exit;
                            newBestExit = true;
                        }
                        else if (distance == shortestDistanceToEnd)
                        {
                            tiedExit = exit;
                            possibleTie = true;
                        }
                    }

                    if ((newBestExit || possibleTie) && allDistances < shortestAllDistances)
                    {
                        shortestAllDistances = allDistances;
                        if (possibleTie) { bestExit = tiedExit; }
                    }
                }
                else if (permittedRooms != null && permittedRooms.Contains(roomAtPosition))
                {
                    var usableExits = roomAtPosition.GetUsableUnusedExits(start);
                    var connection = usableExits.Find(e => e.direction == exit.direction.Opposite() && roomAtPosition.GetGridPosition(e.localGridPosition) == exitTarget);
                    if (connection != null)
                    {
                        bestExit = exit;
                        var requirements = (useLastAffordance && !validTraversalPathGridPosition) ? capabilities.lastGainedAffordance : GetRandomSuitableRequirements(capabilities, exit.toExit, start.environmentalEffect);
                        start.exits.Add(new ExitAbstract(exit, start, requirements));
                        TraversalRequirements endCapabilities;

                        //for final cap gate after first area in a standard run
                        if(roomAtPosition.assignedRoomInfo.roomType == RoomType.StartingRoom && roomAtPosition.assignedRoomInfo.environmentType == _layout.environmentOrder[1])
                        {
                            var finalCapabilities = _layout.traversalCapabilities[_layout.traversalCapabilities.Count - 1];
                            endCapabilities = GetRandomSuitableRequirements(finalCapabilities, connection.toExit, roomAtPosition.environmentalEffect);
                        }
                        else
                        {
                            endCapabilities = GetRandomSuitableRequirements(capabilities, connection.toExit, roomAtPosition.environmentalEffect);
                        }

                        roomAtPosition.exits.Add(new ExitAbstract(connection, roomAtPosition, endCapabilities));
                        break;
                    }
                }
            }

            if (bestExit != null)
            {
                break;
            }
        }

        return bestExit;
    }

    public TraversalRequirements GetRandomSuitableRequirements(TraversalCapabilities capabilites, TraversalLimitations limitations, EnvironmentalEffect environmentalEffect)
    {
        var requirements = new TraversalRequirements();

        if (environmentalEffect == EnvironmentalEffect.Underwater && limitations.requiredJumpHeight > capabilites.waterJumpHeight)
        {
            Debug.LogError("Traversal Capability cannot traverse! Require WATER jump height too high!");
            return null;
        }

        if (limitations.requiredJumpHeight > capabilites.effectiveJumpHeight)
        {
            Debug.LogError("Traversal Capability cannot traverse! Require jump height too high!");
            return null;
        }

        //if limitations.supportedDamageTypes == 0 they're probably from a traversal path
        if (limitations.supportedDamageTypes == 0)
        {
            requirements.requiredDamageType = 0;
        }
        else
        {
            var damageTypes = capabilites.damageTypes.GetFlags();
            var possibleDamageTypes = damageTypes.Where(d => limitations.supportedDamageTypes.HasFlag(d)).ToList();
            possibleDamageTypes.Remove(0);

            if (possibleDamageTypes.Count == 0)
            {
                Debug.LogError("Traversal Capabilities cannot traverse due to damage type!");
                Debug.LogError("limitations.supportedDamageTypes");
                Debug.LogError("================================");
                foreach (var d in limitations.supportedDamageTypes.GetFlags())
                {
                    Debug.LogError(d.ToString());
                }

                Debug.LogError("capabilites.damageTypes");
                Debug.LogError("================================");
                foreach (var d in damageTypes)
                {
                    Debug.LogError(d.ToString());
                }
                return null;
            }

            requirements.requiredDamageType = possibleDamageTypes[0];
        }

        if(environmentalEffect == EnvironmentalEffect.Underwater)
        {
            requirements.minWaterJumpHeight = limitations.requiredJumpHeight;
            requirements.maxWaterJumpHeight = limitations.requiredJumpHeight;
        }
        else
        {
            requirements.minWaterJumpHeight = 0;
            requirements.maxWaterJumpHeight = limitations.requiredJumpHeight;
        }

        requirements.minEffectiveJumpHeight = limitations.requiredJumpHeight;
        requirements.maxEffectiveJumpHeight = limitations.requiredJumpHeight;

        if (limitations.requiresGroundedSmallGaps)
        {
            if (!capabilites.canTraverseGroundedSmallGaps)
            {
                Debug.LogError("Traversal Capability cannot traverse! requiresGroundedSmallGaps");
                return null;
            }

            requirements.requiresGroundedSmallGaps = true;
        }
        else
        {
            requirements.requiresGroundedSmallGaps = limitations.supportsGroundedSmallGaps && capabilites.canTraverseGroundedSmallGaps ? (random.value > 0.5f ? true : false) : false;
        }

        requirements.supportsShotIgnoresTerrain = limitations.supportsShotIgnoresTerrain; //Does this make sense?

        if (limitations.requiresShotIgnoresTerrain)
        {
            if(!capabilites.shotIgnoresTerrain)
            {
                Debug.LogError("Traversal Capability cannot traverse! requiresShotIgnoresTerrain");
                return null;
            }

            requirements.requiresShotIgnoresTerrain = true;
        }
        else if(limitations.supportsShotIgnoresTerrain && capabilites.shotIgnoresTerrain)
        {
            requirements.requiresShotIgnoresTerrain = random.value > 0.5f ? true : false;
        }
        else
        {
            requirements.requiresShotIgnoresTerrain = false;
        }

        if (limitations.requiresPhaseThroughWalls)
        {
            if (!capabilites.canPhaseThroughWalls)
            {
                Debug.LogError("Traversal Capability cannot traverse! requiresPhaseThroughWalls");
                return null;
            }

            requirements.requiresPhaseThroughWalls = true;
        }
        else
        {
            requirements.requiresPhaseThroughWalls = limitations.supportsPhaseThroughWalls && capabilites.canPhaseThroughWalls ? (random.value > 0.5f ? true : false) : false;
        }

        return requirements;
    }
}