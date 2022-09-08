using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthNumeric : HealthBar
{
    public Text text;

    public override void MatchValues()
    {
        text.text = Mathf.CeilToInt(Mathf.Clamp(GetValue(), 0, GetMaxValue())).ToString() + "/" + _maxValue;
        bar.rectTransform.sizeDelta = new Vector2(24f * _value / _maxValue, 5);
    }
}
