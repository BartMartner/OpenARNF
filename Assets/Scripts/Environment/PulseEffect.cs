using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public AudioClip thrum;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private AudioSource _audioSource;
    private float timer = 0f;
    private float time = 2f;
    private bool dark = true;
    public Color brightColor = new Color(0.8f, 1, 1);

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        _audioSource = GetComponent<AudioSource>();

        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null && !activeSlot.flashingLights)
        {
            _spriteRenderer.color = Color.Lerp(_originalColor, brightColor, 0.5f);
            enabled = false;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > time)
        {             
            timer -= time;

            if(!dark && _audioSource)
            {
                _audioSource.PlayOneShot(thrum);
            }

            dark = !dark;
        }

        if (dark)
        {
            _spriteRenderer.color = Color.Lerp(_originalColor, brightColor, timer / time);
        }
        else
        {
            _spriteRenderer.color = Color.Lerp(brightColor, _originalColor, timer / time);
        }
    }
}
