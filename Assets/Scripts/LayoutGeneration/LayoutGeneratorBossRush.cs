using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class LayoutGenerator
{
    public IEnumerable BossRush(int seed, RoomLayout layout, List<AchievementID> achievements)
    {
        _layout = layout;
        _layout.seed = seed;
        _layout.gameMode = GameMode.BossRush;
        _layout.password = SeedHelper.ParametersToKey(_layout.gameMode, _layout.seed, _layout.itemOrder, achievements);

        if (!_initialized) { Initialize(achievements); }
        random = new MicrosoftRandom(seed);

        var baseAbilities = new TraversalCapabilities() { damageTypes = DamageType.Generic, baseJumpHeight = Constants.startingMaxJumpHeight, jumps = 1 };

        bool layoutCompletedSuccessfully = false;

        while (!layoutCompletedSuccessfully)
        {
            _layout.Initialize(RoomLayout.standardWidth, RoomLayout.standardHeight, 0, 0);
            _layout.environmentLimits.Clear();
            _layout.traversalCapabilities = new List<TraversalCapabilities>() { baseAbilities };

            layoutCompletedSuccessfully = true;

            #region Environments
            var environments = new List<EnvironmentType>() { EnvironmentType.Surface };
            if (achievements == null || achievements.Contains(AchievementID.ForestSlums))
            {
                environments.Add(EnvironmentType.ForestSlums);
                if (random.value > 0.5f) { SwapEnvironments(environments, EnvironmentType.Surface, EnvironmentType.ForestSlums); }
            }

            environments.Add(EnvironmentType.Cave);
            if (achievements == null || achievements.Contains(AchievementID.CoolantSewers))
            {
                environments.Add(EnvironmentType.CoolantSewers);
                if (random.value > 0.5f) { SwapEnvironments(environments, EnvironmentType.Cave, EnvironmentType.CoolantSewers); }
            }

            environments.Add(EnvironmentType.Factory);
            if (achievements == null || achievements.Contains(AchievementID.CrystalMines))
            {
                environments.Add(EnvironmentType.CrystalMines);
                if (random.value > 0.5f) { SwapEnvironments(environments, EnvironmentType.Factory, EnvironmentType.CrystalMines); }
            }

            environments.Add(EnvironmentType.BuriedCity);
            #endregion

            //var items = Enum.GetValues(typeof(MajorItem)).Cast<MajorItem>().ToList().FindAll((i) => i != MajorItem.None && (ItemManager.items != null && ItemManager.items[i].bossRushPool));
            var availableBosses = GetBossList(achievements);

            #region Determine Bosses Per Region
            if (availableBosses.Count < 12) { throw new Exception("Only " + availableBosses.Count + " Bosses. Not enough bosses for Boss Rush!"); }

            //reusable
            List<RoomInfo> infos;
            EnvironmentType env;
            var bossRoomInfos = new Dictionary<EnvironmentType, List<RoomInfo>>();

            //possible Room Infos
            var environmentCopy = new List<EnvironmentType>(environments);
            var min = Mathf.Min(availableBosses.Count, 18);

            var totalRoomInfos = bossRoomInfos.Values.Sum(s => s.Count);
            while (totalRoomInfos < min)
            {
                env = totalRoomInfos == 0 ? EnvironmentType.BuriedCity : PickRandom(environmentCopy);
                infos = new List<RoomInfo>(roomInfos[env].bossRoomInfos);
                infos.RemoveAll((r) => !availableBosses.Contains(r.boss));
                if (infos.Count > 0)
                {
                    bossRoomInfos[env] = infos;
                    totalRoomInfos = bossRoomInfos.Values.Sum(s => s.Count);
                    environmentCopy.Remove(env);
                }
            }

            environments.RemoveAll(e => environmentCopy.Contains(e));

            //Mouth Meat in Cave or Factory?
            if (bossRoomInfos.ContainsKey(EnvironmentType.Factory) && bossRoomInfos.ContainsKey(EnvironmentType.Cave) &&
                bossRoomInfos[EnvironmentType.Factory].Any(r => r.boss == BossName.MouthMeatSenior) &&
                bossRoomInfos[EnvironmentType.Cave].Any(r => r.boss == BossName.MouthMeatSenior))
            {
                var fCount = bossRoomInfos[EnvironmentType.Factory].Count;
                var cCount = bossRoomInfos[EnvironmentType.Cave].Count;
                if (fCount > cCount || (fCount == cCount && random.value > 0.5f))
                {
                    bossRoomInfos[EnvironmentType.Factory].RemoveAll(r => r.boss == BossName.MouthMeatSenior);
                }
                else
                {
                    bossRoomInfos[EnvironmentType.Cave].RemoveAll(r => r.boss == BossName.MouthMeatSenior);
                }
            }

            //Flesh Adder in Factory or Mines?
            if (bossRoomInfos.ContainsKey(EnvironmentType.CrystalMines) && bossRoomInfos.ContainsKey(EnvironmentType.Factory)) //Flesh Adder had to be defeated for Crystal Mines to Be Unlocked
            {
                var fCount = bossRoomInfos[EnvironmentType.Factory].Count;
                var cCount = bossRoomInfos[EnvironmentType.CrystalMines].Count;
                if (fCount > cCount || (fCount == cCount && random.value > 0.5f))
                {
                    bossRoomInfos[EnvironmentType.Factory].RemoveAll(r => r.boss == BossName.FleshAdder);
                }
                else
                {
                    bossRoomInfos[EnvironmentType.CrystalMines].RemoveAll(r => r.boss == BossName.FleshAdder);
                }
            }

            //Wall Creep in Caves or Sewers?
            if (bossRoomInfos.ContainsKey(EnvironmentType.CoolantSewers) && bossRoomInfos.ContainsKey(EnvironmentType.Cave)) //WallCreep had to be defeated for CoolantSewers to Be Unlocked
            {
                var cCount = bossRoomInfos[EnvironmentType.Cave].Count;
                var sCount = bossRoomInfos[EnvironmentType.CoolantSewers].Count;
                if (cCount > sCount || (cCount == sCount && random.value > 0.5f))
                {
                    bossRoomInfos[EnvironmentType.Cave].RemoveAll(r => r.boss == BossName.WallCreep);
                }
                else
                {
                    bossRoomInfos[EnvironmentType.CoolantSewers].RemoveAll(r => r.boss == BossName.WallCreep);
                }
            }

            //Mouth Meat in Surface or Forest?
            if (bossRoomInfos.ContainsKey(EnvironmentType.ForestSlums) && bossRoomInfos.ContainsKey(EnvironmentType.Surface)) //MouthMeat had to be defeated for ForestSlums to Be Unlocked
            {
                var sCount = bossRoomInfos[EnvironmentType.Surface].Count;
                var fCount = bossRoomInfos[EnvironmentType.ForestSlums].Count;
                if (sCount > fCount || (sCount == fCount && random.value > 0.5f))
                {
                    bossRoomInfos[EnvironmentType.Surface].RemoveAll(r => r.boss == BossName.MouthMeat);
                }
                else
                {
                    bossRoomInfos[EnvironmentType.ForestSlums].RemoveAll(r => r.boss == BossName.MouthMeat);
                }
            }

            //only want 12 of these rooms
            totalRoomInfos = bossRoomInfos.Values.Sum(s => s.Count);
            while (totalRoomInfos > 12) //start removing possible rooms from the most populous environments
            {
                int mostRooms = 0;
                EnvironmentType crowdedEnv = EnvironmentType.Surface;
                foreach (var kvp in bossRoomInfos)
                {
                    var count = kvp.Value.Count;
                    if (count > mostRooms)
                    {
                        mostRooms = count;
                        crowdedEnv = kvp.Key;
                    }
                }

                var pick = PickRandom(bossRoomInfos[crowdedEnv]);
                bossRoomInfos[crowdedEnv].Remove(pick);
                totalRoomInfos--;
            }
            #endregion

            environments.ForEach(e => _layout.environmentLimits.Add(e, new Rect(0, 0, _layout.width, _layout.height)));
            _layout.environmentOrder = _layout.environmentLimits.Keys.ToArray();

            RoomAbstract lastRoom = null;

            var allDirections = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();

            #region Place Starting Room
            infos = roomInfos[environments[0]].otherSpecialRoomsInfos.Where((r) => r.sceneName.Contains("BossRushStart")).ToList();
            lastRoom = new RoomAbstract(PickRandom(infos), new Int2D(_layout.width / 2, _layout.height - _layout.height / 3));
            lastRoom.isStartingRoom = true;
            lastRoom.shopOfferings = new List<MajorItem>();
            foreach (var kvp in ItemManager.items)
            {
                var item = kvp.Value;
                if (!item.bossRushPool) continue;
                if (item.orbSmithPool || item.artificerPool || item.gunSmithPool || item.isTraversalItem)
                {
                    lastRoom.shopOfferings.Add(item.type);
                }
            }
            lastRoom.shopOfferings.Shuffle(random);
            layout.Add(lastRoom, random.ZeroToMaxInt());
            #endregion
            
            var shrines = Enum.GetValues(typeof(ShrineType)).Cast<ShrineType>().ToList();
            shrines.Remove(ShrineType.None);

            //3 bosses for each area alternate shrines and teleporters
            int spec = 0;
            for (int e = 0; e < environments.Count; e++)
            {
                env = environments[e];
                infos = bossRoomInfos[env];

                Direction direction;

                #region Place Main Branch
                var minExits = 8;
                var minArea = infos.Count > 2 ? 5 : 4;
                var branchRooms = roomInfos[env].generalRoomInfos.Where((r) => r.size.x * r.size.y >= minArea && r.possibleExits.Count >= minExits && r.SuitableBranchingRoom(baseAbilities)).ToList();
                var directions = new List<Direction> { Direction.Down };
                if (random.value > 0.5f)
                {
                    directions.Add(Direction.Left);
                    directions.Add(Direction.Right);
                }
                else
                {
                    directions.Add(Direction.Left);
                    directions.Add(Direction.Right);
                }
                directions.Add(Direction.Up);

                RoomAbstract branchRoom = null;
                while(branchRoom == null && directions.Count > 0)
                {
                    direction = directions.First();
                    directions.Remove(direction);
                    branchRoom = TryPlaceOffExistingRoom(lastRoom, Direction.Down.ToVector2(), branchRooms, baseAbilities, baseAbilities);
                }

                if (branchRoom == null)
                {
                    Debug.LogWarning("Boss Rush couldn't place Branch Room!");
                    layoutCompletedSuccessfully = false;
                    break;
                }

                layout.Add(branchRoom, random.ZeroToMaxInt());
                lastRoom = branchRoom;
                #endregion

                RoomAbstract room = null;
                if (stepByStep) yield return null;

                #region Final Boss
                if (env == EnvironmentType.BuriedCity)
                {
                    var finalInfos = roomInfos[env].otherSpecialRoomsInfos.Where(r => r.sceneName.Contains("BeastRemnant")).ToList();
                    directions = new List<Direction> { Direction.Up, Direction.Left, Direction.Right };
                    directions.Shuffle(random);
                    room = null;

                    while (room == null && directions.Count > 0)
                    {
                        direction = directions.First();
                        directions.Remove(direction);
                        room = TryPlaceOffExistingRoom(branchRoom, direction.ToVector2(), finalInfos, baseAbilities, baseAbilities);
                    }

                    if (room == null)
                    {
                        Debug.LogWarning("Boss Rush couldn't place Final Boss Room!");
                        layoutCompletedSuccessfully = false;
                        break;
                    }

                    _layout.Add(room, random.ZeroToMaxInt());
                    if (stepByStep) yield return null;
                }

                if (!layoutCompletedSuccessfully) { break; }
                #endregion

                #region Place Boss Rooms
                foreach (var roomInfo in infos)
                {
                    directions = new List<Direction> { Direction.Up, Direction.Left, Direction.Right };
                    directions.Shuffle(random);
                    directions.Add(Direction.Down);
                    room = null;

                    while (room == null && directions.Count > 0)
                    {
                        direction = directions.First();
                        directions.Remove(direction);
                        room = TryPlaceOffExistingRoom(branchRoom, direction.ToVector2(), new List<RoomInfo> { roomInfo }, baseAbilities, baseAbilities);
                    }

                    if (room == null)
                    {
                        Debug.LogWarning("Boss Rush couldn't place Boss Room!");
                        layoutCompletedSuccessfully = false;
                        break;
                    }

                    _layout.bossesAdded.Add(roomInfo.boss);
                    _layout.Add(room, random.ZeroToMaxInt());

                    if (stepByStep) yield return null;
                }

                if(!layoutCompletedSuccessfully) { break; }
                #endregion

                #region Shrine or Teleporter
                if (spec % 2 == 1)
                {
                    infos = roomInfos[env].teleporterRoomInfos;
                }
                else
                {
                    infos = roomInfos[env].shrineRoomsInfos.Where(r => shrines.Contains(r.shrineType)).ToList();
                }

                //TODO: make special rooms branch off of Wall Creep and Blightbark if they exist

                directions = new List<Direction> { Direction.Up, Direction.Left, Direction.Right };
                directions.Shuffle(random);
                directions.Add(Direction.Down);
                room = null;

                while (room == null && directions.Count > 0)
                {
                    direction = directions.First();
                    directions.Remove(direction);
                    room = TryPlaceOffExistingRoom(lastRoom, direction.ToVector2(), infos, baseAbilities, baseAbilities);
                }

                if(room == null)
                {
                    Debug.LogWarning("Boss Rush couldn't place Special Room!");
                    layoutCompletedSuccessfully = false;
                    break;
                }

                layout.Add(room, random.ZeroToMaxInt());
                if (room.assignedRoomInfo.roomType == RoomType.Shrine) { shrines.Remove(room.assignedRoomInfo.shrineType); }

                spec++;
                #endregion

                if (!layoutCompletedSuccessfully) break;

                layoutCompletedSuccessfully = FinishLayout();
            }
        }
    }

    public void SwapEnvironments(List<EnvironmentType> environments, EnvironmentType env1, EnvironmentType env2)
    {
        for (int i = 0; i < environments.Count; i++)
        {
            if (environments[i] == env1) { environments[i] = env2; }
            else if (environments[i] == env2) { environments[i] = env1; }
        }
    }

    public List<BossName> GetBossList(List<AchievementID> achievments)
    {
        if(achievments == null)
        {
            var bossList = Enum.GetValues(typeof(BossName)).Cast<BossName>().ToList();
            bossList.Remove(BossName.None);
            bossList.Remove(BossName.GlitchBoss);
            return bossList;
        }

        var bosses = new List<BossName>();
        foreach (var a in achievments)
        {
            if (a == AchievementID.None) continue;
            var aInfo = AchievementManager.achievements[a];
            if (aInfo.associatedBoss != BossName.None)
            {
                bosses.Add(aInfo.associatedBoss);
            }                       
        }

        return bosses;
    }
}

