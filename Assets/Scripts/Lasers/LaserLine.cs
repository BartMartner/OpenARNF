using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LaserLine : MonoBehaviour
{
    public LaserStats laserStats;
    public bool trigger;
    public bool invert;
    public float laserTime = 0.25f;
    public float laserDuration = 0.5f;
    public float angle;
    public float distance = 12;
    public float spacing = 1;
    public float appearTime = 1;
    public float shootDelay = 1;
    public SpriteRenderer startSprite;
    public SpriteRenderer middleSprite;
    public SpriteRenderer endSprite;
    public AudioClip laserFireSound;
    public AudioClip appearSound;
    public AudioClip disappearSound;

    private AudioSource _audioSource;
    private SpriteRenderer _light;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        startSprite.enabled = false;
        middleSprite.enabled = false;
        endSprite.enabled = false;

        var lt = middleSprite.transform.Find("Light");
        if (lt)
        {
            _light = lt.GetComponent<SpriteRenderer>();
            if (_light) { _light.enabled = false; }
        }
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (trigger)
        {
            trigger = false;
            Activate();
        }
    }
#endif

    public void Activate() { StartCoroutine(FireLaserLine()); }

    public IEnumerator FireLaserLine()
    {
        startSprite.enabled = true;
        middleSprite.enabled = true;
        endSprite.enabled = true;

        if (_light) { _light.enabled = true; }
        if (appearSound) { _audioSource.PlayOneShot(appearSound); }

        var timer = 0f;
        while (timer < appearTime)
        {
            timer += Time.deltaTime;
            SetLine(angle, Mathf.Lerp(0, distance, timer / appearTime));
            yield return null;
        }

        yield return new WaitForSeconds(shootDelay);

        var directon = Quaternion.Euler(0, 0, angle + 90) * Vector3.right;
        var sign = invert ? -1 : 1;
        var start = transform.position + directon * distance * 0.5f * sign;
        var end = transform.position + directon * distance * 0.5f * -sign;
        var count = distance / spacing;

        for (int i = 0; i < count; i++)
        {
            if (laserFireSound) { _audioSource.PlayOneShot(laserFireSound); }
            var origin = Vector3.Lerp(start, end, i / (float)count) + directon * spacing * 0.5f * -sign;
            LaserManager.instance.FireLaser(laserStats, origin,angle,laserTime);
            yield return new WaitForSeconds(laserTime);
        }

        yield return new WaitForSeconds(laserDuration + laserStats.stopTime);

        if (disappearSound) { _audioSource.PlayOneShot(disappearSound); }

        yield return StartCoroutine(Disappear());
    }

    public IEnumerator Disappear()
    {
        var timer = 0f;
        while (timer < appearTime)
        {
            timer += Time.deltaTime;
            SetLine(angle, Mathf.Lerp(distance, 0, timer / appearTime));
            yield return null;
        }

        startSprite.enabled = false;
        middleSprite.enabled = false;
        endSprite.enabled = false;

        if (_light) { _light.enabled = false; }
    }

    public void SetLine(float sAngle, float sDistance)
    {
        var spriteAngle = Quaternion.Euler(0, 0, sAngle);
        var directon = Quaternion.Euler(0, 0, sAngle + 90) * Vector3.right;
        var start = transform.position + directon * sDistance * 0.5f;
        var end = transform.position - directon * sDistance * 0.5f;

        startSprite.transform.position = start;
        startSprite.transform.rotation = spriteAngle;
        var halfStart = startSprite.sprite.bounds.extents.y;

        endSprite.transform.position = end;
        endSprite.transform.rotation = spriteAngle;
        var halfEnd = endSprite.sprite.bounds.extents.y;

        middleSprite.transform.position = transform.position;
        middleSprite.transform.rotation = spriteAngle;
        var spriteSize = middleSprite.sprite.bounds.size.y;
        var targetSize = sDistance - halfStart - halfEnd;
        var tiling = targetSize / spriteSize;
        middleSprite.transform.localScale = new Vector3(1, targetSize, 1);
        if (Application.isPlaying) { middleSprite.material.SetFloat("_RepeatY", tiling); }
    }

    public void Stop()
    {
        StopAllCoroutines();
        if (startSprite.enabled)
        {
            StartCoroutine(Disappear());
        }
    }

    public void OnDrawGizmosSelected()
    {
        var directon = Quaternion.Euler(0, 0, angle + 90) * Vector3.right;
        var sign = invert ? -1 : 1;
        var start = transform.position + directon * distance * 0.5f * sign;
        var end = transform.position + directon * distance * 0.5f * -sign;

        if (Application.isPlaying)
        {
            Debug.DrawLine(start, end);
        }
        else
        {
            SetLine(angle, distance);
        }

        var count = distance / spacing;
        var laserDirection = Vector3.Cross(directon, Vector3.forward);
        for (int i = 0; i < count; i++)
        {
            var ratio = i / (float)count;
            var laserStart = Vector3.Lerp(start, end, ratio) + directon * spacing * 0.5f * -sign;
            var laserEnd = laserStart + laserDirection * laserStats.range;
            Debug.DrawLine(laserStart, laserEnd, Color.Lerp(Color.green, Color.red, ratio));
        }
    }
}
