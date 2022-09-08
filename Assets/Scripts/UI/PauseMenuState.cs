using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuState : ScreenState
{
    public SettingsScreenState settingsState;

    public void Settings()
    {
        GoToState(settingsState);
    }
}
