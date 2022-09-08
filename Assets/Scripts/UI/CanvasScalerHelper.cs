using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerHelper : MonoBehaviour
{
    public RectTransform currencyTracker;
    public float rightAdjust;
    private CanvasScaler _scaler;
    private float _lastRatio;
    private Vector3 _currenyTrackerOriginalPosition;

    private void Start()
    {
        _scaler = GetComponent<CanvasScaler>();
        if (currencyTracker) _currenyTrackerOriginalPosition = currencyTracker.anchoredPosition;
    }

    private void Update ()
    {
        float ratio = Screen.width / Screen.height;

        if (ratio != _lastRatio)
        {
            if (ratio > 16 / 9)
            {
                _scaler.matchWidthOrHeight = 1;
                if (currencyTracker) currencyTracker.anchoredPosition = _currenyTrackerOriginalPosition + Vector3.right * rightAdjust;
            }
            else
            {
                _scaler.matchWidthOrHeight = 0;
                if (currencyTracker) currencyTracker.anchoredPosition = _currenyTrackerOriginalPosition;
            }
        }

        _lastRatio = ratio;
	}
}
