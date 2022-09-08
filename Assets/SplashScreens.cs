using Rewired;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashScreens : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Canvas canvas;
    public SwitchVideo switchVideo;
    private Animator _animator;
    private AudioSource _audioSource;
    private Rewired.Player _controller;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator Start()
    {
        _controller = ReInput.players.GetSystemPlayer();

        yield return new WaitForSeconds(0.75f);
#if !UNITY_SWITCH || UNITY_EDITOR
        videoPlayer.gameObject.SetActive(true);
        canvas.enabled = false;
        yield return new WaitForSeconds(1);
        while (videoPlayer.isPlaying && !_controller.GetButtonDown("UISubmit"))
        {
            yield return null;
        }
        videoPlayer.gameObject.SetActive(false);
        canvas.enabled = true;
#else
        switchVideo.quad.gameObject.SetActive(true);
        canvas.enabled = false;
        switchVideo.VideoStart();
        yield return new WaitForSeconds(6.25f);
        switchVideo.VideoStop();
        switchVideo.quad.gameObject.SetActive(false);
        canvas.enabled = true;
        ///_animator.Play("Morningstar");
        //yield return new WaitForSeconds(5);
#endif

#if UNITY_SWITCH
        _animator.Play("Hitcents");
        yield return new WaitForSeconds(5);
        _animator.Play("PremiumEdition");
        yield return new WaitForSeconds(5);
#else
        _animator.Play("Controller");
        yield return new WaitForSeconds(4);
#endif
        SceneManager.LoadScene("Backstory");
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        _audioSource.PlayOneShot(audioClip);
    }
}
