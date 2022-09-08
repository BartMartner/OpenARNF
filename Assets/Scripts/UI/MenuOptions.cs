using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Rewired;

public class MenuOptions : MonoBehaviour
{
    public List<Button> menuOptions = new List<Button>();
    protected int _selectedMenuOptionIndex;
    protected Rewired.Player _controller;
    public Rewired.Player controller
    {
        get { return _controller; }
        set { _controller = value; }
    }

    public bool useSystemPlayer;

    public Button selectedMenuOption
    {
        get
        {
            return menuOptions[_selectedMenuOptionIndex];
        }
    }

    public ScreenState parentScreenState;

    public virtual IEnumerator Start()
    {
        if (_controller == null) { _controller = useSystemPlayer ? ReInput.players.SystemPlayer : ReInput.players.GetPlayer(0); }

        yield return null;

        RefreshSelectedOption();
    }

    public virtual void OnEnable()
    {
        if (menuOptions.Count == 0)
        {
            ResetOptions();
        }
        else if(_selectedMenuOptionIndex >= menuOptions.Count  || !menuOptions[_selectedMenuOptionIndex].interactable)
        {
            _selectedMenuOptionIndex = 0;
            RefreshSelectedOption();
        }
    }

    public void ResetOptions()
    {
        menuOptions = new List<Button>(GetComponentsInChildren<Button>());        
        _selectedMenuOptionIndex = 0;
        RefreshSelectedOption();
    }

    public void Update()
    {
        if (parentScreenState && !parentScreenState.ready) return;

        var up = _controller.GetButtonDown("UIUp");
        var down = _controller.GetButtonDown("UIDown");

        if (up || down)
        {
            bool success = false;

            var initialSelection = _selectedMenuOptionIndex;
            bool started = false;

            while (!success && !(started && _selectedMenuOptionIndex == initialSelection))
            {
                started = true;

                if (down)
                {
                    _selectedMenuOptionIndex = (_selectedMenuOptionIndex + 1) % menuOptions.Count;
                }
                else if (up)
                {
                    _selectedMenuOptionIndex = (_selectedMenuOptionIndex - 1);
                    if (_selectedMenuOptionIndex < 0) { _selectedMenuOptionIndex += menuOptions.Count; }
                }
                var option = menuOptions[_selectedMenuOptionIndex];
                success = option.interactable && option.IsActive();
                if (success && _selectedMenuOptionIndex != initialSelection) { UISounds.instance.OptionChange(); }
            }

            RefreshSelectedOption();
        }

        if (_controller.GetButtonDown("UISubmit"))
        {
            var option = menuOptions[_selectedMenuOptionIndex];
            if (option.interactable && option.onClick != null)
            {
                UISounds.instance.Confirm();
                option.onClick.Invoke();
            }
        }
    }

    public void SelectOption(Button button)
    {
        for (int i = 0; i < menuOptions.Count; i++)
        {
            var option = menuOptions[i];
            if(option == button)
            {
                _selectedMenuOptionIndex = i;
                RefreshSelectedOption();
                return;
            }
        }
    }

    public virtual void RefreshSelectedOption()
    {
        bool needRemoveDestroyed = false;
        for (int i = 0; i < menuOptions.Count; i++)
        {
            var option = menuOptions[i];
            if (option)
            {
                var selected = i == _selectedMenuOptionIndex && option.interactable;
                var colors = option.colors;
                colors.normalColor = selected ? Color.white : Color.gray;
                option.colors = colors;
                option.transform.localScale = selected ? Vector3.one * 1.2f : Vector3.one;
            }
            else
            {
                needRemoveDestroyed = true;                
            }
        }

        if (needRemoveDestroyed) { RemoveDestroyedButtons(); }
    }

    public void RemoveDestroyedButtons()
    {
        if (menuOptions.Count > 0)
        {
            var optionsDestroyed = new List<Button>();
            for (int i = 0; i < menuOptions.Count; i++)
            {
                var option = menuOptions[i];
                if (!option) { optionsDestroyed.Add(option); }
            }

            if (optionsDestroyed.Count > 0)
            {
                optionsDestroyed.ForEach((o) => menuOptions.Remove(o));
                _selectedMenuOptionIndex = 0;
                RefreshSelectedOption();
            }
        }
    }

    public void RemoveButton(GameObject go)
    {
        var button = go.GetComponent<Button>();
        if (menuOptions != null && button) { menuOptions.Remove(button); }
    }
}
