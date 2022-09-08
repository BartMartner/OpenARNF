using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class MaskedMenuOptions : MenuOptions
{
    public RectTransform content;
    public int moveAfterOptionIndex = 4;
    private float _spacing;

    public void Awake()
    {
        var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
        _spacing = verticalLayout.spacing;
    }

    public override void RefreshSelectedOption()
    {
        base.RefreshSelectedOption();

        float contentY = 0f;
        for (int i = 0; i < _selectedMenuOptionIndex-moveAfterOptionIndex; i++)
        {
            var rect = menuOptions[i].GetComponent<RectTransform>();
            contentY += rect.sizeDelta.y + _spacing;
        }
        content.anchoredPosition = new Vector2(0, contentY);
    }
}
