using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBlockGroup : MonoBehaviour
{
    public bool startHidden;
    private bool _visible = true;
    private ToggleBlock[] _toggleBlocks;

    public void Awake()
    {
        _toggleBlocks = GetComponentsInChildren<ToggleBlock>();
    }

    public void Start()
    {
        if (startHidden)
        {
            _visible = false;
            foreach (var b in _toggleBlocks)
            {
                b.HideImmediate();
            }
        }
    }

    public void Toggle()
    {
        if(_visible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Hide()
    {
        _visible = false;
        foreach (var b in _toggleBlocks)
        {
            b.Hide();
        }
    }

    public void Show()
    {
        _visible = true;
        foreach (var b in _toggleBlocks)
        {
            b.Show();
        }
    }
}
