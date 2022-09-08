using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChallengeScreenState : ScreenState
{
    public ScreenState previousState;
    public GameObject newGameAreYouSure;
    public List<ChallengeOption> challenges;
    public Sprite iconIncomplete;
    public Sprite iconComplete;
    public Sprite iconLocked;
    public Text unlockMessage;

    private MenuOptions _menuOptions;
    private bool _skipUpdate;

    protected override void Start()
    {
        base.Start();
        _menuOptions = GetComponentInChildren<MenuOptions>();
        _menuOptions.parentScreenState = this;
    }

    public override void AppearStart()
    {
        bool anyUnlocked = false;
        foreach (var c in challenges)
        {
            if (SaveGameManager.activeSlot.challengesCompleted.Contains(c.associatedGameMode))
            {
                anyUnlocked = true;
                c.button.interactable = true;
                c.icon.sprite = iconComplete;
            }
            else if (AchievementEarned(c.associatedGameMode))
            {
                anyUnlocked = true;
                c.button.interactable = true;
                c.icon.sprite = iconIncomplete;
            }
            else
            {
                c.button.interactable = false;
                c.icon.sprite = iconLocked;
            }
        }

        unlockMessage.gameObject.SetActive(!anyUnlocked);
        base.AppearStart();
    }

    public bool AchievementEarned(GameMode gameMode)
    {
        var mode = gameMode.ToString();
        return SaveGameManager.activeSlot.achievements.Any((a) => a.ToString() == mode);
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

    public void Exterminator()
    {
        NewGame(GameMode.Exterminator);
    }

    public void BossRush()
    {
        NewGame(GameMode.BossRush);
        //NewGame(GameMode.ClassicBossRush);
    }

    public void MegaMap()
    {
        NewGame(GameMode.MegaMap);
    }

    public void NewGame(GameMode gameMode)
    {
        if (SaveGameManager.activeGame != null)
        {
            StartCoroutine(WaitForNewGameConfirm(gameMode));
        }
        else
        {
            StartNewGame(gameMode);
        }
    }

    public void StartNewGame(GameMode gameMode)
    {
        Debug.Log("New Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        InputHelper.instance.AssignPlayer1LastActiveController();
        SaveGameManager.instance.NewGame(gameMode);
        SaveGameManager.instance.Save(false, true);
        SceneManager.LoadScene("NewGame");
    }

    public IEnumerator WaitForNewGameConfirm(GameMode gameMode)
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
                StartNewGame(gameMode);
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
}