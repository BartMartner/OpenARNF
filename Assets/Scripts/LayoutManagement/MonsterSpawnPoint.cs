using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawnPoint : RoomBasedRandom
{
    [Range(0f, 1f)]
    public float spawnNothingChance;
    public EnemySpawnInfo[] spawns;

    private int _currentIndex = 0;
    private float[] _randomPool;

    protected override IEnumerator Start()
    {
        if (_room)
        {
            while (_room.random == null) { yield return null; }
            _random = _room.random;
            while (LayoutManager.instance && _room.roomAbstract == null) { yield return null; }
        }

        var game = SaveGameManager.activeGame;
        if (game != null && game.lockDownRoomsCleared.Contains(_room.roomAbstract.roomID))
        {
            Destroy(gameObject);
            yield break;
        }

        Randomize();
    }

    public override void Randomize()
    {
        PopulateRandomPool();
        SpawnMonster();
    }

    protected void PopulateRandomPool()
    {
        if (_random == null)
        {
            Debug.LogWarning("_random was null in " + name);
            _random = new XorShift(0);
        }

        _randomPool = new float[2];
        for (int i = 0; i < 2; i++) { _randomPool[i] = _random.Value(); }
    }

    protected float GetRandomValue()
    {
        var result = _randomPool[_currentIndex];
        _currentIndex = _currentIndex + 1 % _randomPool.Length;
        return result;
    }

    protected virtual Vector3 GetPosition(EnemySpawnInfo spawnInfo)
    {
        return transform.position + spawnInfo.offset;
    }

    protected virtual Quaternion GetRotation(EnemySpawnInfo spawnInfo)
    {
        return spawnInfo.matchRotation ? transform.rotation : spawnInfo.prefab.transform.rotation;
    }

    public void SpawnMonster()
    {
        //Spawn nothing always uses room random!
        if (spawnNothingChance > 0 && GetRandomValue() < spawnNothingChance)
        {
            Destroy(gameObject);
            return;
        }

        EnemySpawnInfo spawnInfo = null;
        
        EnemySpawnInfo[] usableSpawns = null;
        if (SaveGameManager.activeSlot != null)
        {
            usableSpawns = spawns.Where(s => SaveGameManager.activeSlot.achievements.ContainsAll(s.requiredAchievements) &&
                !s.prohibittedGameModes.HasFlag(SaveGameManager.activeGame.gameMode)).ToArray();
        }
        else
        {
            usableSpawns = spawns;
        }

        float sumProbabilityFactor = Mathf.Max(usableSpawns.Sum(s => s.weight), float.Epsilon);
        float randPercent = GetRandomValue();

        for (int i = 0; i < usableSpawns.Length; i++)
        {
            float probability = usableSpawns[i].weight / sumProbabilityFactor;
            if (randPercent <= probability)
            {
                spawnInfo = usableSpawns[i];
                break;
            }
            randPercent -= probability;
        }

        if (spawnInfo != null)
        {
            var position = GetPosition(spawnInfo);
            var rotation = GetRotation(spawnInfo);

            Enemy enemy = null;

            ChampionSpawnInfo[] usableChampions = null;
            var championChance = 0.085f;

            var game = SaveGameManager.activeGame;
            var slot = SaveGameManager.activeSlot;
            var roomID = LayoutManager.instance && LayoutManager.instance.currentRoom ? LayoutManager.instance.currentRoom.roomAbstract.roomID : string.Empty;

            foreach (var p in PlayerManager.instance.players)
            {
                if (p.itemsPossessed.Contains(MajorItem.TyrsHorns)) { championChance += 0.15f; }
            }

            if (slot != null)
            {
                usableChampions = spawnInfo.championVariants.Where(s => slot.achievements.ContainsAll(s.stats.requiredAchievements) &&
                    !s.stats.prohibittedGameModes.HasFlag(slot.activeGameData.gameMode)).ToArray();

                if (usableChampions.Length > 0)
                {
                    if (slot.achievements.Contains(AchievementID.TheFleshening)) { championChance += 0.025f; }
                    if (slot.achievements.Contains(AchievementID.TheFlesheningII)) { championChance += 0.05f; }

                    if (game != null)
                    {
                        if (game.gameMode == GameMode.Exterminator) { championChance += 0.25f; }
                        if (game.gameMode == GameMode.Spooky) { championChance += 0.05f; }
                        if (game.roomsChampionsSpawned.Contains(roomID)) { championChance *= 0.40f; }
                    }
                }
            }
            else
            {
                usableChampions = spawnInfo.championVariants;
            }

            var champChanceRoll = Random.value;
            if (usableChampions.Length > 0 && champChanceRoll < championChance)
            {
                ChampionSpawnInfo champion = null;

                float sum = Mathf.Max(usableChampions.Sum((s) => s.weight), float.Epsilon);
                float roll = Random.value;
                for (int i = 0; i < usableChampions.Length; i++)
                {
                    float probability = usableChampions[i].weight / sum;
                    if (roll <= probability)
                    {
                        champion = usableChampions[i];
                        break;
                    }
                    roll -= probability;
                }

                if (champion != null && champion.prefab)
                {
                    enemy = Instantiate(champion.prefab, position, rotation, transform.parent);

                    var dropPickups = enemy.GetComponent<SpawnPickUpsOnDeath>();
                    if (dropPickups)
                    {
                        dropPickups.spawnChance = 1;
                        dropPickups.minDrops += champion.stats.dropBonus;
                        dropPickups.maxDrops += champion.stats.dropBonus;
                        dropPickups.scrapChance = champion.stats.scrapChanceBonus;
                        dropPickups.buffs = new DropType[] { DropType.AttackBuff, DropType.DamageBuff, DropType.SpeedBuff };
                    }

                    if (Random.value < champion.stats.dropMinorItemChance && champion.stats.minorItemDrops.Length > 0)
                    {
                        if (dropPickups) Destroy(dropPickups);
                        enemy.onEndDeath.AddListener(() =>
                        {
                            if (!enemy.selfDestruct)
                            {
                                var type = champion.stats.minorItemDrops[Random.Range(0, champion.stats.minorItemDrops.Length)];
                                var prefab = ResourcePrefabManager.instance.LoadGameObject("PickUps/" + type.ToString()).GetComponent<MinorItemPickUp>();
                                var minorItem = Instantiate(prefab, enemy.transform.position, Quaternion.identity, enemy.transform.parent);
                                minorItem.data.globalID = -99;
                            }
                        });
                    }

                    if (game == null || game.allowAchievements)
                    {
                        enemy.onEndDeath.AddListener(() =>
                        {
                            if (slot != null && slot.championsKilled < ushort.MaxValue)
                            {
                                slot.championsKilled++;
                                if (AchievementManager.instance && slot.championsKilled >= 20)
                                {
                                    AchievementManager.instance.TryEarnAchievement(AchievementID.TyrsHorns);
                                }
                            }
                        });
                    }

                    if (game != null && !game.roomsChampionsSpawned.Contains(roomID))
                    {
                        game.roomsChampionsSpawned.Add(roomID);
                        SaveGameManager.instance.Save();
                    }
                }
            }
            else
            {
                enemy = Instantiate<Enemy>(spawnInfo.prefab, position, rotation, transform.parent);
            }

            if (enemy)
            {
                if (spawnInfo.startingDirection != Vector3.zero)
                {
                    var ponger = enemy.GetComponent<PongerBehavior>();
                    if (ponger)
                    {
                        ponger.startingDirection = spawnInfo.startingDirection;
                        ponger.randomStartingDirection = false;
                    }
                }

                if(spawnInfo.overridePrewarm)
                {
                    var eventTimer = enemy.GetComponentInChildren<EventTimer>();
                    if (eventTimer) { eventTimer.preWarm = spawnInfo.preWarm; }
                }
            }
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    public virtual void OnDrawGizmos()
    {
        if (spawns != null && spawns.Length > 0)
        {
            var index = (int)(UnityEditor.EditorApplication.timeSinceStartup % spawns.Length);
            if (spawns[index].prefab)
            {
                var spriteRenderer = spawns[index].prefab.GetComponentInChildren<SpriteRenderer>();
                var sr = GetComponentInChildren<SpriteRenderer>();
                if (sr && spriteRenderer)
                {
                    if (sr.gameObject != gameObject) { sr.transform.localPosition = spawns[index].offset; }
                    sr.sprite = spriteRenderer.sprite;
                }
            }
        }
    }
#endif
}

[Serializable]
public class EnemySpawnInfo
{
    public Enemy prefab;
    [Range(0,1)]
    public float weight = 1f;
    public AchievementID[] requiredAchievements;
    public bool matchRotation = true;
    public Vector3 startingDirection;
    public Vector3 offset;
    public ChampionSpawnInfo[] championVariants;
    public GameMode prohibittedGameModes;
    public bool overridePrewarm;
    [Range(0, 1)]
    public float preWarm;
}

[Serializable]
public class ChampionSpawnInfo
{
    public Enemy prefab;
    [Range(0, 1)]
    public float weight = 1f;
    public ChampionStats stats;
}
