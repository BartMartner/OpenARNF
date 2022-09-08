using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using Newtonsoft.Json;
using Random = UnityEngine.Random;
using System.Threading;

public class SaveGameManager : MonoBehaviour
{
    public static readonly Version version = new Version(1, 6, 1, 35);
    public static SaveGameManager instance;
    private SaveFileData _saveFileData = new SaveFileData();
    public SaveFileData saveFileData
    {
        get
        {
            return _saveFileData;
        }
    }

    private int _selectedSaveSlot = -1;
    public Action OnSelectedSaveSlotChanged;
    public int selectedSaveSlot
    {
        get { return _selectedSaveSlot; }
        set
        {
            _selectedSaveSlot = value;
            if (OnSelectedSaveSlotChanged != null)
            {
                OnSelectedSaveSlotChanged();
            }
        }
    }

    public static bool beastGutsUnlocked
    {
        get { return activeSlot != null && activeSlot.achievements.Contains(AchievementID.BeastGuts); }
    }

    private float timeSinceLastSave;

    public static SaveSlotData activeSlot
    {
        get
        {
            if (!instance)
            {
                return null;
            }
            else
            {
                SaveSlotData data;
                if (!instance.saveFileData.saveSlots.TryGetValue(instance.selectedSaveSlot, out data) || data == null)
                {
                    if (instance.selectedSaveSlot == -1) return null;

                    data = new SaveSlotData();
                    data.slotNumber = instance.selectedSaveSlot;
                    instance.saveFileData.saveSlots[instance.selectedSaveSlot] = data;
                    instance.SaveData();
                }
                return data;
            }
        }
    }

    public static DeathmatchSettings deathmatchSettings
    {
        get
        {
            var slot = activeSlot;
            if (activeSlot == null)
            {
                return null;
            }
            else
            {
                return activeSlot.deathmatchSettings;
            }
        }
    }

    public static int fightNumber
    {
        get
        {
            if (activeSlot != null)
            {
                return activeSlot.totalDeaths;
            }
            else
            {
                return 0;
            }
        }
    }

    public static SaveGameData activeGame
    {
        get
        {
            if(activeSlot != null && !DeathmatchManager.instance)
            {
                return activeSlot.activeGameData;
            }
            else
            {
                return null;
            }
        }
    }

    public DateTime sessionStart;
    public Action onSave;

    private bool _saving;
    public bool saving { get { return _saving; } }
    private bool _needToSave;
    private bool _forceSave;
    private bool _needSaveLayout;
    private bool _needOnSave;
    private string _saveGameFileKey;
    private FileService _fileService;
    private bool _saveAllSlots;

#if ARCADE
    public int credits;
#endif

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

//#if UNITY_SWITCH
//        _fileService = new SwitchFileService();
//#elif STEAM
//        _fileService = new SteamFileService();
//#else
        _fileService = new FileService();
//#endif

        _fileService.Initialize();

        _saveGameFileKey = "SaveFile";
        LoadSaveFile();
    }

    public void LoadSaveFile()
    {
        sessionStart = DateTime.UtcNow;

        var jsonSerializerSettings = new JsonSerializerSettings()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Error = (s, e) => e.ErrorContext.Handled = true,
        };

        var json = _fileService.Read(_saveGameFileKey);
        if (!string.IsNullOrEmpty(json))
        {
            _saveFileData = JsonConvert.DeserializeObject<SaveFileData>(json, jsonSerializerSettings);

            bool needAchievementUpdate = true; //false;
#if !UNITY_SWITCH
            bool clearActiveGames = false;
#endif            
            bool version25CleanUp = false;
            bool version31CleanUp = false;

            if (_saveFileData.lastVersion != version.ToString())
            {
                //needAchievementUpdate = true;
                Debug.Log("Save File Version Mismatch");
                Version last = new Version(_saveFileData.lastVersion);

                if (last.MinorRevision < 31) { version31CleanUp = true; }
                if (last.MinorRevision < 25) { version25CleanUp = true; }
#if !UNITY_SWITCH
                if (last.MinorRevision < 22) { clearActiveGames = true; }
#endif
            }

            for (int i = 0; i < 3; i++)
            {
                var slotJSON = _fileService.Read(SlotFileName(i));
                if (!string.IsNullOrEmpty(slotJSON))
                {
                    try
                    {            
                        _saveFileData.saveSlots[i] = JsonConvert.DeserializeObject<SaveSlotData>(slotJSON, jsonSerializerSettings);
                        var slot = _saveFileData.saveSlots[i];

                        var layoutJson = _fileService.Read(SlotLayoutFileName(i));
                        if (!string.IsNullOrEmpty(layoutJson) && slot.activeGameData != null)
                        {
                            slot.activeGameData.layout = JsonConvert.DeserializeObject<RoomLayout>(layoutJson, jsonSerializerSettings);
                        }
                        else
                        {
                            slot.activeGameData = null;
                        }

                        if(version25CleanUp)
                        {
                            slot.secretSeeds.Remove("JAMESBONDGOLDENEYE111111");
                            if(slot.lowestCompletionRate == 0 && slot.victories == 0 && slot.highestCompletionRate == 0)
                            {
                                slot.lowestCompletionRate = -1;
                            }
                        }

                        if(version31CleanUp)
                        {
                            slot.secretSeeds.Remove("MAPSONMAPSONMAPSONMAPSYO");
                            slot.secretSeeds.Remove("MISSINGBOYISAACFOUNDDEAD");
                            slot.pastSeeds.Clear();
                        }

                        //Will need code later to add new Deathmatch Maps
                        if(slot.deathmatchSettings.mapRotation == null || slot.deathmatchSettings.mapRotation.Count == 0)
                        {
                            slot.deathmatchSettings.mapRotation = new List<string>(DeathmatchManager.allMaps);
                        }

#if !UNITY_SWITCH
                        if (clearActiveGames)
                        {
                            clearActiveGames = false;
                            slot.activeGameData = null;
                        }
#endif

                        if (needAchievementUpdate) slot.needAchievementUpdate = needAchievementUpdate;
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError(exc);
                    }
                }
            }
        }
        else
        {
            _saveFileData = new SaveFileData();
            _saveFileData.dirty = true;
            SaveData();
        }

#if UNITY_SWITCH
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
#else
        var currentResolution = Screen.currentResolution;
        if (_saveFileData.fullScreen)
        {
            Screen.SetResolution(currentResolution.width, currentResolution.height, true);
#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
#endif
        }
        else
        {
            Screen.SetResolution(_saveFileData.resolution.x, _saveFileData.resolution.y, false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (saveFileData.vSync)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
        }
#endif

        AudioListener.volume = SaveGameManager.instance.saveFileData.soundVolume;

        _saveFileData.saveSlots[-1] = null;
    }

    public string SlotFileName(int slotNumber)
    {
        return _saveGameFileKey + "Slot" + slotNumber;
    }

    public string SlotLayoutFileName(int slotNumber)
    {
        return _saveGameFileKey + "Slot" + slotNumber + "Layout";
    }

    //TODO: add option to pass in seed
    public void NewGame(GameMode gameMode, bool raceMode = false)
    {
        if (Player.instance) { onSave -= Player.instance.OnSave; }

        activeSlot.activeGameData = new SaveGameData();
        activeSlot.activeGameData.gameMode = gameMode;
        activeSlot.activeGameData.raceMode = raceMode;

        if(gameMode == GameMode.BossRush)
        {
            activeSlot.activeGameData.healthUpsCollected = 1;
            activeSlot.activeGameData.energyUpsCollected = 1;
            activeSlot.activeGameData.shotSpeedUpsCollected = 1;
            activeSlot.activeGameData.speedUpsCollected = 1;
            activeSlot.activeGameData.damageUpsCollected = 1;
            activeSlot.activeGameData.attackUpsCollected = 1;
        }

        activeSlot.activeGameData.seed = Random.Range(0, Constants.maxSeed);
        activeSlot.activeGameData.playerHealth = Constants.startingHealth * 0.5f;

        if (activeSlot.totalRuns < uint.MaxValue)
        {
            activeSlot.totalRuns++;
        }

        _saveFileData.dirty = true;
        SaveData();
    }

    public void ClearActiveGame()
    {
        if (activeSlot != null)
        {
            activeSlot.activeGameData = null;
            _saveFileData.dirty = true;
            _needToSave = true;
            _needOnSave = true;
            _forceSave = true;
        }
    }

    private void LateUpdate()
    {
        timeSinceLastSave += Time.unscaledDeltaTime;

        if (_saving) return;

#if ARCADE
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("EXTRA LIFE!");
            credits++;
        }
#endif

        if (_needOnSave && onSave != null)
        {
            onSave();
            _needOnSave = false;
        }

        if (_needToSave && (timeSinceLastSave >= 9f || _forceSave))
        {
            //Debug.Log("Attempt to save. Forced = " + _forceSave);
            _needToSave = false;

            if (_forceSave)
            {
                saveFileData.dirty = true;
                _forceSave = false;
            }

            timeSinceLastSave = 0;

            try
            {
                if (!saveFileData.fullScreen)
                {
                    saveFileData.resolution = new Int2D(Screen.width, Screen.height);
                }

#if UNITY_SWITCH
                SaveData();
#else
                Thread saveThread = new Thread(SaveData);
                saveThread.Start();
#endif
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.ToString());
            }
        }
    }

    private void SaveData()
    {
        try
        {
            _saving = true;

            string json;
            var versionString = version.ToString();
            if (saveFileData.lastVersion != versionString)
            {
                saveFileData.lastVersion = versionString;                
            }

            if (saveFileData.dirty)
            {
                Debug.Log("Saving " + _saveGameFileKey);
                lock (saveFileData)
                {
                    json = JsonConvert.SerializeObject(saveFileData, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }

                _fileService.Write(_saveGameFileKey, json);
                saveFileData.dirty = false;
            }

            SaveSlotData data;
            if (saveFileData.saveSlots.TryGetValue(selectedSaveSlot, out data) && data != null)
            {
                lock (data)
                {
                    json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }

                _fileService.Write(SlotFileName(data.slotNumber), json);
                
                if(_needSaveLayout && data.activeGameData != null && data.activeGameData.layout != null)
                {
                    lock(data.activeGameData.layout)
                    {
                        json = JsonConvert.SerializeObject(data.activeGameData.layout, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }
                    _fileService.Write(SlotLayoutFileName(data.slotNumber), json);
                    _needSaveLayout = false;
                }
            }

            _saving = false;
        }
        catch (Exception exc)
        {
            Debug.LogError(exc.ToString());            
            _saving = false;
        }
    }

    public void DeleteSlot(int slotNumber)
    {
        _fileService.Delete(SlotFileName(slotNumber));
        _fileService.Delete(SlotLayoutFileName(slotNumber));
        saveFileData.saveSlots.Remove(slotNumber);
        _needToSave = true;        
        _forceSave = true;
        selectedSaveSlot = -1;
    }

    /// <summary>
    /// marks _needToSave true, cause SaveGameManager to save in LateUpdate
    /// </summary>
    public void Save(bool saveLayout = false, bool forceSave = false)
    {
        if (!DeathmatchManager.instance)
        {
            _needToSave = true;
            _needOnSave = true;
            _forceSave = forceSave ? true : _forceSave;
            _needSaveLayout = saveLayout ? true : _needSaveLayout;
        }
    }  

    public void OnDestroy()
    {
        if(_fileService != null)
        {
            _fileService.OnDestroy();
        }

        if (instance == this)
        {
            instance = null;
        }
    }
}

