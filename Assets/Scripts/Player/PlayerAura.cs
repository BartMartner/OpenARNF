using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAura : MonoBehaviour, IHasTeam
{
    public AudioClip showSound;
    public AudioClip hideSound;
    private AudioSource _audioSource;
    private float _scaleTime = 0.5f;
    private bool _destroying;

    public Team team = Team.Player;
    Team IHasTeam.team
    {
        get { return team; }
        set { team = value; }
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        transform.localScale = Vector3.zero;
    }

    public void Show()
    {
        if (!_destroying)
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(Scale(Vector3.one));
            _audioSource.PlayOneShot(showSound);
        }
    }

    public void Hide()
    {
        if (!_destroying)
        {
            StopAllCoroutines();
            StartCoroutine(Scale(Vector3.zero));
            _audioSource.PlayOneShot(hideSound);
        }
    }

    public void HideDestroy()
    {
        if (gameObject.activeInHierarchy)
        {
            _destroying = true;
            StopAllCoroutines();
            StartCoroutine(Scale(Vector3.zero));
            _audioSource.PlayOneShot(hideSound);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Scale(Vector3 target)
    {
        var timer = 0f;

        var startingScale = transform.localScale;
        var actualTime = _scaleTime * Mathf.Abs(target.x - startingScale.x);

        while(timer < actualTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startingScale, target, timer / actualTime);
            yield return null;
        }

        if(_destroying)
        {
            Destroy(gameObject);
        }
        else if(transform.localScale == Vector3.zero)
        {
            gameObject.SetActive(false);
        }
    }
}
