using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource _audioSource;
    private List<AudioSource> _clipAtPointPlayers = new List<AudioSource>();

    [Header("CommonSounds")]
    public AudioClip tweep03;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0;
            _audioSource.dopplerLevel = 0;
        }
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        _audioSource.PlayOneShot(audioClip);
    }

    public void PlayOneShot(AudioClip audioClip, float volumeScale)
    {
        _audioSource.PlayOneShot(audioClip, volumeScale);
    }

    public void PlayClipAtPoint(AudioClip audioClip, Vector3 position, float volume = 1, float pitch = 1, int priority = 128)
    {
        var source = GetFreeAudioSource();
        source.transform.position = position;
        source.priority = priority;
        source.volume = volume;
        source.pitch = pitch;
        source.PlayOneShot(audioClip);
    }

    public AudioSource GetFreeAudioSource()
    {
        var source = _clipAtPointPlayers.FirstOrDefault((a) => !a.isPlaying);
        if(!source)
        {
            source = new GameObject("AudioSource" + _clipAtPointPlayers.Count).AddComponent<AudioSource>();
            source.transform.parent = transform;
            source.minDistance = 6;
            source.maxDistance = 36;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1;
            source.dopplerLevel = 0.25f;
            _clipAtPointPlayers.Add(source);
        }

        return source;
    }
}
