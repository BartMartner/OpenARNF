using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingController : MonoBehaviour
{
    public RectTransform skipPopUp;
    public AudioClip beastRoar;
    public AudioSource music;

    private bool _finished;
    private bool _ready;
    // Use this for initialization
    protected Rewired.Player _controller;
    private AudioSource _audioSource;

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        _controller = ReInput.players.SystemPlayer;        
        _audioSource = GetComponent<AudioSource>();
        _ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_ready)
        {
            if (skipPopUp.gameObject.activeInHierarchy)
            {
                if(_controller.GetButtonDown("UICancel"))
                {
                    UISounds.instance.Confirm();
                    skipPopUp.gameObject.SetActive(false);
                }
                else if (_controller.GetButtonDown("UISubmit"))
                {
                    Quit();
                }
            }
            else if (_controller.GetAnyButtonDown())
            {
                if (_finished)
                {
                    Quit();
                }
                else
                {
                    UISounds.instance.Confirm();
                    skipPopUp.gameObject.SetActive(true);
                }
            }
        }
    }

    public void CheckForBeastGuts()
    {
        if (!SaveGameManager.beastGutsUnlocked)
        {
            _audioSource.PlayOneShot(beastRoar);
        }
    }

    public void Finished()
    {
        _finished = true;
        StartCoroutine(WaitTitleScreen());
    }

    void Quit()
    {
        UISounds.instance.Confirm();
        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.runCompleted)
        {
            SaveGameManager.instance.ClearActiveGame();
        }
        SceneManager.LoadScene("StartScreen");
    }

    public IEnumerator WaitTitleScreen()
    {
        if (music)
        {
            var timer = 6f;
            var startingVolume = music.volume;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                music.volume = Mathf.Lerp(0, startingVolume, timer / 6f);
                yield return null;
            }
        }

        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.runCompleted)
        {
            SaveGameManager.instance.ClearActiveGame();
        }
        SceneManager.LoadScene("StartScreen");
    }
}
