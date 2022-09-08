using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ChainedSprites : MonoBehaviour
{
    public SpriteRenderer[] sprites;
    public float[] tValues;
    public float[] tangents;
    public Transform start;
    public Transform end;
    public float maxSineMag = 1f;
    public float pingPongInterval = 1f;
    public bool dying;
    public float deathTime = 2f;
    [Tooltip("Controls how long sineMag transitions should take")]
    public float smoothSineTime = 2f;
    public FXType deathFX;
    public bool destroyGameObjectOnEnd = true;
    private float _totalWidth;
    private IEnumerator _sineSmooth;
    private int _lastLength = 0;

	public void Update ()
    {
        if (!dying && start && end && sprites.Length > 0)
        {
            if (_lastLength != sprites.Length) { GetTotalWidth(); }

            //var t = 0f;
            var so = sprites[0].sortingOrder;
            //float tangent;
            float mag = 0;
            Vector3 position;
            var startPosition = start.position;
            var endPosition = end.position;
            var perp = Vector3.Cross((endPosition - startPosition).normalized, Vector3.forward);
            if (maxSineMag != 0)
            {
                mag = -maxSineMag + Mathf.PingPong(Time.time * (maxSineMag / pingPongInterval), maxSineMag * 2);
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];
                position = Vector3.Lerp(startPosition, endPosition, tValues[i]);

                if (maxSineMag != 0)
                {
                    //tangent = Mathf.Sin(t * Mathf.PI * 2);        
                    //position += perp * tangent * mag;
                    position += perp * (tangents[i] * mag);
                }

                sprite.transform.position = position;
                sprite.sortingOrder = so;
                //t += sprite.bounds.size.x / _totalWidth;
                so++;
            }
        }
	}

    private void GetTotalWidth()
    {
        _totalWidth = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i]) _totalWidth += sprites[i].bounds.size.x;
        }

        var t = 0f;
        tValues = new float[sprites.Length];
        tangents = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            tValues[i] = t;
            tangents[i] = Mathf.Sin(t * Mathf.PI * 2);
            t += sprites[i].bounds.size.x / _totalWidth;
        }

        _lastLength = sprites.Length;
    }

    public void DeactivateChain()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i])
            {
                sprites[i].gameObject.SetActive(false);
            }
        }
    }

    public void ActivateChain()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i])
            {
                sprites[i].gameObject.SetActive(true);
            }
        }
    }

    public void SmoothMaxSine(float newMaxSine)
    {
        if(_sineSmooth != null)
        {
            StopCoroutine(_sineSmooth);
        }
        _sineSmooth = LerpMaxSine(newMaxSine);
        StartCoroutine(_sineSmooth);
    }

    public void SmoothMaxSine(float newMaxSine, float time)
    {
        smoothSineTime = time;
        if (_sineSmooth != null)
        {
            StopCoroutine(_sineSmooth);
        }
        _sineSmooth = LerpMaxSine(newMaxSine);
        StartCoroutine(_sineSmooth);
    }

    public IEnumerator LerpMaxSine(float newMaxSine)
    {
        var timer = 0f;
        var origMaxSine = maxSineMag;
        while(timer < smoothSineTime)
        {
            timer += Time.deltaTime;
            maxSineMag = Mathf.Lerp(origMaxSine, newMaxSine, timer);
            yield return null;
        }
        _sineSmooth = null;
    }

    public void DestroyChain(bool fromStart)
    {
        if (fromStart)
        {
            StartCoroutine(DestroyFromStart());
        }
        else
        {
            StartCoroutine(DestroyFromEnd());
        }
    }

    public IEnumerator DestroyFromStart()
    {
        dying = true;
        var interval = deathTime / sprites.Length;
        var wait = new WaitForSeconds(interval);
        for (int i = 0; i < sprites.Length; i++)
        {
            var sprite = sprites[i];
            Destroy(sprite.gameObject);
            if(deathFX != FXType.None)
            {
                FXManager.instance.SpawnFX(deathFX, sprite.transform.position);
            }
            yield return wait;
        }

        if (destroyGameObjectOnEnd)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator DestroyFromEnd()
    {
        dying = true;
        var interval = deathTime / sprites.Length;

        if (end) { Destroy(end.gameObject); }

        var wait = new WaitForSeconds(interval);
        for (int i = sprites.Length-1; i >= 0; i--)
        {
            var sprite = sprites[i];
            if (sprite)
            {
                Destroy(sprite.gameObject);
                if (deathFX != FXType.None)
                {
                    FXManager.instance.SpawnFX(deathFX, sprite.transform.position);
                }
            }
            yield return wait;
        }

        if (destroyGameObjectOnEnd)
        {
            Destroy(gameObject);
        }
    }
}
