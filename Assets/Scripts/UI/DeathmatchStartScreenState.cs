using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class DeathmatchStartScreenState : ScreenState
{
    public ScreenState previousState;
    public Text modeText;
    public Text limitText;
    public Text itemMode;
    public Text rouletteItems;
    public Text spawnRoomItems;
    public Text molemenSpawnRate;
    public Text maxMolemen;
    public ScreenState changeMapRotationState;
    public MenuOptions menuOptions;

    private float _lastOptionChangeDelay;
    private float _optionChangeDelay;
    private bool _optionsChanged;
    private Button _maxMolemenButton;

    protected override void Start()
    {
        base.Start();
        Refresh();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Refresh();
    }

    public override void ReadyUpdate()
    {
        if (SaveGameManager.deathmatchSettings == null) return;

        var mode = SaveGameManager.deathmatchSettings.mode;
        var refresh = false;

        if (_optionChangeDelay > 0) { _optionChangeDelay -= Time.deltaTime; }

        var selected = menuOptions.selectedMenuOption.gameObject;

        if (selected == limitText.gameObject)
        {
            int limit;

            if (GetHorizontalButton("UILeft"))
            {
                if (mode == DeathmatchMode.FragLimit)
                {
                    limit = SaveGameManager.deathmatchSettings.fragLimit - 5;
                    if (limit < 5) limit = 50;
                    SaveGameManager.deathmatchSettings.fragLimit = limit;
                }
                else if (mode == DeathmatchMode.TimeLimit)
                {
                    limit = SaveGameManager.deathmatchSettings.timeLimit - 15;
                    if (limit < 60) limit = 1200;
                    SaveGameManager.deathmatchSettings.timeLimit = limit;
                }

                refresh = true;
                UISounds.instance.OptionChange();
            }
            else if (GetHorizontalButton("UIRight"))
            {
                if (mode == DeathmatchMode.FragLimit)
                {
                    limit = SaveGameManager.deathmatchSettings.fragLimit + 5;
                    if (limit > 50) limit = 5;
                    SaveGameManager.deathmatchSettings.fragLimit = limit;
                }
                else if (mode == DeathmatchMode.TimeLimit)
                {
                    limit = SaveGameManager.deathmatchSettings.timeLimit + 15;
                    if (limit > 1200) limit = 60;
                    SaveGameManager.deathmatchSettings.timeLimit = limit;
                }

                refresh = true;
                UISounds.instance.OptionChange();
            }
        }
        else if(selected == itemMode.gameObject)
        {
            var itemMode = SaveGameManager.deathmatchSettings.itemMode;
            if (GetHorizontalButton("UILeft"))
            {
                var modes = Enum.GetValues(typeof(DeathmatchItemMode)).Cast<DeathmatchItemMode>().ToList();
                var index = (modes.IndexOf(itemMode) - 1);
                if (index < 0) { index = modes.Count - index; }
                itemMode = modes[index];

                refresh = true;
                UISounds.instance.OptionChange();
            }
            else if (GetHorizontalButton("UIRight"))
            {
                var modes = Enum.GetValues(typeof(DeathmatchItemMode)).Cast<DeathmatchItemMode>().ToList();
                var index = (modes.IndexOf(itemMode) + 1) % modes.Count;
                itemMode = modes[index];

                refresh = true;
                UISounds.instance.OptionChange();
            }

            if(refresh) { SaveGameManager.deathmatchSettings.itemMode = itemMode; }
        }
        else if(selected == molemenSpawnRate.gameObject)
        {
            var spawnRate = SaveGameManager.deathmatchSettings.molemanSpawnRate;
            if (GetHorizontalButton("UILeft"))
            {
                spawnRate -= 1;
                if(spawnRate < 0) { spawnRate = DeathmatchManager.maxMolemanSpawnRate; }
                refresh = true;
                UISounds.instance.OptionChange();
            }
            else if (GetHorizontalButton("UIRight"))
            {
                spawnRate += 1;
                if(spawnRate > DeathmatchManager.maxMolemanSpawnRate) { spawnRate = 0; }
                refresh = true;
                UISounds.instance.OptionChange();
            }

            if (refresh) { SaveGameManager.deathmatchSettings.molemanSpawnRate = spawnRate; }
        }
        else if (selected == maxMolemen.gameObject)
        {
            var maxMolemen = SaveGameManager.deathmatchSettings.maxMolemen;
            if (GetHorizontalButton("UILeft"))
            {
                maxMolemen -= 1;
                if (maxMolemen < 1) { maxMolemen = DeathmatchManager.maxMolemen; }
                refresh = true;
                UISounds.instance.OptionChange();
            }
            else if (GetHorizontalButton("UIRight"))
            {
                maxMolemen += 1;
                if (maxMolemen > DeathmatchManager.maxMolemen) { maxMolemen = 1; }
                refresh = true;
                UISounds.instance.OptionChange();
            }

            if (refresh) { SaveGameManager.deathmatchSettings.maxMolemen = maxMolemen; }
        }

        if (refresh)
        {
            _optionsChanged = true;
            Refresh();
        }

        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(previousState);
        }
    }

    public bool GetHorizontalButton(string button)
    {
        if (_controller.GetButtonDown(button))
        {
            _optionChangeDelay = 0.5f;
            _lastOptionChangeDelay = 0.5f;
            return true;
        }
        else if (_optionChangeDelay <= 0 && controller.GetButton(button))
        {
            _optionChangeDelay = Mathf.Clamp(_lastOptionChangeDelay * 0.75f, 0.1f, 0.5f);
            _lastOptionChangeDelay = _optionChangeDelay;
            return true;
        }
        else
        {
            return false;
        }
    }


    public void ChangeMode()
    {
        if(SaveGameManager.deathmatchSettings != null)
        {
            var mode = SaveGameManager.deathmatchSettings.mode;
            var modes = Enum.GetValues(typeof(DeathmatchMode)).Cast<DeathmatchMode>().ToList();
            var index = (modes.IndexOf(mode) + 1) % modes.Count;
            SaveGameManager.deathmatchSettings.mode = modes[index];
        }

        _optionsChanged = true;

        Refresh();
    }

    public void ChangeItemMode()
    {
        if (SaveGameManager.deathmatchSettings != null)
        {
            var mode = SaveGameManager.deathmatchSettings.itemMode;
            var modes = Enum.GetValues(typeof(DeathmatchItemMode)).Cast<DeathmatchItemMode>().ToList();
            var index = (modes.IndexOf(mode) + 1) % modes.Count;
            SaveGameManager.deathmatchSettings.itemMode = modes[index];
        }

        _optionsChanged = true;

        Refresh();
    }

    public void ToggleRouletteItems()
    {
        if (SaveGameManager.deathmatchSettings != null)
        {
            SaveGameManager.deathmatchSettings.rouletteItems = !SaveGameManager.deathmatchSettings.rouletteItems;
        }

        _optionsChanged = true;
        Refresh();
    }

    public void ToggleSpawnRoomItems()
    {
        if (SaveGameManager.deathmatchSettings != null)
        {
            SaveGameManager.deathmatchSettings.spawnRoomItems = !SaveGameManager.deathmatchSettings.spawnRoomItems;
        }

        _optionsChanged = true;
        Refresh();
    }

    public void StartDeathmatch()
    {
        SaveGameManager.instance.Save(false, true);
        SceneManager.LoadScene("DeathmatchCore");
    }

    public void Refresh()
    {
        var mode = SaveGameManager.deathmatchSettings.mode;
        modeText.text = "Mode - " + mode.ToParsedString();
        switch(mode)
        {
            case DeathmatchMode.FragLimit:
                limitText.gameObject.SetActive(true);
                limitText.text = "Frag Limit - " + SaveGameManager.deathmatchSettings.fragLimit;
                break;
            case DeathmatchMode.TimeLimit:
                var time = TimeSpan.FromSeconds(SaveGameManager.deathmatchSettings.timeLimit);
                limitText.gameObject.SetActive(true);
                limitText.text = "Time Limit - " + String.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
                break;
        }

        itemMode.text = "Item Spawns - " + SaveGameManager.deathmatchSettings.itemMode.ToParsedString();
        rouletteItems.text = "Item Roulette - " + (SaveGameManager.deathmatchSettings.rouletteItems ? "On" : "Off");
        spawnRoomItems.text = "Spawn Room Items -" + (SaveGameManager.deathmatchSettings.spawnRoomItems ? "On" : "Off");

        var spawnRate = SaveGameManager.deathmatchSettings.molemanSpawnRate;
        molemenSpawnRate.text = "Moleman Spawn Rate - " + spawnRate;

        _maxMolemenButton = maxMolemen.GetComponent<Button>();
        if (spawnRate > 0)
        {
            maxMolemen.text = "Max Molemen - " + (SaveGameManager.deathmatchSettings.maxMolemen);
            _maxMolemenButton.interactable = true;
        }
        else
        {
            maxMolemen.text = "Max Molemen - 0";
            _maxMolemenButton.interactable = false;
        }

        menuOptions.RefreshSelectedOption();
    }

    protected override IEnumerator GoToStateCoroutine(ScreenState newState)
    {
        if (_optionsChanged)
        {
            SaveGameManager.instance.Save(false, true);
        }
        return base.GoToStateCoroutine(newState);
    }

    public void ChangeMapRotation()
    {
        GoToState(changeMapRotationState);
    }
}
