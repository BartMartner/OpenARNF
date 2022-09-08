using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteRendererGroupTransitionFade : MonoBehaviour
{
    private SpriteRenderer[] _spriteRenderers;
    private List<Color> _originalColors;
    private List<float> _flashAmount;
    private bool _subscribed;

    public void Awake()
    {
        Debug.LogWarning("SpriteRendererGroupTransitionFade still exists on " + gameObject.name + " in scene " + gameObject.scene.name);
        Destroy(this);

        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        _originalColors = new List<Color>();
        _flashAmount = new List<float>();

        foreach (var r in _spriteRenderers)
        {
            _originalColors.Add(r.color);
            _flashAmount.Add(r.material.HasProperty("_FlashAmount") ? r.material.GetFloat("_FlashAmount") : -1);
        }

        /*
        if (LayoutManager.instance)
        {
            _subscribed = true;
            LayoutManager.instance.onRoomExited += OnRoomExit;
            LayoutManager.instance.onRoomLoaded += OnRoomLoaded;
            LayoutManager.instance.onTransitionComplete += OnTransitionComplete;
        }
        */
    }

    public void OnRoomExit()
    {
        StartCoroutine(ColorLerp(true, Color.black, Constants.transitionFadeTime));
        Unsubscribe();
    }

    public void OnRoomLoaded()
    {
        foreach (var r in _spriteRenderers)
        {
            r.color = Color.clear;
            if (r.material.HasProperty("_FlashAmount"))
            {
                r.material.SetFloat("_FlashAmount", 0);
            }
        }
    }

    public void OnTransitionComplete() { StartCoroutine(ColorLerp(false, Color.clear, Constants.transitionFadeTime)); }

    private void Unsubscribe()
    {
        if (_subscribed && LayoutManager.instance)
        {
            _subscribed = false;
            LayoutManager.instance.onRoomExited -= OnRoomExit;
            LayoutManager.instance.onRoomLoaded -= OnRoomLoaded;
            LayoutManager.instance.onTransitionComplete -= OnTransitionComplete;
        }
    }

    private IEnumerator ColorLerp(bool fromOriginal, Color otherColor, float time)
    {
        var timer = 0f;

        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var r = _spriteRenderers[i];
                if (fromOriginal)
                {
                    r.color = Color.Lerp(_originalColors[i], otherColor, timer / time);
                    if (_flashAmount[i] != -1)
                    {
                        r.material.SetFloat("_FlashAmount", Mathf.Lerp(_flashAmount[i], 0, timer/time));
                    }
                }
                else
                {
                    r.color = Color.Lerp(otherColor, _originalColors[i], timer / time);
                    if (_flashAmount[i] != -1)
                    {
                        r.material.SetFloat("_FlashAmount", Mathf.Lerp(0, _flashAmount[i], timer / time));
                    }
                }
            }
            yield return null;
        }

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            var r = _spriteRenderers[i];

            if (_flashAmount[i] != -1)
            {
                r.material.SetFloat("_FlashAmount", fromOriginal ? 0 : _flashAmount[i]);
            }

            if (fromOriginal)
            {
                r.color = otherColor;
            }
            else
            {
                r.color = _originalColors[i];
            }
        }
    }

    public void OnDestroy()
    {
        Unsubscribe();
    }
}


