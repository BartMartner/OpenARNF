using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveGameData
{
    public bool started;
    public int seed;
    public string password;
    [JsonProperty(PropertyName = "time")]
    public float playTime;
    [JsonIgnore]
    public RoomLayout layout;
    [JsonProperty(PropertyName = "strt")]
    public Vector3 playerStart;
    [JsonProperty(PropertyName = "lstRm")]
    public Int2D lastRoom;
    [JsonProperty(PropertyName = "itCol")]
    public List<MajorItem> itemsCollected = new List<MajorItem>();
    [JsonProperty(PropertyName = "itPos")]
    public List<MajorItem> itemsPossessed = new List<MajorItem>();
    [JsonProperty(PropertyName = "itApl")]
    public HashSet<MajorItem> itemsApplied = new HashSet<MajorItem>();
    [JsonProperty(PropertyName = "minTCol")]
    public List<MinorItemType> minorItemTypesCollected = new List<MinorItemType>();
    [JsonProperty(PropertyName = "curItem")]
    public MajorItem currentActivatedItem;
    [JsonProperty(PropertyName = "hCol")]
    public int healthUpsCollected;
    [JsonProperty(PropertyName = "eCol")]
    public int energyUpsCollected;
    [JsonProperty(PropertyName = "dCol")]
    public int damageUpsCollected;
    [JsonProperty(PropertyName = "aCol")]
    public int attackUpsCollected;
    [JsonProperty(PropertyName = "sCol")]
    public int speedUpsCollected;
    [JsonProperty(PropertyName = "ssCol")]
    public int shotSpeedUpsCollected;
    [JsonProperty(PropertyName = "bots")]
    public int nanobots;
    [JsonProperty(PropertyName = "hlth")]
    public float playerHealth = Constants.startingHealth;
    [JsonProperty(PropertyName = "nrg")]
    public float playerEnergy = Constants.startingEnergy;
    [JsonProperty(PropertyName = "minCol")]
    public List<int> minorItemIdsCollected = new List<int>();
    [JsonProperty(PropertyName = "doors")]
    public List<int> doorsOpened = new List<int>();
    [JsonProperty(PropertyName = "visit")]
    public HashSet<string> roomsVisited = new HashSet<string>();
    [JsonProperty(PropertyName = "disc")]
    public HashSet<string> roomsDiscovered = new HashSet<string>();
    [JsonProperty(PropertyName = "rChmp")]
    public HashSet<string> roomsChampionsSpawned = new HashSet<string>();
    [JsonProperty(PropertyName = "shpEmpt")]
    public List<string> shopsEmptied = new List<string>();
    [JsonProperty(PropertyName = "bossD")]
    public List<BossName> bossesDefeated = new List<BossName>();
    [JsonProperty(PropertyName = "ldrooms")]
    public List<string> lockDownRoomsCleared = new List<string>();
    [JsonProperty(PropertyName = "actSvPos")]
    public List<Int2D> activeSaveRoomPositions = new List<Int2D>();
    [JsonProperty(PropertyName = "desSvPos")]
    public List<Int2D> destroyedSaveRoomPositions = new List<Int2D>();
    [JsonProperty(PropertyName = "loose")]
    public Dictionary<string, List<LooseItemData>> looseItems = new Dictionary<string, List<LooseItemData>>();
    [JsonProperty(PropertyName = "mapSegs")]
    public Dictionary<string, AutomapSegmentState> automapSegmentStates = new Dictionary<string, AutomapSegmentState>();
    [JsonProperty(PropertyName = "scrp")]
    public int grayScrap;
    [JsonProperty(PropertyName = "rScrp")]
    public int redScrap;
    [JsonProperty(PropertyName = "gScrp")]
    public int greenScrap;
    [JsonProperty(PropertyName = "bScrp")]
    public int blueScrap;
    [JsonProperty(PropertyName = "tutCom")]
    public bool tutorialComplete;
    [JsonProperty(PropertyName = "plyrDed")]
    public bool playerDead;
    [JsonProperty(PropertyName = "rky")]
    public bool redKeyLit;
    [JsonProperty(PropertyName = "gky")]
    public bool greenKeyLit;
    [JsonProperty(PropertyName = "bky")]
    public bool blueKeyLit;
    [JsonProperty(PropertyName = "kky")]
    public bool blackKeyLit;
    
    [JsonProperty(PropertyName = "cmplt")]
    public bool runCompleted;

    //1.1.1.14
    [JsonProperty(PropertyName = "shrnUsd")]
    public Dictionary<ShrineType, int> shrinesUsed = new Dictionary<ShrineType, int>();

    //1.1.2.18
    [JsonProperty(PropertyName = "ints")]
    public Dictionary<string, int> otherInts = new Dictionary<string, int>();

    //1.1.2.20
    [JsonProperty(PropertyName = "prmsts")]
    public Dictionary<int, int> permanentStateObjects = new Dictionary<int, int>();

    [JsonProperty(PropertyName = "gmd")]
    public GameMode gameMode;

    //1.2.0.23
    [JsonProperty(PropertyName = "dscTel")]
    public List<Int2D> discoveredTeleporters = new List<Int2D>();

    [JsonProperty(PropertyName = "trvgt")]
    public List<float> traversalItemCollectTimes = new List<float>();

    //1.6.0.32
    [JsonProperty(PropertyName = "rc")]
    public bool raceMode;
    [JsonProperty(PropertyName = "p2e")]
    public bool player2Entered;
    [JsonProperty(PropertyName = "p3e")]
    public bool player3Entered;
    [JsonProperty(PropertyName = "p4e")]
    public bool player4Entered;

    [JsonIgnore]
    public bool allowAchievements
    {
        get
        {
            return string.IsNullOrEmpty(password) && _permitAchievements.HasFlag(gameMode);
        }
    }

    [JsonIgnore]
    private const GameMode _permitAchievements = GameMode.Normal | GameMode.MegaMap | GameMode.Exterminator | GameMode.BossRush;

    [JsonIgnore]
    public float corruption
    {
        get
        {
            try //there are strange instance where this code can get called not on the main thread
            {
                var fromRooms = Mathf.Clamp(roomsVisited.Count / 10f, 0, 1); //corruption hits 1 after 10 rooms
                var iCount = itemsCollected.Count((i) => ItemManager.items[i].isTraversalItem) - 1;

                float fromTraversalItems;
                if (gameMode != GameMode.MegaMap)
                {
                    fromTraversalItems = Mathf.Clamp(iCount, 0, 6); //every traversal item after the first increases corruption by 1
                }
                else
                {
                    if (iCount < 0) { iCount = 0; }
                    var totalItems = (layout != null && layout.itemOrder != null) ? layout.itemOrder.Count - 1 : 6;
                    var t = iCount / totalItems;
                    fromTraversalItems = Mathf.Lerp(0, 6, t); //for mega map
                }
                //max corruption is 7
                return fromRooms + fromTraversalItems;
            }
            catch
            {
                return 0;
            }
        }
    }

    [JsonIgnore]
    public float collectRate
    {
        get
        {
            try //there are strange instance where this code can get called not on the main thread
            {
                if (layout != null)
                {
                    if (gameMode == GameMode.BossRush || gameMode == GameMode.ClassicBossRush)
                    {
                        return 0;
                    }

                    float totalItems = layout.traversalItemCount + layout.minorItemCount + layout.bonusItemsAdded.Count;
                    var majorItemsCollected = itemsCollected.FindAll((i) => layout.itemOrder.Contains(i) || layout.bonusItemsAdded.Contains(i)).Count;
                    float totalItemsCollected = minorItemIdsCollected.Count + majorItemsCollected;
                    return totalItemsCollected / totalItems;
                }
                else
                {
                    return 0f;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
