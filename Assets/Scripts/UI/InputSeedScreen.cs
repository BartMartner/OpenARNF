using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputSeedScreen : ScreenState
{
    public ScreenState previousState;
    public Text passwordText;
    public Text invalidSeed;
    public string enteredSeed;
    public GridLayoutGroup buttonGrid;
    public SeedScreenCharacter characterPrefab;
    public Button goButton;
    public Toggle raceToggle;
    public Button backspaceButton;
    public GameObject pastSeedsGrid;
    public GameObject secretSeedsGrid;
    public GameObject raceModePopUp;
    private float _flashTimer;
    private bool _flash;
    private bool _buttonsSet;
    private List<Button> _buttons = new List<Button>();
    private List<SeedScreenSaveSeed> _pastSeeds;
    private List<SeedScreenSaveSeed> _secretSeeds;
    private bool _go;
    private float _inputSkip;
    public bool raceMode { get; private set; } = true;

    private void Awake()
    {
        _pastSeeds = new List<SeedScreenSaveSeed>(pastSeedsGrid.GetComponentsInChildren<SeedScreenSaveSeed>());
        _secretSeeds = new List<SeedScreenSaveSeed>(secretSeedsGrid.GetComponentsInChildren<SeedScreenSaveSeed>());
    }

    protected override void Start()
    {
        base.Start();
        if (!_buttonsSet)
        {
            foreach (var c in SeedHelper.baseDefinition)
            {
                var charButton = Instantiate<SeedScreenCharacter>(characterPrefab);
                charButton.transform.SetParent(buttonGrid.transform);
                charButton.transform.localScale = Vector3.one;
                charButton.character = c;
                charButton.text.text = c.ToString();
                charButton.button.onClick.RemoveAllListeners();
                charButton.button.onClick.AddListener(() => AddCharacter(c.ToString()));
                _buttons.Add(charButton.button);
            }

            //Dumb shit to make navigation work
            var newBackspaceButton = Instantiate<Button>(backspaceButton);
            newBackspaceButton.transform.SetParent(buttonGrid.transform);
            newBackspaceButton.transform.localScale = Vector3.one;
            newBackspaceButton.onClick.AddListener(Backspace);
            Destroy(backspaceButton.gameObject);
            backspaceButton = newBackspaceButton;

            goButton.onClick.AddListener(Go);
            raceToggle.onValueChanged.AddListener(RaceMode);
            if(!raceToggle.animator) { Debug.LogError("NO ANIMATOR?!"); }
            _buttonsSet = true;
        }

        var nav = goButton.navigation;
        nav.selectOnUp = backspaceButton;
        nav.selectOnDown = _buttons[0];
        nav.selectOnLeft = raceToggle;
        nav.selectOnRight = raceToggle;
        goButton.navigation = nav;

        nav = raceToggle.navigation;
        nav.selectOnUp = backspaceButton;
        nav.selectOnDown = _buttons[0];
        nav.selectOnLeft = goButton;
        nav.selectOnRight = goButton;
        raceToggle.navigation = nav;
    }

    public void SetPastSeeds()
    {
        var activeSlot = SaveGameManager.activeSlot;
        if(activeSlot == null)
        {
            Debug.LogError("InputSeed Screen is trying to add seeds and there's no active slot");
        }

        for (int i = 0; i < _pastSeeds.Count; i++)
        {
            if(activeSlot == null || i >= activeSlot.pastSeeds.Count)
            {
                _pastSeeds[i].SetSeed(string.Empty);
            }
            else
            {
                _pastSeeds[i].SetSeed(activeSlot.pastSeeds[i]);
            }
        }

        for (int i = 0; i < _secretSeeds.Count; i++)
        {
            if (activeSlot == null || i >= activeSlot.secretSeeds.Count)
            {
                _secretSeeds[i].SetSeed(string.Empty);
            }
            else
            {
                _secretSeeds[i].SetSeed(activeSlot.secretSeeds[i]);
            }
        }
    }

    public override void AppearStart()
    {
        raceMode = false;
        enteredSeed = string.Empty;
        SetPasswordText();
        StartCoroutine(WaitSetPastSeeds());
        base.AppearStart();
    }

    private IEnumerator WaitSetPastSeeds()
    {
        yield return new WaitForEndOfFrame();
        SetPastSeeds();
    }

    public override void AppearFinished()
    {
        base.AppearFinished();
        _buttons[0].Select();
    }

    private void AddCharacter(string c)
    {
        if(SeedHelper.baseDefinition.Contains(c) && enteredSeed.Length < 24)
        {
            UISounds.instance.Confirm();
            enteredSeed += c;

            if (enteredSeed.Length == 24) { goButton.Select(); }
        }
        else
        {
            UISounds.instance.UIFail();
        }
    }

    private void Backspace()
    {
        if (enteredSeed.Length > 0)
        {
            UISounds.instance.Cancel();
            enteredSeed = enteredSeed.Remove(enteredSeed.Length - 1, 1);
        }
        else
        {
            UISounds.instance.UIFail();
        }
    }

    public override void ReadyUpdate()
    {
        if(_inputSkip > 0)
        {
            _inputSkip -= Time.unscaledDeltaTime;
            return;
        }

        if(raceModePopUp.activeInHierarchy)
        {
            if(_controller.GetAnyButtonDown())
            {
                raceToggle.interactable = true;
                raceModePopUp.SetActive(false);
                UISounds.instance.Confirm();
                raceToggle.Select();
                _inputSkip = 0.25f;
            }
            return;
        }

        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(previousState);
        }

        _flashTimer += Time.deltaTime;
        if (_flashTimer > 0.25f)
        {
            _flashTimer = 0;
            _flash = !_flash;
        }

        if (_controller.controllers.hasKeyboard)
        {
            foreach (var pollingInfo in _controller.controllers.Keyboard.PollForAllKeysDown())
            {
                var key = pollingInfo.keyboardKey;
                switch (key)
                {
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                    case KeyCode.RightArrow:
                    case KeyCode.LeftArrow:
                    case KeyCode.Escape:
                    case KeyCode.Return:
                        //DO NOTHING
                        break;
                    case KeyCode.Backspace:
                        Backspace();
                        break;
                    default:
                        var keyString = key.ToString().Replace("Alpha", "");
                        AddCharacter(keyString);                        
                        break;
                }
            }
        }

        SetPasswordText();
    }

    public void SetPasswordText()
    {
        var currentSpace = enteredSeed.Length;
        var padded = enteredSeed.PadRight(24, '_');
        var password = padded.Substring(0, 6) + ' ' + padded.Substring(6, 6) + '\n' +
                       padded.Substring(12, 6) + ' ' + padded.Substring(18, 6);
        currentSpace += (currentSpace / 6);

        if (_flash && currentSpace < 24)
        {
            var colorText = "<color=#00000000>";
            password = password.Insert(currentSpace, colorText);
            password = password.Insert(currentSpace + 1 + colorText.Length, "</color>");
        }

        passwordText.text = password;
    }

    public void RaceMode(bool toggle)
    {
        raceMode = toggle;
        if (raceMode)
        {
            raceToggle.interactable = false;
            raceModePopUp.SetActive(true);
            _inputSkip = 0.25f;
        }
        UISounds.instance.Confirm();
    }

    public void Go()
    {
        switch (enteredSeed)
        {
            case SeedHelper.ClassicBossRush:
                SeedHelper.StartClassicBossRush();
                break;
            case SeedHelper.Spooky:
                SeedHelper.StartSpookyMode();
                break;
            case SeedHelper.MirrorWorld:
                SeedHelper.StartMirrorWorldMode();
                break;
#if TRUECOOP
            case SeedHelper.TrueCoOp:
                SeedHelper.StartTrueCoOpMode();
                break;
#endif
            default:
                if(enteredSeed.Length < 24)
                {
                    StopAllCoroutines();
                    StartCoroutine(InvalidSeed());
                    return;
                }

                var parameters = SeedHelper.KeyToParameters(enteredSeed);
                if (parameters != null)
                {
                    Debug.Log("Seeded Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
                    InputHelper.instance.AssignPlayer1LastActiveController();
                    SaveGameManager.instance.NewGame(parameters.gameMode, raceMode);
                    SaveGameManager.instance.Save();
                    SaveGameManager.activeGame.password = enteredSeed;
                    UISounds.instance.Confirm();
                    SceneManager.LoadScene("NewGame");
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(InvalidSeed());
                }
                break;
        }
    }

    public void ScreenClose()
    {
        UISounds.instance.Cancel();
        GoToState(previousState);
    }

    public IEnumerator InvalidSeed()
    {
        UISounds.instance.UIFail();
        invalidSeed.color = Color.white;
        invalidSeed.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        var timer = 0f;
        while(timer < 1)
        {
            timer += Time.deltaTime;
            invalidSeed.color = Color.Lerp(Color.white, Color.clear, timer);
            yield return null;
        }
        invalidSeed.gameObject.SetActive(false);
    }
}
