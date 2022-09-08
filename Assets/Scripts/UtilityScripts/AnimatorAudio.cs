using UnityEngine;
using System.Collections;

public class AnimatorAudio : MonoBehaviour
{
    public AudioSource audioSource;

	void Awake ()
    {
        if (!audioSource) { audioSource = GetComponent<AudioSource>(); }
	}

    public void PlayOneShotAtVolume(AudioClip audioClip, float volume)
    {
#if DEBUG
        if (!audioClip) { Debug.LogError(gameObject.name + "'s animator tried to play a null audioClip.");  return; }
#endif
        if (audioSource)
        {
            audioSource.PlayOneShot(audioClip, volume);
        }
        else
        {
            AudioManager.instance.PlayClipAtPoint(audioClip, transform.position, volume);
        }
    }

    public void PlayOneShot(AudioClip audioClip)
    {
#if DEBUG        
        if (!audioClip) { Debug.LogError(gameObject.name + "'s animator tried to play a null audioClip."); return; }
#endif
        if (audioSource)
        {
            audioSource.PlayOneShot(audioClip, 1);
        }
        else
        {
            AudioManager.instance.PlayClipAtPoint(audioClip, transform.position);
        }        
    }

    public void PlayOneShotHalfVolume(AudioClip audioClip)
    {
#if DEBUG
        if (!audioClip) { Debug.LogError(gameObject.name + "'s animator tried to play a null audioClip."); return; }
#endif
        if (audioSource)
        {
            audioSource.PlayOneShot(audioClip, 0.5f);
        }
        else
        {
            AudioManager.instance.PlayClipAtPoint(audioClip, transform.position, 0.5f);
        }
    }

    public void PlayOneShotQuarterVolume(AudioClip audioClip)
    {
#if DEBUG
        if (!audioClip) { Debug.LogError(gameObject.name + "'s animator tried to play a null audioClip."); return; }
#endif
        if (audioSource)
        {
            audioSource.PlayOneShot(audioClip, 0.25f);
        }
        else
        {
            AudioManager.instance.PlayClipAtPoint(audioClip, transform.position, 0.25f);
        }
    }
}
