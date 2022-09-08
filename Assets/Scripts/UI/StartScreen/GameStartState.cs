using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class GameStartState : ScreenState
{
    public NewGameScreenState newGameScreenState;
    public FileScreenState fileScreenState;
    public SettingsScreenState settingsScreenState;
    public AchievementsScreenState achievementScreenState;
    public CollectionScreenState collectionScreenState;
    public StatsScreenState statScreenState;
    public EndingsMenuState endingMenuState;
    public DebugMenuState debugMenuState;
    public Text fileName;
    public Button continueButton;
 
    private MenuOptions _menuOptions;

    protected override void Start()
    {
        _menuOptions = GetComponentInChildren<MenuOptions>();
#if ARCADE
        var debug = _menuOptions.GetComponentsInChildren<Button>(true).FirstOrDefault(m => m.name == "Debug");
        if (debug != null) { debug.gameObject.SetActive(true); }
        if(_menuOptions.menuOptions.Count > 0 && !_menuOptions.menuOptions.Contains(debug))
        {
            _menuOptions.menuOptions.Add(debug);
        }
#endif

        base.Start();
        _menuOptions.parentScreenState = this;
        if(SaveGameManager.activeGame != null)
        {
            _menuOptions.SelectOption(continueButton);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if(SaveGameManager.activeGame != null)
        {
            var activeGame = SaveGameManager.activeGame;
            //Sanity Check
            if (activeGame.runCompleted || activeGame.layout == null || activeGame.layout.roomAbstracts.Count == 0)
            {
                SaveGameManager.instance.ClearActiveGame();
                Debug.LogWarning("Acive Game is corrupted or run was completed. Deleting.");
                continueButton.interactable = false;
            }
            else
            {
                continueButton.interactable = true;
            }
        }
        else
        {
            continueButton.interactable = false;
        }
        
    }

    public void SetSelectedSlot(int selectedSlot)
    {
        SaveGameManager.instance.selectedSaveSlot = selectedSlot;
        fileName.text = "File " + (selectedSlot + 1);
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(fileScreenState);
        }

        var activeSlot = SaveGameManager.activeSlot;
        var keyboard = _controller.controllers.Keyboard;
#if DEBUG || UNITY_EDITOR
        if (keyboard != null && keyboard.GetKey(KeyCode.LeftControl) && keyboard.GetKeyDown(KeyCode.A) && activeSlot != null)
        {
            var allAchievements = Enum.GetValues(typeof(AchievementID)).Cast<AchievementID>().ToList();
            foreach (var a in allAchievements)
            {
                if (!activeSlot.achievements.Contains(a)) { activeSlot.achievements.Add(a); }
            }
            Debug.Log("Awarding all achievements");
            SaveGameManager.instance.Save();
            UISounds.instance.Confirm();

            activeSlot.itemsCollected.Clear();
            foreach (MajorItem item in Enum.GetValues(typeof(MajorItem)))
            {
                activeSlot.itemsCollected.Add(item);
            }
        }
#elif STEAM
        if (keyboard != null && keyboard.GetKey(KeyCode.LeftControl) && keyboard.GetKeyDown(KeyCode.A) && activeSlot != null)
        {
            var allAchievements = Enum.GetValues(typeof(AchievementID)).Cast<AchievementID>().ToList();
            foreach (var a in allAchievements)
            {
                bool achieved = false;
                if (!activeSlot.achievements.Contains(a) && Steamworks.SteamUserStats.GetAchievement(a.ToString(), out achieved) && achieved)
                {
                    activeSlot.achievements.Add(a);
                }
            }
            Debug.Log("Syncing game achievements with Steam!");
            SaveGameManager.instance.Save();
            UISounds.instance.Confirm();
        }

#if DEBUG
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
        {
            UISounds.instance.Confirm();

            Debug.Log("Clearing Steam Achievements");
            foreach (var achievement in Enum.GetNames(typeof(AchievementID)))
            {
                Debug.Log("Clearing " + achievement);
                Steamworks.SteamUserStats.ClearAchievement(achievement);
                Steamworks.SteamUserStats.StoreStats();
            }

            foreach (var slot in SaveGameManager.instance.saveFileData.saveSlots)
            {
                if (slot.Value != null)
                {
                    slot.Value.achievements.Clear();
                }
            }

            SaveGameManager.instance.Save();
        }
#endif
#endif
    }

    public void Continue()
    {
        Debug.Log("Continue Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        InputHelper.instance.AssignPlayer1LastActiveController();
        SceneManager.LoadScene("MainScene");
    }

    public void NewGame()
    {
        GoToState(newGameScreenState);
    }

    public void Settings()
    {
        GoToState(settingsScreenState);
    }

    public void Achievements()
    {
        GoToState(achievementScreenState);
    }

    public void Collection()
    {
        GoToState(collectionScreenState);
    }

    public void Stats()
    {
        GoToState(statScreenState);
    }

    public void Endings()
    {
        GoToState(endingMenuState);
    }

    public void DebugMenu()
    {
        GoToState(debugMenuState);
    }
}
