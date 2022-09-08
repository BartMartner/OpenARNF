using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingsMenuState : ScreenState
{
    public GameStartState gameStartState;
    public Button intro;
    public Button ending1;
    public Button ending2;
    public Button ending3;
    public Button bossRush;
    public Button exterminator;
    public Button megamap;
    private MenuOptions _menuOptions;

    protected override void Start()
    {
        base.Start();
        _menuOptions = GetComponentInChildren<MenuOptions>();
        _menuOptions.parentScreenState = this;
        if (SaveGameManager.activeGame != null)
        {
            _menuOptions.SelectOption(intro);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null)
        {
            ending1.interactable = activeSlot.megaBeastKills > 0;
            ending2.interactable = activeSlot.bossKills.ContainsKey(BossName.MegaBeastCore) && activeSlot.bossKills[BossName.MegaBeastCore] > 0;
            ending3.interactable = activeSlot.bossKills.ContainsKey(BossName.GlitchBoss) && activeSlot.bossKills[BossName.GlitchBoss] > 0;
            bossRush.interactable = activeSlot.challengesCompleted.Contains(GameMode.BossRush);
            exterminator.interactable = activeSlot.challengesCompleted.Contains(GameMode.Exterminator);
            megamap.interactable = activeSlot.challengesCompleted.Contains(GameMode.MegaMap);
        }
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(gameStartState);
        }
    }

    public void Intro()
    {
        SceneManager.LoadScene("Backstory");
    }

    public void Ending1()
    {
        SceneManager.LoadScene("EndScreen01");
    }

    public void Ending2()
    {
        SceneManager.LoadScene("EndScreen02");
    }

    public void Ending3()
    {
        SceneManager.LoadScene("EndScreen03");
    }

    public void BossRush()
    {
        SceneManager.LoadScene("BossRushEndScreen");
    }

    public void Exterminator()
    {
        SceneManager.LoadScene("ExterminatorEndScreen");
    }

    public void MegaMap()
    {
        SceneManager.LoadScene("MegaMapEndScreen");
    }
}