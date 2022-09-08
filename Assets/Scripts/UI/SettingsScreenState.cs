using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreenState : ScreenState
{
    public ScreenState previousState;
    public ScreenState setControlsScreenState;
    public DisplaySettingsScreenState displaySettings;
    public OtherOptionsScreen otherOptions;
    public Text soundVolume;
    public Text musicVolume;
    public MenuOptions menuOptions;

    private bool _saveOnExit;

    public void Awake()
    {
        menuOptions.parentScreenState = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SetVolumeText();
    }

    public void SetControls()
    {
        GoToState(setControlsScreenState);
    }

    public void Display()
    {
        GoToState(displaySettings);
    }

    public void OtherOptions()
    {
        GoToState(otherOptions);
    }

    public override void ReadyUpdate()
    {
        if (SaveGameManager.instance)
        {
            var xAxis = _controller.GetButtonDown("UILeft") ? -1 : _controller.GetButtonDown("UIRight") ? 1 : 0;
            if (xAxis != 0)
            {
                if (menuOptions.selectedMenuOption.gameObject == soundVolume.gameObject)
                {
                    var currentVolume = SaveGameManager.instance.saveFileData.soundVolume;
                    currentVolume += xAxis > 0 ? 0.1f : -0.1f;
                    SaveGameManager.instance.saveFileData.soundVolume = Mathf.Clamp01(currentVolume);
                    _saveOnExit = true;
                    AudioListener.volume = SaveGameManager.instance.saveFileData.soundVolume;
                    SetVolumeText();
                    UISounds.instance.OptionChange();
                }
                else if (menuOptions.selectedMenuOption.gameObject == musicVolume.gameObject)
                {
                    var currentVolume = SaveGameManager.instance.saveFileData.musicVolume;
                    currentVolume += xAxis > 0 ? 0.1f : -0.1f;
                    SaveGameManager.instance.saveFileData.musicVolume = Mathf.Clamp01(currentVolume);
                    _saveOnExit = true;
                    if (MusicController.instance)
                    {
                        MusicController.instance.RefreshVolume();
                    }
                    SetVolumeText();
                    UISounds.instance.OptionChange();
                }
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

    public void SetVolumeText()
    {
        if (SaveGameManager.instance)
        {
            soundVolume.text = "Sound Volume: " + Mathf.RoundToInt(SaveGameManager.instance.saveFileData.soundVolume * 10);
            musicVolume.text = "Music Volume: " + Mathf.RoundToInt(SaveGameManager.instance.saveFileData.musicVolume * 10);
        }
    }
}
