using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UISounds : MonoBehaviour
{
    public static UISounds instance;

    public AudioClip blip;
    public AudioClip optionChange;
    public AudioClip confirm;
    public AudioClip cancel;
    public AudioClip purchase;
    public AudioClip beastGutsFade;
    public AudioClip itemCollect;
    public AudioClip uiFail;
    public AudioClip effectEnd;
    public AudioClip screenFlash;

    public AudioSource audioSource;
    public AudioSource audioSourceLowPriority;

    private HashSet<string> _lastLimitedClips = new HashSet<string>();

    void Awake ()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Blip()
    {
        audioSource.PlayOneShot(blip);
    }

    public void OptionChange()
    {
        audioSource.PlayOneShot(optionChange);
    }

    public void Confirm()
    {
        audioSource.PlayOneShot(confirm);
    }

    public void Cancel()
    {
        audioSource.PlayOneShot(cancel);
    }

    public void UIFail()
    {
        audioSource.PlayOneShot(uiFail);
    }

    public void EffectEnd()
    {
        audioSource.PlayOneShot(effectEnd);
    }

    public void Purchase()
    {
        audioSource.PlayOneShot(purchase);
    }

    public void BeastGutsFade()
    {
        audioSource.PlayOneShot(beastGutsFade);
    }

    public void ItemCollect()
    {
        audioSource.PlayOneShot(itemCollect);
    }

    public void ScreenFlash()
    {
        audioSource.PlayOneShot(screenFlash);
    }

    public void PlayOneShot(AudioClip audioClip, float volume = 1)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }

    public void PlayOneShotLowPriority(AudioClip audioClip, float volume = 1)
    {
        audioSourceLowPriority.PlayOneShot(audioClip, volume);
    }

    public void PlayLowPriorityLimited(AudioClip audioClip, float volume = 1, float limitDelay = 0.1f)
    {
        if (!_lastLimitedClips.Contains(audioClip.name))
        {
            audioSourceLowPriority.PlayOneShot(audioClip, volume);
            StartCoroutine(LimitSoundDelay(audioClip, limitDelay));
        }
    }

    public IEnumerator LimitSoundDelay(AudioClip audioClip, float limitDelay)
    {
        _lastLimitedClips.Add(audioClip.name);
        yield return new WaitForSeconds(limitDelay);
        _lastLimitedClips.Remove(audioClip.name);
    }
}
