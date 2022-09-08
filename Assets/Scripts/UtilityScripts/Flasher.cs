using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flasher : MonoBehaviour
{
    protected bool _flashing;
    public bool flashing { get { return _flashing; } }
    public SpriteRenderer[] _renderers;
    private Color _defaultFlashColor = Color.white;
    private float _defaultFlashAmount;
    

    private void Start()
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    private void SetFlashColor(Color color, float amount)
    {
        if (_renderers == null)
        {
            Debug.LogWarning(gameObject.name + " Flasher " + gameObject.name + " has null _renderers Array");
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            if (renderer != null)
            {
                renderer.material.SetColor("_FlashColor", color);
                renderer.material.SetFloat("_FlashAmount", amount);
            }
        }
    }

    public void SetDefaultFlashColor(Color color, float amount)
    {
        if (_renderers == null)
        {
            Debug.LogWarning(gameObject.name + " Flasher " + gameObject.name + " has null _renderers Array");
            return;
        }

        _defaultFlashColor = color;
        _defaultFlashAmount = amount;

        if (!flashing)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                if (renderer != null)
                {
                    renderer.material.SetColor("_FlashColor", color);
                    renderer.material.SetFloat("_FlashAmount", amount);
                }
            }
        }
    }

    public void StartFlash(int flashes, float time, Color color, float amount, bool fade)
    {
        if (fade)
        {
            StartCoroutine(FadeFlash(flashes, time, color, amount));
        }
        else
        {
            StartCoroutine(Flash(flashes, time, color, amount));
        }
    }

    public IEnumerator FadeColor(float time, Color color, float amount, bool to)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;

        var timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            SetFlashColor(color, to ? Mathf.Lerp(0, amount, timer / time) : Mathf.Lerp(amount, 0, timer / time));
            yield return null;
        }
    }

    protected virtual IEnumerator FadeFlash(int flashes, float time, Color color, float amount)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;
        var flashCounter = 0;

        while (flashCounter < flashes)
        {
            flashCounter++;
            var timer = 0f;
            while (timer < time)
            {
                timer += Time.deltaTime;
                SetFlashColor(color, Mathf.Lerp(amount, 0, timer / time));
                yield return null;
            }
        }

        StopFlash();
    }

    protected virtual IEnumerator Flash(int flashes, float time, Color color, float amount)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;
        var flashCounter = 0;

        while (flashCounter < flashes)
        {
            flashCounter++;
            SetFlashColor(color, amount);
            yield return new WaitForSeconds(time * 0.5f);
            SetFlashColor(_defaultFlashColor, _defaultFlashAmount);
            yield return new WaitForSeconds(time * 0.5f);
        }

        StopFlash();
    }

    public virtual void StartMultiColorFlash(float timePerFlash, Color32[] colors, float amount, bool fade)
    {
        StartCoroutine(MultiColorFlash(timePerFlash, colors, amount, fade));
    }

    protected virtual IEnumerator MultiColorFlash(float timePerFlash, Color32[] colors, float amount, bool fade)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;

        foreach (var color in colors)
        {
            if (fade)
            {
                yield return StartCoroutine(Flash(1, timePerFlash, color, amount));
            }
            else
            {
                yield return StartCoroutine(FadeFlash(1, timePerFlash, color, amount));
            }
        }

        StopFlash();
    }

    public void StopFlash()
    {
        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_FlashColor", _defaultFlashColor);
                renderer.material.SetFloat("_FlashAmount", _defaultFlashAmount);
            }
        }
        _flashing = false;
    }
}
