using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TitleScreenState : ScreenState
{
    public FileScreenState fileScreenState;
    public GameStartState gameStartState;
    public NewGameScreenState newGameScreenState;
    public SetImageForControl[] buttons;
    public GameObject exitHint;

    private float _backDelay = 0;
    private float _introDelay = 0f;
    private float _introTime = 177f;

    protected override void Start()
    {
        base.Start();
        InputHelper.instance.AssignSystemPlayerAllControllers(null);
    }

    public override void AppearStart()
    {
        foreach (var b in buttons)
        {
            if (b)
            {
                b.SetImage();
            }
        }

        if (exitHint)
        {
            var conventionMode = SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode;
            exitHint.gameObject.SetActive(!conventionMode && Application.platform != RuntimePlatform.Switch);
        }

        base.AppearStart();
    }

    public override void ReadyUpdate()
    {
        if (_backDelay < 0.5f)
        {
            _backDelay += Time.deltaTime;
        }

        var conventionMode = SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode;
        if (conventionMode)
        {
            _introDelay += Time.deltaTime;
            if (_introDelay > _introTime) { SceneManager.LoadScene("Backstory"); }

            if (Input.GetKeyDown(KeyCode.C))
            {
                SaveGameManager.instance.saveFileData.conventionMode = false;
                SaveGameManager.instance.Save();
                GoToState(fileScreenState);
                return;
            }
        }

        if (_controller.GetButtonDown("UICancel") && Application.platform != RuntimePlatform.Switch && !conventionMode)
        {
            if (_backDelay >= 0.5f)
            {
                Debug.Log("Quit!");
                Application.Quit();
            }
            else
            {
                Debug.Log("Too Soon!");
            }
        }
        else if (conventionMode && Input.GetKey(KeyCode.LeftShift))
        {
            GoToState(fileScreenState);
        }
        else if ((_controller.controllers.hasKeyboard && Input.anyKeyDown) || _controller.GetAnyButtonDown())
        {
            UISounds.instance.Confirm();
            if (conventionMode)
            {
                newGameScreenState.previousState = this;
                GoToState(newGameScreenState);
            }
            else
            {
                GoToState(fileScreenState);
            }
        }
    }
}

