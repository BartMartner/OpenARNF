
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using cPlayer = Rewired.Player;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

public class DeathmatchManager : MonoBehaviour
{
    public static readonly List<string> allMaps = new List<string>()
    {
        "Deathmatch00", "Deathmatch01", "Deathmatch02", "Deathmatch03", "Deathmatch04", "Deathmatch05", "Deathmatch06", "Deathmatch07",
    };

    public static List<string> ValidMaps(List<string> mapRotation, List<AchievementID> achievements)
    {
        var valid = new List<string>(mapRotation);
        valid.RemoveAll(m => !allMaps.Contains(m));
        if (achievements != null)
        {            
            if (!achievements.Contains(AchievementID.BeastGuts)) { valid.Remove("Deathmatch04"); }
            if (!achievements.Contains(AchievementID.ForestSlums)) { valid.Remove("Deathmatch05"); }
            if (!achievements.Contains(AchievementID.CoolantSewers)) { valid.Remove("Deathmatch06"); }
            if (!achievements.Contains(AchievementID.CrystalMines)) { valid.Remove("Deathmatch07"); }
        }
        return valid;
    }

    public const int maxMolemanSpawnRate = 10;
    public const int maxMolemen = 30;

    public static DeathmatchManager instance;
    public DeathmatchScoreboard scoreboard;
    public DeathmatchPlayer playerPrefab;
    public HashSet<int> idsJoined = new HashSet<int>();
    public List<DeathmatchPlayer> players = new List<DeathmatchPlayer>();
    public GameObject[] screens;
    public MainCamera[] cameras;
    public DeathMatchUI[] ui;
    public GameObject[] spawnPoints;
    public MolemanDMSpawn[] molemanSpawnPoints;
    public Material[] materials;
    public IntEvent onJoin = new IntEvent();
    public IntEvent onDrop = new IntEvent();
    public bool allowPause = true;

    public Dictionary<Team, int> totalScore = new Dictionary<Team, int>()
    {
        {Team.DeathMatch0, 0 },
        {Team.DeathMatch1, 0 },
        {Team.DeathMatch2, 0 },
        {Team.DeathMatch3, 0 },
    };

    public Dictionary<Team, int> score = new Dictionary<Team, int>()
    {
        {Team.DeathMatch0, 0 },
        {Team.DeathMatch1, 0 },
        {Team.DeathMatch2, 0 },
        {Team.DeathMatch3, 0 },
    };

    public Dictionary<Team, int> deaths = new Dictionary<Team, int>()
    {
        {Team.DeathMatch0, 0 },
        {Team.DeathMatch1, 0 },
        {Team.DeathMatch2, 0 },
        {Team.DeathMatch3, 0 },
    };

    private Team _lastLeader;
    public Team leader { get { return _lastLeader; } }
    private int _lastHighScore;

    private List<cPlayer> _controllers = new List<cPlayer>();
    private int _playerCount;
    private int _lastSpawnPointUsed = 0;
    public HashSet<MajorItem> itemPool = new HashSet<MajorItem>();
    public HashSet<MajorItem> spawnRoomItems = new HashSet<MajorItem>();
    private Scene currentScene;
    private int currentArenaIndex;
    private float _time;
    public float time { get { return _time; } }    
    private bool _oneMinuteRemaining;
    private int _countDown = 10;
    private bool _ready;
    private bool _playersActive;
    private bool _tie;
    public bool tie { get { return _tie; } }

    [Header("Audio")]
    public AudioClip tieAudio;
    public List<AudioClip> winsAudio;
    public List<AudioClip> takesTheLeadAudio;
    public List<AudioClip> countdown;
    public AudioClip oneMinuteWarningAudio;

    public List<MajorItem> itemsOnTheField = new List<MajorItem>();

    [Header("Molemen")]
    public AdvancedAI[] molemenPrefabs;
    private float _molemanCounter;
    private Room _currentArena;
    private HashSet<Transform> _molemen = new HashSet<Transform>();

    private DeathmatchSettings _defaultSettings = new DeathmatchSettings()
    {
        fragLimit = 10,
        itemMode = DeathmatchItemMode.EnergyOnly,
        mode = DeathmatchMode.TimeLimit,
        rouletteItems = true,
        spawnRoomItems = true,
        timeLimit = 600,
        mapRotation = new List<string>(allMaps),
        molemanSpawnRate = 0,
        maxMolemen = 10,
    };

    public DeathmatchSettings settings
    {
        get
        {
            if(SaveGameManager.deathmatchSettings == null)
            {
                return _defaultSettings;   
            }
            else
            {
                return SaveGameManager.deathmatchSettings;
            }
        }
    }

    public void Awake()
    {
        _ready = false;

        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        for (int i = 0; i < 4; i++)
        {
            _controllers.Add(ReInput.players.GetPlayer(i));
        }

        totalScore = new Dictionary<Team, int>()
        {
            {Team.DeathMatch0, 0 },
            {Team.DeathMatch1, 0 },
            {Team.DeathMatch2, 0 },
            {Team.DeathMatch3, 0 },
        };

        var needLoadArena = true;

        //Preload some item spritesheets
        var items = new string[] { "Flamethrower", "RocketLauncher", "BuzzsawGun", "RailGun", "MachineGun", "ScatterGun", };
        for (int i = 0; i < items.Length; i++)
        {
            ResourcePrefabManager.instance.LoadTexture2D("PlayerParts/" + items[i] + "BackArm");
            ResourcePrefabManager.instance.LoadTexture2D("PlayerParts/" + items[i] + "FrontArm");
        }
        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if(scene.name != "DeathmatchCore")
            {
                currentScene = SceneManager.GetSceneByName(scene.name);
                _currentArena = FindObjectOfType<Room>();
                needLoadArena = false;
                break;
            }
        }

        if (needLoadArena)
        {
            StartCoroutine(LoadNewArena());
        }
        else
        {
            ResetManager();
            HandleSpawnRoomItems();
            _ready = true;
        }
    }

    public void Start()
    {
        CheckForSpawnPoints();
    }

    public void ResetManager()
    {
        _lastHighScore = 0;
        _lastLeader = Team.DeathMatch0;
        _time = settings.timeLimit;
        _oneMinuteRemaining = false;
        _countDown = 10;
        _playersActive = false;

        itemPool.Clear();
        itemsOnTheField.Clear();
        spawnRoomItems.Clear();

        score = new Dictionary<Team, int>()
        {
            {Team.DeathMatch0, 0 },
            {Team.DeathMatch1, 0 },
            {Team.DeathMatch2, 0 },
            {Team.DeathMatch3, 0 },
        };
        _tie = true;

        deaths = new Dictionary<Team, int>()
        {
            {Team.DeathMatch0, 0 },
            {Team.DeathMatch1, 0 },
            {Team.DeathMatch2, 0 },
            {Team.DeathMatch3, 0 },
        };

        foreach (var item in ItemManager.items.Values.Where((i) => i.deathmatch))
        {
            if (settings.itemMode == DeathmatchItemMode.All ||
                (settings.itemMode == DeathmatchItemMode.EnergyOnly && (item.isActivatedItem || item.isEnergyWeapon)))
            {
                itemPool.Add(item.type);
            }
        }
    }

    public IEnumerator LoadNewArena()
    {
        ResetManager();

        if(currentScene.IsValid())
        {
            var async  = SceneManager.UnloadSceneAsync(currentScene);
            while (!async.isDone) yield return null;
        }

        var achievements = SaveGameManager.activeSlot == null ? null : SaveGameManager.activeSlot.achievements;
        var validMaps = ValidMaps(settings.mapRotation, achievements);
        var sceneName = validMaps[currentArenaIndex];
        currentArenaIndex = (currentArenaIndex + 1) % validMaps.Count;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        yield return null;

        currentScene = SceneManager.GetSceneByName(sceneName);
        scoreboard.gameObject.SetActive(false);
        CheckForSpawnPoints();
        HandleSpawnRoomItems();
        _currentArena = FindObjectOfType<Room>();

        foreach (var p in players)
        {
            RespawnPlayer(p);
        }

        Resources.UnloadUnusedAssets();
        _ready = true;
    }

    public void HandleSpawnRoomItems()
    {
        if (settings.spawnRoomItems)
        {
            var starters = FindObjectsOfType<MajorItemRoulettePickUp>();
            foreach (var item in starters) { spawnRoomItems.AddRange(item.possibleItems); }
            itemPool.RemoveWhere(i => spawnRoomItems.Contains(i));
        }
    }

    public void CheckForSpawnPoints()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("DMSpawnPoint");
        molemanSpawnPoints = FindObjectsOfType<MolemanDMSpawn>();
    }

    public void Update()
    {
        if (!_ready) return;

#if DEBUG
        if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.K))
        {
            foreach (var p in players)
            {
                p.Hurt(2000);
            }
        }
#endif

        if (!PauseMenu.instance || !PauseMenu.instance.visible)
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
#if ARCADE
                if ((_controllers[i].GetButton("UISubmit") || _controllers[i].GetButton("Pause")) && !idsJoined.Contains(i))
                {
                    if (SaveGameManager.instance)
                    {
                        if(SaveGameManager.instance.saveFileData.freePlay)
                        {
                            PlayerJoin(i);
                        }
                        else if (SaveGameManager.instance.credits > 0)
                        {
                            SaveGameManager.instance.credits = SaveGameManager.instance.credits - 1;
                            PlayerJoin(i);
                        }
                    }
                }
#else
                if (_controllers[i].GetAnyButton() && !idsJoined.Contains(i)) { PlayerJoin(i); }
#endif
            }
        }

        if(!_playersActive)
        {
            _playersActive = players.Any((p) => p.isActiveAndEnabled);
        }
        else if (settings.mode == DeathmatchMode.TimeLimit)
        {
            UpdateTime();
        }

        if(settings.molemanSpawnRate > 0)
        {
            MolemanUpdate();
        }
    }

    public void MolemanUpdate()
    {
        _molemanCounter -= Time.deltaTime;
        if (molemenPrefabs != null && molemenPrefabs.Length > 0 && 
            _currentArena && _molemanCounter <= 0)
        {
            _molemen.RemoveWhere((m) => !m);
            if (_molemen.Count < settings.maxMolemen)
            {
                var prefab = molemenPrefabs[Random.Range(0, molemenPrefabs.Length)];
                _molemanCounter = 60 / settings.molemanSpawnRate;
                var spawn = molemanSpawnPoints[Random.Range(0, molemanSpawnPoints.Length)];
                var moleman = Instantiate(prefab, spawn.transform.position, Quaternion.identity, _currentArena.transform);
                moleman.SetHuntMode();
                _molemen.Add(moleman.transform);
            }
        }
    }

    public void UpdateTime()
    {
        _time -= Time.deltaTime;

        if(!_oneMinuteRemaining && (int)_time <= 60)
        {
            _oneMinuteRemaining = true;
            AudioManager.instance.PlayOneShot(oneMinuteWarningAudio);
        }

        if ((int)_time <= _countDown && _countDown > 0)
        {
            _countDown--;
            AudioManager.instance.PlayOneShot(countdown[_countDown]);
        }

        if (_time <= 0) { DeclareWinner(); }
    }

    public void PlayerJoin(int id) { StartCoroutine(PlayerJoinRoutine(id)); }

    private IEnumerator PlayerJoinRoutine(int id)
    {
        idsJoined.Add(id);
        if (id >= 0 && id < ui.Length) { ui[id].joinPrompt.gameObject.SetActive(false); }

        var transition = cameras[id].GetComponentInChildren<TransitionFade>();
        if (settings.rouletteItems)
        {
            yield return StartCoroutine(WaitForItemSlotMachine(id));
        }

        var p = Instantiate(playerPrefab);
        p.respawning = true;
        p.name = "Player (" + id + ")";
        p.transform.position = NextSpawnPoint();
        AssignPlayerId(p, id);
        players.Add(p);
        PlayerManager.instance.AddPlayer(p);

        SceneManager.MoveGameObjectToScene(p.gameObject, SceneManager.GetSceneByName("DeathmatchCore"));

        if (onJoin != null) { onJoin.Invoke(id); }

        yield return null;

        if (settings.rouletteItems)
        {
            foreach (var item in ui[p.playerId].slotMachine.result) { p.CollectMajorItem(item, false); }
            p.MatchItems(true);
            yield return null;
            transition.FadeIn(0.5f);
        }

        p.respawning = false;
    }

    public void RespawnPlayer(DeathmatchPlayer player)
    {
        StartCoroutine(PlayerRespawnRoutine(player));
    }

    private IEnumerator PlayerRespawnRoutine(DeathmatchPlayer player)
    {        
        RecycleItems(player);

        player.respawning = true;
        player.ClearItems();
        player.gameObject.SetActive(false);

        var cam = cameras[player.playerId];
        var transition = cam.GetComponentInChildren<TransitionFade>();
        
        if (settings.rouletteItems)
        {
            transition.FadeOut(1.5f); 
            yield return StartCoroutine(WaitForItemSlotMachine(player.playerId));
        }

        player.transform.position = NextSpawnPoint();
        player.gameObject.SetActive(true);
        player.Respawn();

        yield return null;

        if (settings.rouletteItems)
        {
            foreach (var item in ui[player.playerId].slotMachine.result) { player.CollectMajorItem(item); }
            transition.FadeIn(0.5f);
        }

        Resources.UnloadUnusedAssets(); //Calls GC.Collect

        yield return null;
        player.respawning = false;
    }

    private IEnumerator WaitForItemSlotMachine(int id)
    {
        Debug.Log("wait for roulette");
        var playerUI = ui[id];
        var slotMachine = playerUI.slotMachine;
        playerUI.blackOut.gameObject.SetActive(true);

        var items = GetSlotMachineItems();

        slotMachine.Initialize(id, items[0], items[1], items[2]);
        slotMachine.gameObject.SetActive(true);

        while (!slotMachine.complete) { yield return null; }

        foreach (var array in items)
        {
            RecycleItems(array.Where(i => !slotMachine.result.Contains(i)));
        }

        yield return new WaitForSeconds(0.5f);
        playerUI.blackOut.gameObject.SetActive(false);
        slotMachine.gameObject.SetActive(false);
    }

    public void SetCollisionIgnore(Team team, Collider2D collider2D)
    {
        foreach (var p in players)
        {
            Physics2D.IgnoreCollision(collider2D, p.boxCollider2D, team == p.team);            
        }
    }

    public List<MajorItem[]> GetSlotMachineItems()
    {
        var result = new List<MajorItem[]> { new MajorItem[5], new MajorItem[5], new MajorItem[5] };
        var items = new List<MajorItemInfo>();
        foreach (var item in ItemManager.items.Values)
        {
            if (!item.deathmatch) continue;
            if (item.isEnergyWeapon) continue;
            if (item.isActivatedItem) continue;
            if (settings.spawnRoomItems && spawnRoomItems.Contains(item.type)) continue;
            items.Add(item);
        }
        
        var useless = new List<MajorItem>();

        for (int i = 0; i < 3; i++)
        {
            useless.Clear();
            for (int j = 0; j < 5; j++)
            {
                var pick = items[Random.Range(0, items.Count)];
                result[i][j] = pick.type;

                //Don't pick this item again
                items.Remove(pick);

                //Remove any picked items from the itemPool so they won't spawn in the world
                itemPool.Remove(pick.type);

                //Any items rendered useless by an item picked for this slot shouldn't have a chance to appear in subsequent slots
                //A slot might contain both arachnomorph and slide, but the next slot shouldn't
                useless.AddRange(pick.rendersUseless);
            }

            //Remove potentially useless items
            items.RemoveAll(item => useless.Contains(item.type));
        }

        return result;
    }

    public void RecycleItems(Player player)
    {
        var recycle = player.itemsPossessed.ToList();
        if(player.activatedItem) { recycle.Add(player.activatedItem.item); }
    }

    public void RecycleItems(IEnumerable<MajorItem> items)
    {
        if (settings.itemMode == DeathmatchItemMode.None) return;

        itemPool.AddRange(items.Where((i) =>
        {
            //player.itemsCollected contains all the activated items the player has ever collected
            //so if itemPool is a list it should check if it already contains an item or if it's on the field before recycling it

            //if (itemPool.Contains(i)) { return false; } //not needed as itemPool is a HashSet

            if (itemsOnTheField.Contains(i)) { return false; }
            if (spawnRoomItems.Contains(i)) { return false; }

            var item = ItemManager.items[i];
            return item.deathmatch && (settings.itemMode == DeathmatchItemMode.All || item.isEnergyWeapon || item.isActivatedItem);
        }));
    }

    public Vector3 NextSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var spawnPoint = (_lastSpawnPointUsed + 1) % spawnPoints.Length;
            _lastSpawnPointUsed = spawnPoint;
            return spawnPoints[spawnPoint].transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public bool AwardPoints(Team team, Team victimTeam)
    {
        if (score.ContainsKey(team))
        {
            bool win = false;

            var index = TeamToIndex(team);
            var color = IndexToRichTextColorString(index);
            var victimIndex = TeamToIndex(victimTeam);
            var victimColor = IndexToRichTextColorString(victimIndex);

            if (team == victimTeam)
            {
                if (index > 0 && index < ui.Length)
                {
                    ui[index].ShowNotification(color + " had an accident");
                }
                score[team]--;
                totalScore[team]--;
            }
            else
            {
                if (victimIndex > 0 && victimIndex < ui.Length)
                {
                    ui[victimIndex].ShowNotification("Killed by " + color);
                }
                score[team]++;
                totalScore[team]++;
            }

            var leader = score.First().Key;
            var highScore = score.First().Value;
            _tie = false;
            foreach (var kvp in score)
            {
                if (kvp.Key != leader)
                {
                    if (kvp.Value > highScore)
                    {
                        highScore = kvp.Value;
                        leader = kvp.Key;
                        _tie = false;
                    }
                    else if (kvp.Value == highScore)
                    {
                        _tie = true;
                    }
                }
            }

            if (settings.mode == DeathmatchMode.FragLimit && score[team] >= settings.fragLimit)
            {
                win = true;
                DeclareWinner();
            }

            if (!win)
            {
                var newHighScore = score.Values.Max();
                var newLeader = score.FirstOrDefault(s => s.Value == newHighScore).Key;
                if (newHighScore > _lastHighScore && (_lastHighScore == 0 || newLeader != _lastLeader))
                {
                    if (_lastHighScore != 0)
                    {
                        ui[TeamToIndex(_lastLeader)].ShowNotification("You've Lost the Lead");
                    }

                    _lastLeader = newLeader;
                    AudioManager.instance.PlayOneShot(takesTheLeadAudio[TeamToIndex(newLeader)]);

                    if (_lastHighScore == 0)
                    {
                        ui[index].ShowNotification("First Blood!");
                    }
                    else
                    {
                        ui[index].ShowNotification(color + " Takes the Lead");
                    }
                }
                else if (team != victimTeam)
                {
                    ui[index].ShowNotification("You killed " + victimColor);
                }

                _lastHighScore = newHighScore;
            }

            return true;
        }

        return team == Team.Enemy;
    }

    public void DeclareWinner()
    {
        foreach (var p in players)
        {
            if (p.activeSpecialMove) p.activeSpecialMove.DeathStop();
            if (p.selectedEnergyWeapon) p.selectedEnergyWeapon.ImmediateStop();
        }

        var currentLeader = score.First().Key;
        var highScore = score.First().Value;
        _tie = false;
        foreach (var kvp in score)
        {
            if (kvp.Key != currentLeader)
            {
                if (kvp.Value > highScore)
                {
                    highScore = kvp.Value;
                    currentLeader = kvp.Key;
                    _tie = false;
                }
                else if (kvp.Value == highScore)
                {
                    _tie = true;
                }
            }
        }

        StopAllCoroutines();

        _ready = false;
        allowPause = false;
        for (int i = 0; i < ui.Length; i++)
        {
            ui[i].slotMachine.gameObject.SetActive(false);
            ui[i].blackOut.gameObject.SetActive(true);
        }

        var index = TeamToIndex(currentLeader);

        if (_tie)
        {
            Debug.Log("Tie");
            AudioManager.instance.PlayOneShot(tieAudio);
            StartCoroutine(scoreboard.Show("Tie"));
        }
        else
        {
            string wins = IndexToColorString(index) + " Wins!";
            AudioManager.instance.PlayOneShot(winsAudio[index]);
            StartCoroutine(scoreboard.Show(wins));
        }        
    }

    public void AssignPlayerId(DeathmatchPlayer player, int id)
    {
        var team = IndexToTeam(id);
        player.playerId = id;
        player.team = team;

        if(id >= 0 && id < cameras.Length)
        {
            var c = cameras[id];
            c.player = player;
            player.mainCamera = c;
            c.gameObject.SetActive(true);
        }

        if(id >= 0 && id < screens.Length)
        {
            screens[id].gameObject.SetActive(true);
        }

        if(id >= 0 && id < ui.Length)
        {
            ui[id].AssignPlayer(player);
            ui[id].gameObject.SetActive(true);
        }

        if(id >= 0 && id < materials.Length)
        {
            player.material = materials[id];            
        }
    }

    public Player GetClosestPlayerInRange(Transform origin, float range, bool inFrontOnly)
    {
        Player closest = null;
        float shortestDistance = range;
        foreach (var player in players)
        {
            var distance = Vector3.Distance(player.transform.position, origin.position);
            if (distance > range ||
                (inFrontOnly && Mathf.Sign(player.transform.position.x - origin.position.x) != Mathf.Sign(origin.right.x)))
            {
                continue;
            }

            if(distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = player;
            }
        }
        return closest;
    }

    public void SpawnDeathParticles(Vector3 position)
    {
        StartCoroutine(SpawnDeathParticlesRoutine(position));
    }

    private IEnumerator SpawnDeathParticlesRoutine(Vector3 position)
    {
        var particlesCenter = new GameObject("particlesCenter");
        particlesCenter.transform.position = position;

        for (int i = 0; i < 3; i++)
        {
            var particleCount = 8f;
            var startingDistance = 0.25f;
            for (int j = 0; j < particleCount; j++)
            {
                var progress = (j / particleCount) * 360 * Mathf.Deg2Rad;
                var particle = Instantiate(players.First().playerDeathParticle);
                particle.transform.parent = particlesCenter.transform;
                particle.transform.localPosition = new Vector3(Mathf.Sin(progress), Mathf.Cos(progress)) * startingDistance;
            }
            yield return new WaitForSeconds(0.33f);
        }

        while (particlesCenter.transform.childCount > 0) yield return null;

        Destroy(particlesCenter);
    }

    public void RestartMatch()
    {
        StartCoroutine(RestartMatchRoutine());
    }

    private IEnumerator RestartMatchRoutine()
    {
        bool restart = false;
        while(!restart)
        {
            foreach (var p in players)
            {
                if (p.controller.GetButtonDown("UISubmit"))
                {
                    restart = true;
                    break;
                }
            }
            yield return null;
        }

        yield return StartCoroutine(LoadNewArena());

        if (!settings.rouletteItems)
        {
            for (int i = 0; i < ui.Length; i++)
            {
                ui[i].blackOut.gameObject.SetActive(false);
            }
        }

        Time.timeScale = 1;
        allowPause = true;
        //_ready = true; //set at end of LoadNewArena
    }

    public MajorItem PickItemToSpawn()
    {
        if (itemPool.Count > 0)
        {
            var energyWeapons = 0;
            var activatedItems = 0;
            var passives = 0;

            foreach (var item in itemsOnTheField)
            {
                if (item == MajorItem.None) continue;

                var info = ItemManager.items[item];
                if (info.isEnergyWeapon) { energyWeapons++; }
                else if (info.isActivatedItem) { activatedItems++; }
                else { passives++; }
            }

            var subPool = new HashSet<MajorItem>();

            if (settings.itemMode == DeathmatchItemMode.All && passives < activatedItems && passives < energyWeapons)
            {
                subPool.AddRange(itemPool.Where(item => { var info = ItemManager.items[item]; return !info.isActivatedItem && !info.isEnergyWeapon; }));
            }
            else if (activatedItems < energyWeapons)
            {
                subPool.AddRange(itemPool.Where(item => ItemManager.items[item].isActivatedItem));
            }
            else
            {
                subPool.AddRange(itemPool.Where(item => ItemManager.items[item].isEnergyWeapon));
            }

            if (subPool.Count == 0) { subPool = new HashSet<MajorItem>(itemPool); }

            var i = subPool.ElementAt(Random.Range(0, subPool.Count));
            itemPool.Remove(i);
            itemsOnTheField.Add(i);
            return i;
        }
        else
        {
            return MajorItem.None;
        }
    }

    public Team IndexToTeam(int index)
    {
        switch (index)
        {
            default:
            case 0:
                return Team.DeathMatch0;
            case 1:
                return Team.DeathMatch1;                
            case 2:
                return Team.DeathMatch2;
            case 3:
                return Team.DeathMatch3;
        }
    }

    public string IndexToRichTextColorString(int index)
    {
        switch (index)
        {
            default:
            case -1:
                return "<color=#ffffffff>"+Constants.GetMolemanName()+"</color>";
            case 0:
                return "<color=#ff4444ff>Red</color>";
            case 1:
                return "<color=#4444ffff>Blue</color>";
            case 2:
                return "<color=yellow>Yellow</color>";
            case 3:
                return "<color=green>Green</color>";
        }
    }

    public string IndexToColorString(int index)
    {
        switch (index)
        {
            default:
            case 0:
                return "Red";
            case 1:
                return "Blue";
            case 2:
                return "Yellow";
            case 3:
                return "Green";
        }
    }

    public string TeamToColorString(Team team)
    {
        switch (team)
        {
            default:
            case Team.DeathMatch0:
                return "Red";
            case Team.DeathMatch1:
                return "Blue";
            case Team.DeathMatch2:
                return "Yellow";
            case Team.DeathMatch3:
                return "Green";
        }
    }

    public int TeamToIndex(Team team)
    {
        switch(team)
        {
            default:
            case Team.DeathMatch0:
                return 0;
            case Team.DeathMatch1:
                return 1;
            case Team.DeathMatch2:
                return 2;
            case Team.DeathMatch3:
                return 3;
            case Team.Enemy:
                return -1;
        }
    }

    public void DropPlayer(int id)
    {
        Debug.Log("Drop " + id);

        var player = players.FirstOrDefault((p) => p.playerId == id);

        if (id >= 0 && player)
        {
            RecycleItems(player);

            PlayerManager.instance.RemovePlayer(player);

            if (id < cameras.Length)
            {
                var c = cameras[id];
                c.player = null;
                c.gameObject.SetActive(false);
            }

            if (id < screens.Length) { screens[id].gameObject.SetActive(false); }
            if (id < ui.Length) { ui[id].DropPlayer(player); }

            score[player.team] = 0;
            totalScore[player.team] = 0;
            deaths[player.team] = 0;
            players.Remove(player);
            Destroy(player.gameObject);
            onDrop.Invoke(id);
            StartCoroutine(WaitRemoveID(id));
        }

        _playersActive = players.Any((p) => p.isActiveAndEnabled);
    }

    private IEnumerator WaitRemoveID(int id)
    {
        yield return new WaitForSeconds(0.5f);
        idsJoined.Remove(id);
    }

    private void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }
}