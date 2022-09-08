using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameScreenState : ScreenState
{
    public ScreenState previousState;
    public InputSeedScreen inputSeedState;
    public DeathmatchStartScreenState deathmatchStartState;
    public ChallengeScreenState challengesState;
    public GameObject newGameAreYouSure;
    public Button continueButton;

    private MenuOptions _menuOptions;
    private bool _skipUpdate;

    protected override void Start()
    {
        base.Start();
        if (SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            SaveGameManager.instance.selectedSaveSlot = 0;
        }

        _menuOptions = GetComponentInChildren<MenuOptions>();
        _menuOptions.parentScreenState = this;
    }

    public override void ReadyUpdate()
    {
        if (_skipUpdate) return;

        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(previousState);
        }
    }

    public void NewGame()
    {
        if (SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            ConNewGame();
            return;
        }

        if (SaveGameManager.activeGame != null)
        {
            StartCoroutine(WaitForNewGameConfirm());
        }
        else
        {
            StartNewGame();
        }
    }

    public void ConNewGame()
    {
        var activeSlot = SaveGameManager.activeSlot;

        var achievements = Enum.GetValues(typeof(AchievementID)).Cast<AchievementID>().ToList();
        achievements.Remove(AchievementID.None);
        achievements.Remove(AchievementID.CognitiveStabilizer);
        achievements.Remove(AchievementID.TheFleshening);
        achievements.Remove(AchievementID.TheFlesheningII);
        achievements.Remove(AchievementID.TheQuickening);
        achievements.Remove(AchievementID.FastBot);
        achievements.Remove(AchievementID.FightBot);
        achievements.Remove(AchievementID.GunBot);
        achievements.Remove(AchievementID.OrbBot);
        achievements.Remove(AchievementID.ThoroughBot);
        achievements.Remove(AchievementID.GlitchMap);
        achievements.Remove(AchievementID.GlitchShell);
        achievements.Remove(AchievementID.GlitchScrap);
        achievements.Remove(AchievementID.GlitchModule);

        activeSlot.tutorialSmithVisted = false;
        activeSlot.achievements = achievements;
        activeSlot.totalDeaths = 0;
        activeSlot.victories = 0;
        activeSlot.deathmatchSettings.timeLimit = 300;
        activeSlot.deathmatchSettings.mode = DeathmatchMode.TimeLimit;

        Debug.Log("New Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        InputHelper.instance.AssignPlayer1LastActiveController();
        SaveGameManager.instance.NewGame(GameMode.Normal);
        SaveGameManager.instance.Save();
        SceneManager.LoadScene("NewGame");
    }

    public void Continue()
    {
        Debug.Log("Continue Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        InputHelper.instance.AssignPlayer1LastActiveController();
        SceneManager.LoadScene("MainScene");
    }


    public override void OnEnable()
    {
        base.OnEnable();
        if(SaveGameManager.instance.saveFileData.conventionMode && SaveGameManager.activeGame != null)
        {
            var activeGame = SaveGameManager.activeGame;
            //Sanity Check
            if (activeGame.runCompleted || activeGame.layout == null || activeGame.layout.roomAbstracts.Count == 0)
            {
                SaveGameManager.instance.ClearActiveGame();
                Debug.LogWarning("Acive Game is corrupted or run was completed. Deleting.");
                continueButton.gameObject.SetActive(false);
            }
            else
            {
                continueButton.gameObject.SetActive(true);
                if(_menuOptions && !_menuOptions.menuOptions.Contains(continueButton))
                {
                    _menuOptions.ResetOptions();
                }
            }
        }
        else
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    public void StartNewGame()
    {
        Debug.Log("New Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        InputHelper.instance.AssignPlayer1LastActiveController();
        SaveGameManager.instance.NewGame(GameMode.Normal);
        SaveGameManager.instance.Save(false, true);
        SceneManager.LoadScene("NewGame");
    }

    public IEnumerator WaitForNewGameConfirm()
    {
        _skipUpdate = true;
        newGameAreYouSure.gameObject.SetActive(true);

        yield return null;

        bool _finished = false;
        while (!_finished)
        {
            if (_controller.GetButtonDown("UISubmit"))
            {
                _finished = true;
                UISounds.instance.Confirm();
                StartNewGame();
            }
            else if (_controller.GetButtonDown("UICancel"))
            {
                UISounds.instance.Cancel();
                _finished = true;
            }
            yield return false;
        }

        _skipUpdate = false;
        newGameAreYouSure.gameObject.SetActive(false);
    }

    public void SeededRun()
    {
        GoToState(inputSeedState);
    }

    public void Deathmatch()
    {
#if ARCADE
        InputHelper.instance.RestoreDefaultAssignments();

        if(SaveGameManager.instance.saveFileData.conventionMode)
        {
            SaveGameManager.deathmatchSettings.mode = DeathmatchMode.TimeLimit;
            SaveGameManager.deathmatchSettings.timeLimit = 300;
            SaveGameManager.deathmatchSettings.maxMolemen = 0;
            SaveGameManager.deathmatchSettings.molemanSpawnRate = 0;
            SaveGameManager.deathmatchSettings.rouletteItems = false;
            SaveGameManager.deathmatchSettings.mapRotation = new List<string>(DeathmatchManager.allMaps);
            SaveGameManager.instance.Save(false, true);
            SceneManager.LoadScene("DeathmatchCore");
            return;
        }
#endif
        GoToState(deathmatchStartState);
    }

    public void Challenges()
    {
        GoToState(challengesState);
    }
}
