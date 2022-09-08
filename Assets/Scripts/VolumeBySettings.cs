using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VolumeBySettings : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update ()
    {
        if (SaveGameManager.instance && SaveGameManager.instance.saveFileData != null && _audioSource.volume != SaveGameManager.instance.saveFileData.musicVolume)
        {
            _audioSource.volume = SaveGameManager.instance.saveFileData.musicVolume;
        }
	}
}
