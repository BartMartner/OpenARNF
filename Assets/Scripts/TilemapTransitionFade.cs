using UnityEngine;
using System.Collections;
using CreativeSpore.SuperTilemapEditor;

[RequireComponent(typeof(STETilemap))]
public class TilemapTransitionFade : MonoBehaviour
{
    private STETilemap _tileMap;
    private Color _originalColor;
    private bool _subscribed;
    private bool _colorLerping;
    private bool _transitionComplete = true;
    public bool transitioning
    {
        get { return !_transitionComplete || _colorLerping; }
    }

    public void Awake()
    {
        Debug.LogWarning("TilemapTransitionFade still exists on " + gameObject.name + " in scene " + gameObject.scene.name);
        Destroy(this);

        _tileMap = GetComponent<STETilemap>();
        _originalColor = _tileMap.TintColor;

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
        StartCoroutine(ColorLerp(_originalColor, Color.black));
        Unsubscribe();
    }

    public void OnRoomLoaded()
    {
        _transitionComplete = false;
        _tileMap.TintColor = Color.clear;
    }

    public void OnTransitionComplete()
    {
        _transitionComplete = true;
        StartCoroutine(ColorLerp(Color.clear, _originalColor));
    }

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

    private IEnumerator ColorLerp(Color start, Color end)
    {
        var timer = 0f;
        _colorLerping = true;
        while (_tileMap.TintColor != end)
        {
            timer += Time.unscaledDeltaTime;
            _tileMap.TintColor = Color.Lerp(start, end, timer / Constants.transitionFadeTime);
            yield return null;
        }
        _tileMap.TintColor = end;
        _colorLerping = false;
    }

    public void OnDestroy()
    {
        Unsubscribe();
    }
}
