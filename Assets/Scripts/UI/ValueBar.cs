using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public abstract class ValueBar : MonoBehaviour
{
    public Image bar;
    public bool fixedSize;
    public bool smoothChange;
    public Color gainFlash = Color.green;
    public Color lossFlash = Color.red;
    private RectTransform _barRect;
    private RectTransform _rectTransform;
    protected float _maxValue;
    protected float _value;
    protected bool _adjusting;
    public float bottomSize = 18;
    protected float _topSize = 4;
    protected bool _flashing;

    // Use this for initialization
    protected virtual void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _barRect = bar.GetComponent<RectTransform>();
        bar.material = new Material(bar.material); //prevents actual UIFlashMaterial from being modified
        _value = GetValue();
        _maxValue = GetMaxValue();
        MatchValues();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!_adjusting && GetValue() != _value || GetMaxValue() != _maxValue)
        {
            StartCoroutine(ValueChanged());
        }
    }

    public IEnumerator ValueChanged()
    {
        _adjusting = true;
        _maxValue = GetMaxValue();
        MatchValues();

        var getValue = GetValue();

        var valueDelta = Mathf.Abs(getValue - _value);
        var increment = Mathf.Ceil(valueDelta / 3);

        while (_value != getValue)
        {
            getValue = GetValue();

            var newValueDelta = Mathf.Abs(getValue - _value);
            if (newValueDelta > valueDelta)
            {
                valueDelta = newValueDelta;
                increment = Mathf.Ceil(valueDelta / 3);
            }

            if (smoothChange)
            {
                _value = Mathf.MoveTowards(_value, getValue, valueDelta * Time.deltaTime * 2);
                _flashing = true;
                bar.material.SetColor("_FlashColor", _value < getValue ? gainFlash : lossFlash);
                bar.material.SetFloat("_FlashAmount", 0.5f);
                MatchValues();
                yield return null;
            }
            else
            {                
                _value = Mathf.MoveTowards(_value, getValue, increment);
                yield return StartCoroutine(Flash(_value < getValue ? gainFlash : lossFlash));
                MatchValues();
            }
        }

        if (smoothChange)
        {
            _flashing = false;
            bar.material.SetFloat("_FlashAmount", 0);
        }
        _value = getValue;
        MatchValues();

        _adjusting = false;
    }

    public virtual void MatchValues()
    {
        var size = _rectTransform.sizeDelta;
        if (!fixedSize)
        {
            size.y = _maxValue * 2 + bottomSize + _topSize - 1;
        }
        _rectTransform.sizeDelta = size;
        var barSize = size.y - bottomSize - _topSize + 1;
        var inneBarSize =_maxValue > 0 ? barSize * (_value / _maxValue) : 0;
        var innerBarYOffset = (int)(-_topSize - (barSize - inneBarSize));
        _barRect.offsetMax = new Vector2(_barRect.offsetMax.x, innerBarYOffset);
        _barRect.offsetMin = new Vector2(_barRect.offsetMin.x, bottomSize - 1);
    }

    public IEnumerator Flash(Color color)
    {
        _flashing = true;
        bar.material.SetColor("_FlashColor", color);
        bar.material.SetFloat("_FlashAmount", 0.5f);
        yield return new WaitForSeconds(0.05f);

        bar.material.SetFloat("_FlashAmount", 0);
        yield return new WaitForSeconds(0.05f);
        _flashing = false;
    }

    protected virtual float GetValue()
    {
        return 0;
    }

    protected virtual float GetMaxValue()
    {
        return 0;
    }

    public void OnDisable()
    {
        _value = GetValue();
        _maxValue = GetMaxValue();
        bar.material.SetFloat("_FlashAmount", 0);
        MatchValues();
        _adjusting = false;
    }
}
