using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedObject : MonoBehaviour
{
    public float lifespan;
    public bool fadeIn;
    public bool scaleIn;
    public float inTime;
    public bool fadeOut;
    public bool scaleOut;
    public SimpleAnimation outAnim;
    public float outTime;
    private SimpleAnimator _simpleAnimator;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Color _clearColor;
    private Vector3 _originalScale;

    private void Awake()
    {
        _simpleAnimator = GetComponentInChildren<SimpleAnimator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        _clearColor = _originalColor;
        _clearColor.a = 0;
        _originalScale = transform.localScale;
    }

    private IEnumerator Start()
    {
        var timer = 0f;
        while (timer < inTime)
        {
            timer += Time.deltaTime;
            if (fadeIn) { _spriteRenderer.color = Color.Lerp(_clearColor, _originalColor, timer / inTime); }
            if (scaleIn) { transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, timer / inTime); }
            yield return null;
        }

        yield return new WaitForSeconds(lifespan - inTime - outTime);

        if(outAnim && _simpleAnimator)
        {
            _simpleAnimator.simpleAnimation = outAnim;
            _simpleAnimator.randomFrame = false;
            _simpleAnimator.loop = false;
            _simpleAnimator.ResetAndSetFrame();
        }

        timer = 0;
        while (timer < outTime)
        {
            timer += Time.deltaTime;
            if (fadeOut) { _spriteRenderer.color = Color.Lerp(_originalColor, _clearColor, timer / outTime); }
            if (scaleOut) { transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, timer / outTime); }
            yield return null;
        }

        Destroy(gameObject);
    }
}
