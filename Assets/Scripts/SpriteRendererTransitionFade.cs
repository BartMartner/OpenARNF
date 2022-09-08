using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererTransitionFade : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private bool _subscribed;

    public void Awake()
    {
        Debug.LogWarning("SpriteRendererTransitionFade still exists on " + gameObject.name + " in scene " + gameObject.scene.name);
        Destroy(this);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;

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
        StartCoroutine(ColorLerp(_originalColor, Color.black, Constants.transitionFadeTime));
        Unsubscribe();
    }

    public void OnRoomLoaded() { _spriteRenderer.color = Color.clear; }
    public void OnTransitionComplete() { StartCoroutine(ColorLerp(Color.clear, _originalColor, Constants.transitionFadeTime)); }

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

    private IEnumerator ColorLerp(Color start, Color end, float time)
    {
        var timer = 0f;
        while (_spriteRenderer.color != end)
        {
            timer += Time.unscaledDeltaTime;
            _spriteRenderer.color = Color.Lerp(start, end, timer / time);
            yield return null;
        }
        _spriteRenderer.color = end;
    }

    public void FadeOut(float time)
    {
        StartCoroutine(ColorLerp(_originalColor, Color.clear, time));
    }

    public void OnDestroy()
    {
        Unsubscribe();
    }
}

