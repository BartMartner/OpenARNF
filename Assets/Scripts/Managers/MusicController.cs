using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Audio;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public static MusicController instance;
    public AudioClip itemJingle;
    public AudioClip achievementJingle;
    public AudioClip caveMusic;
    public AudioClip factoryMusic;
    public AudioClip buriedCityMusic;
    public AudioClip beastGutsMusic;
    public AudioClip bossFightMusic;
    public AudioClip surfaceIntroMusic;
    public AudioClip surfaceMusic;
    public AudioClip specialRoomMusic;
    public AudioClip finalBossMusic;
    public AudioClip secretBossMusic;
    public AudioClip titleScreenMusic;
    public AudioClip ominousDrone;
    public AudioClip glitchMusic;
    public AudioClip forestSlumsMusic;
    public AudioClip coolantSewersMusic;
    public AudioClip blackFleshMusic;
    public AudioClip crystalMinesMusic;
    public bool jinglePlaying;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    private AudioSource _activeAudioSource;
    public float maxMusicVolume
    {
        get
        {
            if(SaveGameManager.instance)
            {
                return SaveGameManager.instance.saveFileData.musicVolume;
            }
            else
            {
                return 0.75f;
            }
        }
    }

    private bool _inIntro;
    public EnvironmentType currentEnvironment;
    public bool isTitleScreen;

    private float _lastActiveTrackTime;
    private bool _mirrorMode;

    public float pitch { get { return audioSource1.pitch; } }

    public void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _activeAudioSource = audioSource1;

        audioSource1.ignoreListenerVolume = true;
        audioSource2.ignoreListenerVolume = true;

        if(isTitleScreen)
        {
            StartCoroutine(PlayTrack(titleScreenMusic));
        }

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            if (activeGame.gameMode == GameMode.Spooky)
            {
                Debug.Log("LOADING CREEPY MODE MIXER!");
                var mixer = Resources.Load<AudioMixer>("SpookyMusic");
                audioSource1.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
                audioSource2.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
            }
        }
    }

    public void PlayItemJingle()
    {
        StartCoroutine(PlayJingle(itemJingle));
    }

    public void PlayAchievementJingle()
    {
        StartCoroutine(PlayJingle(achievementJingle));
    }

    public void StartFadeOut(float time)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutMusic(time));
    }

    public void PlayBossFightMusic()
    {
        _activeAudioSource.Stop();
        _activeAudioSource.clip = bossFightMusic;
        _activeAudioSource.time = 0;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
    }

    public void PlaySecretBossMusic()
    {
        _activeAudioSource.Stop();
        _activeAudioSource.clip = secretBossMusic;
        _activeAudioSource.time = 0;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
    }

    public void PlayFinalBossMusic()
    {
        _activeAudioSource.Stop();
        _activeAudioSource.clip = finalBossMusic;
        _activeAudioSource.time = 0;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
    }

    public void PlayBlackFleshMusic()
    {
        _activeAudioSource.Stop();
        _activeAudioSource.clip = blackFleshMusic;
        _activeAudioSource.time = 0;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
    }

    public void ForestSlumsStart()
    {
        currentEnvironment = EnvironmentType.ForestSlums;
        _activeAudioSource.clip = surfaceIntroMusic;
        _activeAudioSource.time = 0;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
        _inIntro = true;
    }

    public void SurfaceStart()
    {
        currentEnvironment = EnvironmentType.Surface;
        _activeAudioSource.clip = surfaceIntroMusic;
        _activeAudioSource.time = 0;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
        _inIntro = true;
    }

    public void SetMusicFromRoom(RoomAbstract roomAbstract)
    {
        currentEnvironment = roomAbstract.assignedRoomInfo.environmentType;

        if(roomAbstract.assignedRoomInfo.roomType == RoomType.TransitionRoom)
        {
            currentEnvironment = roomAbstract.assignedRoomInfo.transitionsTo;
        }

        if (_inIntro && (currentEnvironment == EnvironmentType.Surface || currentEnvironment == EnvironmentType.ForestSlums))
        {
            _inIntro = false;
            if(currentEnvironment == EnvironmentType.Surface)
            {
                StartCoroutine(CrossFadeToClip(surfaceMusic, 0.2f));
            }
            else
            {
                StartCoroutine(CrossFadeToClip(forestSlumsMusic, 0.2f));
            }
            return;
        }
        else
        {
            AudioClip chosenClip;

            if (roomAbstract.assignedRoomInfo.boss == BossName.GlitchBoss)
            {
                chosenClip = ominousDrone;
                _lastActiveTrackTime = _activeAudioSource.time;
            }
            else if (roomAbstract.assignedRoomInfo.roomType == RoomType.SaveRoom ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.Shop || 
                roomAbstract.assignedRoomInfo.roomType == RoomType.Shrine ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.Teleporter ||
                roomAbstract.majorItem != 0)
            {
                chosenClip = specialRoomMusic;
                _lastActiveTrackTime = _activeAudioSource.time;
            }
            else if (roomAbstract.preBossRoom != BossName.None && SaveGameManager.activeGame != null &&
                !SaveGameManager.activeGame.bossesDefeated.Contains(roomAbstract.preBossRoom))
            {
                chosenClip = ominousDrone;
                _lastActiveTrackTime = _activeAudioSource.time;
            }
            else
            {
                chosenClip = GetEnvironmentMusic(currentEnvironment);
            }

            if (_activeAudioSource.clip != chosenClip)
            {
                StartCoroutine(PlayTrack(chosenClip));
            }
        }
    }

    public void SetEnvironment(EnvironmentType environment)
    {
        if (currentEnvironment != environment || _activeAudioSource.clip == null)
        {
            currentEnvironment = environment;
            PlayEnvironmentMusic();
        }
    }

    public IEnumerator CrossFadeToClip(AudioClip newClip, float time)
    {
        var newActiveAudioSource = _activeAudioSource == audioSource1 ? audioSource2 : audioSource1;
        newActiveAudioSource.volume = 0;
        newActiveAudioSource.clip = newClip;
        newActiveAudioSource.timeSamples = _activeAudioSource.timeSamples;
        newActiveAudioSource.Play();

        if (time > 0)
        {
            var timer = 0f;
            while (timer < time)
            {
                timer += Time.deltaTime;
                var progress = timer / time;
                newActiveAudioSource.volume = Mathf.Lerp(0, maxMusicVolume, progress);
                _activeAudioSource.volume = Mathf.Lerp(maxMusicVolume, 0, progress);
                yield return null;
            }
        }

        _activeAudioSource.volume = 0;
        newActiveAudioSource.volume = maxMusicVolume;
        _activeAudioSource.Stop();
        _activeAudioSource = newActiveAudioSource;
    }

    public void PlayEnvironmentMusic()
    {
        StartCoroutine(PlayTrack(GetEnvironmentMusic(currentEnvironment)));
    }

    public AudioClip GetEnvironmentMusic(EnvironmentType environment)
    {
        switch (currentEnvironment)
        {
            case EnvironmentType.Cave:
                return caveMusic;
            case EnvironmentType.Factory:
                return factoryMusic;
            case EnvironmentType.CoolantSewers:
                return coolantSewersMusic;
            case EnvironmentType.CrystalMines:
                return crystalMinesMusic;
            case EnvironmentType.ForestSlums:
                return (SaveGameManager.activeGame != null && SaveGameManager.activeGame.corruption >= 4) ? titleScreenMusic : forestSlumsMusic;
            case EnvironmentType.Surface:
                return (SaveGameManager.activeGame != null && SaveGameManager.activeGame.corruption >= 4) ? titleScreenMusic : surfaceMusic;
            case EnvironmentType.BuriedCity:
                return buriedCityMusic;
            case EnvironmentType.BeastGuts:
                return beastGutsMusic;
            case EnvironmentType.Glitch:
                return glitchMusic;
            default:
                return null;
        }
    }

    private IEnumerator PlayJingle(AudioClip jingle)
    {
        if(!_activeAudioSource)
        {
            Debug.LogWarning("MusicController has no _activeAudioSource so it can't PlayJingle()");
            yield break;
        }

        jinglePlaying = true;

        float originalTime;
        if (_activeAudioSource.clip)
        {
            originalTime = (_activeAudioSource.time + 0.5f) % _activeAudioSource.clip.length;
        }
        else
        {
            originalTime = 0;
        }

        yield return FadeOutMusic(0.5f);

        _activeAudioSource.Stop();
        _activeAudioSource.loop = false;
        _activeAudioSource.volume = maxMusicVolume;
        _activeAudioSource.PlayOneShot(jingle);
        while (_activeAudioSource.isPlaying)
        {
            yield return null;
        }

        jinglePlaying = false;

        _activeAudioSource.volume = 0;
        _activeAudioSource.loop = true;
        _activeAudioSource.Play();
        _activeAudioSource.time = originalTime;

        yield return FadeInMusic(1f);
    }

    private IEnumerator PlayTrack(AudioClip audio)
    {
        if (_activeAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(1));
        }

        _activeAudioSource.Stop();        

        var time = _activeAudioSource.clip == specialRoomMusic ? _lastActiveTrackTime : 0;

        _activeAudioSource.clip = audio;
        _activeAudioSource.time = time;
        _activeAudioSource.Play();

        yield return StartCoroutine(FadeInMusic(1));
    }

    private IEnumerator FadeInMusic(float time)
    {
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            _activeAudioSource.volume = Mathf.Lerp(0, maxMusicVolume, timer / time);
            yield return null;
        }
        _activeAudioSource.volume = maxMusicVolume;
    }

    private IEnumerator FadeOutMusic(float time)
    {
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            _activeAudioSource.volume = Mathf.Lerp(maxMusicVolume, 0, timer/time);
            yield return null;
        }
        _activeAudioSource.volume = 0;
    }

    public void RefreshVolume()
    {
        _activeAudioSource.volume = maxMusicVolume;
    }

    public void SetPitch(float pitch)
    {
        audioSource1.pitch = pitch;
        audioSource2.pitch = pitch;
    }

    public void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }
}
