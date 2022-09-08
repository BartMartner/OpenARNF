using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySettingsScreenState : ScreenState
{
    public ScreenState previousState;
    public Text alwaysShowTime;
    public Text alwaysShowScrap;
    public Text alwaysShowStats;
    public Text fullScreen;
    public Text vSync;
    public Text pixelPerfect;
    public Text healthAndEnergy;
    public MenuOptions menuOptions;
    private bool _saveOnExit;

    public void Awake()
    {
        menuOptions.parentScreenState = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (SaveGameManager.instance)
        {
            fullScreen.text = "Fullscreen - " + (SaveGameManager.instance.saveFileData.fullScreen ? "On" : "Off");
            vSync.text = "VSync - " + (SaveGameManager.instance.saveFileData.vSync ? "On" : "Off");
            pixelPerfect.text = "Pixel Perfect - " + (SaveGameManager.instance.saveFileData.pixelPerfect ? "On" : "Off");
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                alwaysShowScrap.text = "Always Show Scrap - " + (slot.scrapAlwaysVisible ? "On" : "Off");
                alwaysShowTime.text = "Always Show Time - " + (slot.timeAlwaysVisible ? "On" : "Off");
                alwaysShowStats.text = "Always Show Stats - " + (slot.statsAlwaysVisible ? "On" : "Off");
                healthAndEnergy.text = "Health and Energy - " + (slot.numericHealthAndEnergy ? "Numeric" : "Bars");
            }
        }
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UILeft") || _controller.GetButtonDown("UIRight"))
        {
            if (fullScreen && menuOptions.selectedMenuOption.gameObject == fullScreen.gameObject)
            {
                ToggleFullScreen();
            }
            else if (pixelPerfect && menuOptions.selectedMenuOption.gameObject == pixelPerfect.gameObject)
            {
                TogglePixelPerfect();
            }
            else if (vSync && menuOptions.selectedMenuOption.gameObject == vSync.gameObject)
            {
                ToggleVsync();
            }
            else if (alwaysShowTime && menuOptions.selectedMenuOption.gameObject == alwaysShowTime.gameObject)
            {
                ToggleTime();
            }
            else if (alwaysShowScrap && menuOptions.selectedMenuOption.gameObject == alwaysShowScrap.gameObject)
            {
                ToggleScrap();
            }
            else if (alwaysShowStats && menuOptions.selectedMenuOption.gameObject == alwaysShowStats.gameObject)
            {
                ToggleStats();
            }
        }

        if (_controller.GetButtonDown("UICancel"))
        {
            if (_saveOnExit)
            {
                _saveOnExit = false;
                SaveGameManager.instance.Save(false, true);
            }

            UISounds.instance.Cancel();
            GoToState(previousState);
        }
    }

    public void ToggleFullScreen()
    {
        if (SaveGameManager.instance)
        {
            var saveFileData = SaveGameManager.instance.saveFileData;
            if (!saveFileData.fullScreen)
            {
                saveFileData.resolution = new Int2D(Screen.width, Screen.height);
            }

            saveFileData.fullScreen = !saveFileData.fullScreen;

            fullScreen.text = "Fullscreen - " + (SaveGameManager.instance.saveFileData.fullScreen ? "On" : "Off");
            UISounds.instance.OptionChange();

            var currentResolution = Screen.currentResolution;
            if (saveFileData.fullScreen)
            {
                Screen.SetResolution(currentResolution.width, currentResolution.height, true);
#if !UNITY_EDITOR
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
#endif
            }
            else
            {
                Screen.SetResolution(saveFileData.resolution.x, saveFileData.resolution.y, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            _saveOnExit = true;
        }
    }

    public void TogglePixelPerfect()
    {
        if (SaveGameManager.instance)
        {
            var saveFileData = SaveGameManager.instance.saveFileData;

            saveFileData.pixelPerfect = !saveFileData.pixelPerfect;

            pixelPerfect.text = "Pixel Perfect - " + (SaveGameManager.instance.saveFileData.pixelPerfect ? "On" : "Off");
            UISounds.instance.OptionChange();

            if (MainCamera.instance)
            {
                MainCamera.instance.SetPixelPerfect();   
            }

            _saveOnExit = true;
        }
    }

    public void ToggleVsync()
    {
        if (SaveGameManager.instance)
        {
            var saveFileData = SaveGameManager.instance.saveFileData;

            saveFileData.vSync = !saveFileData.vSync;

            vSync.text = "VSync - " + (SaveGameManager.instance.saveFileData.vSync ? "On" : "Off");
            UISounds.instance.OptionChange();

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

            _saveOnExit = true;
        }
    }

    public void ToggleTime()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.timeAlwaysVisible = !slot.timeAlwaysVisible;
                alwaysShowTime.text = "Always Show Time - " + (slot.timeAlwaysVisible ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }

    public void ToggleScrap()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.scrapAlwaysVisible = !slot.scrapAlwaysVisible;
                alwaysShowScrap.text = "Always Show Scrap - " + (slot.scrapAlwaysVisible ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }

    public void ToggleStats()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.statsAlwaysVisible = !slot.statsAlwaysVisible;
                alwaysShowStats.text = "Always Show Stats - " + (slot.statsAlwaysVisible ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }

    public void ToggleHealthAndEnergy()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.numericHealthAndEnergy = !slot.numericHealthAndEnergy;
                healthAndEnergy.text = "Health and Energy - " + (slot.numericHealthAndEnergy ? "Numeric" : "Bars");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }
}
