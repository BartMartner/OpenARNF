using UnityEngine;
using System.Collections;

public class FX : MonoBehaviour
{
    public float lifespan;
    public bool fadeIn;
    public bool scaleIn;
    public float inTime;
    public bool fadeOut;
    public bool scaleOut;
    public float outTime;
    private SimpleAnimator _simpleAnimator;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Color _clearColor;
    private Vector3 _originalScale;

    private void Awake()
    {
        _simpleAnimator = GetComponent<SimpleAnimator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        _clearColor = _originalColor;
        _clearColor.a = 0;
        _originalScale = transform.localScale;
    }

    public void Spawn(Vector3 origin)
    {    
        if(_simpleAnimator)
        {
            _simpleAnimator.Reset();
        }

        transform.position = origin;
        _spriteRenderer.color = fadeIn ? _clearColor : _originalColor;
        transform.localScale = scaleIn ? Vector3.zero : _originalScale;
        gameObject.SetActive(true);
        StartCoroutine(WaitAndRecycle());
    }

    private IEnumerator WaitAndRecycle()
    {
        var timer = 0f;
        while(timer < inTime)
        {
            timer += Time.deltaTime;
            if (fadeIn) { _spriteRenderer.color = Color.Lerp(_clearColor, _originalColor, timer / inTime); }
            if(scaleIn) { transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, timer / inTime); }
            yield return null;
        }

        yield return new WaitForSeconds(lifespan - inTime - outTime);

        timer = 0;
        while (timer < outTime)
        {
            timer += Time.deltaTime;
            if (fadeOut) { _spriteRenderer.color = Color.Lerp(_originalColor, _clearColor, timer / outTime); }
            if (scaleOut) { transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, timer / outTime); }
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
