using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackstoryScrawl : MonoBehaviour
{
    private bool _ready;
    // Use this for initialization

    protected Rewired.Player _controller;

    public IEnumerator Start()
    {
        _controller = ReInput.players.SystemPlayer;
        yield return new WaitForSeconds(0.25f);
        _ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_ready && _controller.GetAnyButtonDown())
        {
            UISounds.instance.Confirm();
            SceneManager.LoadScene("StartScreen");
        }
    }

    public void StartScreen()
    {
        SceneManager.LoadScene("StartScreen");
    }
}
