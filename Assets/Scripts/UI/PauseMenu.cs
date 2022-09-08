using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;
    public GameObject menuScreen;
    public ScreenState[] otherStates;
    public GameObject restartConfirmPopup;
    public System.Action onHide;
    public bool visible { get { return _visible; } }
    private bool _visible;
    private bool _skipUpdate;
    private Rewired.Player _controller;
    private Player _player;

    private float _conventionTimer;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        _player = PlayerManager.instance ? PlayerManager.instance.player1 : null;
        if (_player)
        {
            _controller = _player.controller;
        }
        else
        {
            _controller = ReInput.players.SystemPlayer;
        }
    }

    public void Update()
    {
        if (_skipUpdate) return;

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            UISounds.instance.Confirm();
            StartCoroutine(RestartShortcut());
        }

        if (CanPause() && (PausePressed() || Input.GetKeyDown(KeyCode.Escape)))
        {
            Show();
            return;
        }

        if (menuScreen.activeInHierarchy && _controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            Hide();
        }

        if(SaveGameManager.instance && SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            if (ReInput.controllers.GetAnyButton()) { _conventionTimer = 0; }
            _conventionTimer += Time.deltaTime;
            if (_conventionTimer > 120) { ExitGame(); }
        }
    }

    public bool PausePressed()
    {
        if(DeathmatchManager.instance)
        {
            foreach (var player in DeathmatchManager.instance.players)
            {
                if(player.controller.GetButtonDown("Pause"))
                {
                    _controller = player.controller;
                    var menuState = menuScreen.GetComponent<ScreenState>();
                    menuState.controller = _controller;
                    foreach (var state in otherStates)
                    {
                        state.controller = _controller;
                    }
                    return true;
                }
            }

            return false;
        }
        else
        {
            return _controller.GetButtonDown("Pause");
        }
    }

    public void Show()
    {
        _visible = true;
        menuScreen.SetActive(true);
        Time.timeScale = 0;

        foreach (var state in otherStates)
        {
            state.gameObject.SetActive(false);
        }   
    }

    public void Hide()
    {
        _visible = false;
        menuScreen.SetActive(false);
        Time.timeScale = 1;

        foreach (var state in otherStates)
        {
            state.gameObject.SetActive(false);
        }

        if(onHide != null)
        {
            onHide();
        }
    }

    public void ExitGame()
    {
        Time.timeScale = 1;

        if(_player && _player.state != DamageableState.Alive)
        {
            SaveGameManager.instance.ClearActiveGame();
        }

        SaveGameManager.instance.Save(true, true);

        SceneManager.LoadScene("StartScreen");
    }

    public void RestartConfirm()
    {
        StartCoroutine(WaitForRestartConfirm());
    }

    public IEnumerator WaitForRestartConfirm()
    {
        _skipUpdate = true;
        menuScreen.gameObject.SetActive(false);
        restartConfirmPopup.gameObject.SetActive(true);

        yield return null;

        bool _finished = false;
        while (!_finished)
        {
            if (_controller.GetButtonDown("UISubmit"))
            {
                _finished = true;
                UISounds.instance.Confirm();
                Restart();
            }
            else if (_controller.GetButtonDown("UICancel"))
            {
                UISounds.instance.Cancel();
                _finished = true;
                _skipUpdate = false;
                menuScreen.gameObject.SetActive(true);
                restartConfirmPopup.gameObject.SetActive(false);
            }
            yield return false;
        }
    }

    public IEnumerator RestartShortcut()
    {
        _visible = true; //a trick to make some stuff not function
        Time.timeScale = 0;
        _skipUpdate = true;
        yield return null;
        Restart();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        Debug.Log("New Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        var activeGame = SaveGameManager.activeGame;
        var enteredSeed = activeGame.password;
        SaveGameManager.instance.NewGame(activeGame.gameMode, activeGame.raceMode); //this sets layout to null
        if (!string.IsNullOrEmpty(enteredSeed)) { SaveGameManager.activeGame.password = enteredSeed; }
        SaveGameManager.instance.Save(false, true);
        SceneManager.LoadScene("NewGame");
    }

#if !DEBUG
    private void OnApplicationFocus(bool focus)
    {
        if(!focus && CanPause())
        {
            Show();
        }
    }
#endif

    public bool CanPause()
    {
        if (menuScreen.activeInHierarchy) return false;
        if (Automap.instance && Automap.instance.gridSelectMode) return false;

        return (DeathmatchManager.instance || !_player || _player.state == DamageableState.Alive) &&
           (!LayoutManager.instance || !LayoutManager.instance.transitioning) &&
           (!DeathScreen.instance || !DeathScreen.instance.visible) &&
           (!ItemCollectScreen.instance || !ItemCollectScreen.instance.visible) &&
           (!NPCDialogueManager.instance || !NPCDialogueManager.instance.dialogueActive) &&
           (!DeathmatchManager.instance || DeathmatchManager.instance.allowPause) &&
           !otherStates.Any((s) => s.visible);
    }

    public void DeathmatchDrop()
    {
        if(DeathmatchManager.instance)
        {
            DeathmatchManager.instance.DropPlayer(_controller.id);
        }

        Hide();
    }

    public void OnDestroy()
    {
        if (instance = this)
        {
            instance = null;
        }
    }
}
