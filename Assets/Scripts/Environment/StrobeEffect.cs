using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrobeEffect : MonoBehaviour
{
    public AudioClip thrum;
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private Color _originalColor;

    private void Start()
    {

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _originalColor = _spriteRenderer.color;
        var activeSlot = SaveGameManager.activeSlot;

        if (activeSlot != null && !activeSlot.flashingLights)
        {
            var color = new Color(Random.Range(0.6f, 0.8f), Random.Range(0.8f, 1), Random.Range(0.8f, 1f));
            _spriteRenderer.color = color * 0.75f;
            enabled = false;
        }
        else
        {
            StartCoroutine(Strobe());
        }
    }

    private IEnumerator Strobe()
    {
        while(enabled)
        {
            _audioSource.Play();
            var count = Random.Range(15, 20);
            for (int i = 0; i < count; i++)
            {
                var color = new Color(Random.Range(0.6f, 0.8f), Random.Range(0.8f, 1), Random.Range(0.8f, 1f));
                _spriteRenderer.color = color;
                yield return new WaitForSeconds(Random.Range(0.1f,0.3f));
                _spriteRenderer.color = color * 0.5f;
                yield return new WaitForSeconds(Random.Range(0.01f, 0.075f));
            }
            _spriteRenderer.color = _originalColor;
            _audioSource.PlayOneShot(thrum);
            yield return new WaitForSeconds(Random.Range(0.25f, 3.5f));            
        }
    }
}
