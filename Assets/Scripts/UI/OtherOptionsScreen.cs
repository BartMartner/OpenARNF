using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OtherOptionsScreen : ScreenState
{
    public ScreenState previousState;
    public Text aimLook;
    public Text flashingLights;
    public Text blood;
    public Text shotCharging;
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
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                aimLook.text = "Look Controls - " + (slot.lookControls ? "On" : "Off");
                flashingLights.text = "Flashing Lights - " + (slot.flashingLights ? "On" : "Off");
                blood.text = "Blood - " + (slot.blood ? "High" : "Low");
                shotCharging.text = "Shot Charging - " + (slot.shotCharging ? "Enabled" : "Disabled");
            }
        }
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UILeft") || _controller.GetButtonDown("UIRight"))
        {
            if (aimLook && menuOptions.selectedMenuOption.gameObject == aimLook.gameObject)
            {
                ToggleAimLook();
            }
            else if (flashingLights && menuOptions.selectedMenuOption.gameObject == flashingLights.gameObject)
            {
                ToggleFlashingLights();
            }
            else if (blood && menuOptions.selectedMenuOption.gameObject == blood.gameObject)
            {
                ToggleBlood();
            }
            else if (shotCharging && menuOptions.selectedMenuOption.gameObject == shotCharging.gameObject)
            {
                ToggleShotCharging();
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

    public void ToggleFlashingLights()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.flashingLights = !slot.flashingLights;
                flashingLights.text = "Flashing Lights - " + (slot.flashingLights ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }

    public void ToggleBlood()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.blood = !slot.blood;
                blood.text = "Blood - " + (slot.blood ? "High" : "Low");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }

    public void ToggleAimLook()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.lookControls = !slot.lookControls;
                aimLook.text = "Look Controls - " + (slot.lookControls ? "On" : "Off");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }

    public void ToggleShotCharging()
    {
        if (SaveGameManager.instance)
        {
            var slot = SaveGameManager.activeSlot;
            if (slot != null)
            {
                slot.shotCharging = !slot.shotCharging;
                shotCharging.text = "Shot Charging - " + (slot.shotCharging ? "Enabled" : "Disabled");
                _saveOnExit = true;
                UISounds.instance.OptionChange();
            }
        }
    }
}
