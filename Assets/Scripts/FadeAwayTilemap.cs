using UnityEngine;
using System.Collections;
using CreativeSpore.SuperTilemapEditor;

[RequireComponent(typeof(STETilemap))]
public class FadeAwayTilemap : MonoBehaviour
{
    private STETilemap tilemap;
    private Color _opaque;
    private Color _clear;
    private TilemapTransitionFade _transitionFade;
    private Collider2D _collider2D;

    private void Awake()
    {
        tilemap = GetComponentInParent<STETilemap>();
        _opaque = _clear = tilemap.TintColor;
        _clear.a = 0;
        _transitionFade = GetComponent<TilemapTransitionFade>();
        _collider2D = GetComponent<Collider2D>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        StopAllCoroutines();
        StartCoroutine(FadeToColor(_clear));
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if ((!_transitionFade || !_transitionFade.transitioning) && 
            (!_collider2D || !PlayerManager.instance || !PlayerManager.instance.IntersectsAnyPlayerBounds(_collider2D.bounds)))
        {
            StopAllCoroutines();
            StartCoroutine(FadeToColor(_opaque));
        }
    }

    public IEnumerator FadeToColor(Color targetColor)
    {
        if (_transitionFade && _transitionFade.transitioning)
        {
            while(_transitionFade.transitioning)
            {
                yield return null;
            }
        }

        float timer = 0;
        float time = Mathf.Abs(tilemap.TintColor.a - targetColor.a) * 0.5f;
        var originalColor = tilemap.TintColor;
        while(timer < time)
        {
            timer += Time.deltaTime;
            var progress = timer / time;
            tilemap.TintColor = Color.Lerp(originalColor, targetColor, progress);
            yield return null;
        }

        tilemap.TintColor = targetColor;
    }
}
