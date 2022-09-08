using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionFade : MonoBehaviour
{
    public Camera gameCamera;
    public static TransitionFade instance;
    public Renderer transitionFadeScreen;
    new public Camera camera { get; private set; }
    private LayerMask _defaultCullingMask;
    private IEnumerator _activeCoroutine;

    public void Awake()
    {
        camera = GetComponent<Camera>();
        camera.backgroundColor = Color.black;
        _defaultCullingMask = camera.cullingMask;
        instance = this;
        Show();
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        FadeIn(0.3f);
    }

    private void LateUpdate()
    {
        if (camera.orthographicSize != gameCamera.orthographicSize)
        {
            camera.orthographicSize = gameCamera.orthographicSize;
        }
    }

    public void FadeIn(float time)
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);

        Show();
        _activeCoroutine = FadeInCoroutine(time, Color.black);
        StartCoroutine(_activeCoroutine);
    }

    public void FadeIn(float time, Color bgColor, bool resetCullingMask = false)
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);

        Show();
        _activeCoroutine = FadeInCoroutine(time, bgColor, resetCullingMask);
        StartCoroutine(_activeCoroutine);
    }

    private IEnumerator FadeInCoroutine(float time, Color bgColor, bool resetCullingMask = false)
    {
        camera.backgroundColor = bgColor;
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;    
            camera.backgroundColor = Color.Lerp(bgColor, Color.clear, timer/time);
            yield return null;
        }

        if(resetCullingMask)
        {
            camera.cullingMask = _defaultCullingMask;
        }

        transitionFadeScreen.gameObject.SetActive(false);
        enabled = false;
    }

    public void ResetCullingMask()
    {
        camera.cullingMask = _defaultCullingMask;
    }

    public void SetCullingMask(LayerMask layerMask)
    {
        camera.cullingMask = layerMask;
    }

    public void FadeOut(float time)
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);

        Show();
        _activeCoroutine = FadeOutCoroutine(time, Color.black);
        StartCoroutine(_activeCoroutine);
    }

    public void FadeOut(float time, Color bgColor)
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);

        Show();
        _activeCoroutine = FadeOutCoroutine(time, bgColor);
        StartCoroutine(_activeCoroutine);
    }

    public IEnumerator FadeOutCoroutine(float time, Color bgColor)
    {
        camera.backgroundColor = Color.clear;
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            camera.backgroundColor = Color.Lerp(Color.clear, bgColor, timer / time);
            yield return null;
        }
        camera.backgroundColor = bgColor;
    }

    private void Show()
    {
        enabled = true;
        transitionFadeScreen.gameObject.SetActive(true);
    }

    public void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }
}
