using UnityEngine;
using System.Collections;
using Rewired;

public class ScreenState : MonoBehaviour
{    
    private CanvasGroup _canvasGroup;
    public bool ready;
    public bool useSystemPlayer;

    private bool _visible;
    public bool visible { get { return _visible; } }

    protected Rewired.Player _controller;
    public Rewired.Player controller
    {
        get { return _controller; }
        set
        {
            _controller = value;
            var menuOptions = GetComponentsInChildren<MenuOptions>(true);
            foreach (var menuOption in menuOptions)
            {
                menuOption.controller = _controller;
            }
        }
    }

    protected virtual void Start()
    {
        if (_controller == null)
        {
            _controller = useSystemPlayer ? ReInput.players.SystemPlayer : ReInput.players.GetPlayer(0);
        }
    }

    private void Update()
    {
        if(ready)
        {
            ReadyUpdate();
        }
    }

    public virtual void ReadyUpdate() { }

    public virtual void OnEnable()
    {
        if(!_canvasGroup)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        StartCoroutine(Appear());
    }

    protected IEnumerator Appear()
    {
        AppearStart();
        ready = false;
        _visible = true;
        _canvasGroup.alpha = 0;
        _canvasGroup.gameObject.SetActive(true);
        while (_canvasGroup.alpha < 1)
        {
            _canvasGroup.alpha += Time.unscaledDeltaTime * 4;
            yield return null;
        }

        _canvasGroup.alpha = 1;
        ready = true;
        AppearFinished();
    }

    public virtual void AppearStart() { }
    public virtual void AppearFinished() { }

    protected IEnumerator Hide()
    {
        HideStart();
        ready = false;
        while (_canvasGroup.alpha > 0)
        {
            _canvasGroup.alpha -= Time.unscaledDeltaTime * 4;
            yield return null;
        }
        _visible = false;
        HideFinished();
    }

    public virtual void HideStart() { }
    public virtual void HideFinished() { }

    protected virtual void OnDisable()
    {
        _visible = false;
    }

    public void GoToState(ScreenState newState)
    {
        if (enabled && ready && gameObject.activeInHierarchy)
        {
            StartCoroutine(GoToStateCoroutine(newState));
        }
    }

    protected virtual IEnumerator GoToStateCoroutine(ScreenState newState)
    {
        yield return StartCoroutine(Hide());
        newState.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
