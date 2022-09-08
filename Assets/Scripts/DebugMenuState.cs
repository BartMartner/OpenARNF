using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuState : ScreenState
{
    public ScreenState previousState;
    public ScreenState titleScreenState;
    public Text forcePhaseShell;
    public Text forceCrystalMines;
    public Text conventionMode;
    public Text freePlay;
    private MenuOptions _menuOptions;
    private bool _saveOnExit;

#if !ARCADE
    private void Awake()
    {
        Destroy(freePlay.gameObject);
    }
#endif

    protected override void Start()
    {
        base.Start();
        _menuOptions = GetComponentInChildren<MenuOptions>();
        _menuOptions.parentScreenState = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                forceCrystalMines.text = "Force Crystal Mines - " + (slot.forceCrystalMine ? "On" : "Off");
                forcePhaseShell.text = "Force Phase Shell - " + (slot.forcePhaseShell ? "On" : "Off");                
                conventionMode.text = "Convention Mode - " + (SaveGameManager.instance.saveFileData.conventionMode ? "On" : "Off");
#if ARCADE
                freePlay.text = "Free Play - " + (SaveGameManager.instance.saveFileData.freePlay ? "On" : "Off");
#endif
            }
        }
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UILeft") || _controller.GetButtonDown("UIRight"))
        {
            if (forceCrystalMines && _menuOptions.selectedMenuOption.gameObject == forceCrystalMines.gameObject)
            {
                ToggleForceCrystalMines();
            }

            if (forcePhaseShell && _menuOptions.selectedMenuOption.gameObject == forcePhaseShell.gameObject)
            {
                ToggleForcePhaseShell();
            }

            if(conventionMode && _menuOptions.selectedMenuOption.gameObject == conventionMode.gameObject)
            {
                ToggleConventionMode();
            }

#if ARCADE
            if (freePlay && _menuOptions.selectedMenuOption.gameObject == freePlay.gameObject)
            {
                ToggleFreePlay();
            }
#endif
        }

        if (_controller.GetButtonDown("UICancel"))
        {
            if (_saveOnExit)
            {
                _saveOnExit = false;
                SaveGameManager.instance.Save(false, true);
            }

            UISounds.instance.Cancel();
            if (SaveGameManager.instance && SaveGameManager.instance.saveFileData.conventionMode)
            {
                GoToState(titleScreenState);
            }
            else
            {
                GoToState(previousState);
            }
        }
    }

    public void ToggleForceCrystalMines()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.forceCrystalMine = !slot.forceCrystalMine;
                forceCrystalMines.text = "Force Crystal Mines - " + (slot.forceCrystalMine ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
                if (slot.forceCrystalMine && AchievementManager.instance)
                {
                    AchievementManager.instance.TryEarnAchievement(AchievementID.CrystalMines);
                }
            }
        }
    }

    public void ToggleForcePhaseShell()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.forcePhaseShell = !slot.forcePhaseShell;
                forcePhaseShell.text = "Force Phase Shell - " + (slot.forcePhaseShell ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
                if(slot.forcePhaseShell && AchievementManager.instance)
                {
                    AchievementManager.instance.TryEarnAchievement(AchievementID.PhaseShell);
                }
            }
        }
    }

    public void ToggleConventionMode()
    {
        if(SaveGameManager.instance && SaveGameManager.instance.saveFileData != null)
        {
            SaveGameManager.instance.saveFileData.conventionMode = !SaveGameManager.instance.saveFileData.conventionMode;
            conventionMode.text = "Convention Mode - " + (SaveGameManager.instance.saveFileData.conventionMode ? "On" : "Off");
            _saveOnExit = true;
            UISounds.instance.OptionChange();
        }
    }

    public void ToggleFreePlay()
    {
#if ARCADE
        if (SaveGameManager.instance && SaveGameManager.instance.saveFileData != null)
        {
            SaveGameManager.instance.saveFileData.freePlay = !SaveGameManager.instance.saveFileData.freePlay;
            freePlay.text = "Free Play - " + (SaveGameManager.instance.saveFileData.freePlay ? "On" : "Off");
            _saveOnExit = true;
            UISounds.instance.OptionChange();
        }
#endif
    }
}
