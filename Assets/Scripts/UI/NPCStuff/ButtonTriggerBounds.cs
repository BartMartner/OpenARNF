using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonTriggerBounds : MonoBehaviour
{
    private bool _playerPresent;
    private Player _player;
    private bool _buttonHintVisible;
    public SpriteRenderer buttonHint;
    public UnityEvent onSubmit;
    private IEnumerator _buttonFadeIn;
    private IEnumerator _buttonFadeOut;

    protected virtual void Awake()
    {
        _playerPresent = false;
        gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        buttonHint.color = Color.clear;   
    }

    protected virtual IEnumerator Start()
    {
        yield return null;
        var text = buttonHint.GetComponentInChildren<TextMesh>();
        if (text) text.color = Color.clear;
    }

    // Update is called once per frame
    public void Update()
    {
        if(Time.timeScale == 0) return;

        if (_playerPresent && _player.grounded &&
            !_player.teleporting && _player.activeSpecialMove == null &&
            !NPCDialogueManager.instance.dialogueActive &&
            (!Automap.instance || !Automap.instance.gridSelectMode))
        {
            if (buttonHint && !_buttonHintVisible)
            {
                StopButtonCoroutines();
                _buttonFadeIn = ButtonHintFadeIn();
                StartCoroutine(_buttonFadeIn);
            }

            if (_player.controller.GetButtonDown("SpecialMove")) { OnSubmit(); }
        }
        else if (buttonHint && _buttonHintVisible)
        {
            FadeOut();
        }

        buttonHint.transform.rotation = Quaternion.identity;
    }

    public void StopButtonCoroutines()
    {
        if(_buttonFadeIn != null)
        {
            StopCoroutine(_buttonFadeIn);
            _buttonFadeIn = null;
        }

        if(_buttonFadeOut != null)
        {
            StopCoroutine(_buttonFadeOut);
            _buttonFadeOut = null;
        }
    }

    public virtual void OnSubmit() { if (onSubmit != null) { onSubmit.Invoke(); } }

    public void FadeOut()
    {
        StopButtonCoroutines();
        _buttonFadeOut = ButtonHintFadeOut();
        StartCoroutine(_buttonFadeOut);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if(player)
        {
            _player = player;
            _playerPresent = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_player != null && collision.gameObject == _player.gameObject)
        {
            StopButtonCoroutines();
            if (buttonHint) { StartCoroutine(ButtonHintFadeOut()); }
            _player = null;
            _playerPresent = false;
        }
    }

    public IEnumerator ButtonHintFadeIn()
    {
        _buttonHintVisible = true;
        var text = buttonHint.GetComponentInChildren<TextMesh>();

        var timer = 0f;
        var startingColorHint = buttonHint.color;
        var startingTextColor = text ? text.color : Color.clear;
        var time = (1 -buttonHint.color.a) * 0.5f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            buttonHint.color = Color.Lerp(startingColorHint, Color.white, timer / time); ;
            if (text) text.color = Color.Lerp(startingTextColor, Color.black, timer / time); ;            
            yield return null;
        }
        buttonHint.color = Color.white;
    }

    public IEnumerator ButtonHintFadeOut()
    {
        _buttonHintVisible = false;        

        var text = buttonHint.GetComponentInChildren<TextMesh>();

        var timer = 0f;
        var startingColorHint = buttonHint.color;
        var startingTextColor = text ? text.color : Color.clear;
        var time = (startingColorHint.a) * 0.5f;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            buttonHint.color = Color.Lerp(startingColorHint, Color.clear, timer / time); ;
            if (text) text.color = Color.Lerp(startingTextColor, Color.clear, timer / time); ;            
            yield return null;
        }
        buttonHint.color = Color.clear;
    }
}
