using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressStartText : MonoBehaviour
{
    private Text _startText;
    private SetImageForControl _button;
    private Rewired.Player _controller;
    private ControllerType _lastActive;

    private void Awake()
    {
        _controller = ReInput.players.SystemPlayer;
        _startText = GetComponentInChildren<Text>();
        _button = GetComponentInChildren<SetImageForControl>();
        _lastActive = _controller.controllers.Joysticks.Count > 0 ? ControllerType.Joystick : ControllerType.Keyboard;
        SetText();
    }

    private void Update()
    {
        if (_controller != null && _controller.controllers != null)
        {
            var lastActive = _controller.controllers.GetLastActiveController();
            if (lastActive != null && _lastActive != lastActive.type)
            {
                _lastActive = lastActive.type;
                SetText();
            }
        }
    }

    public void SetText()
    {       
        if (_lastActive == ControllerType.Keyboard)
        {
            _startText.text = "Press Any Key";
            _startText.rectTransform.anchoredPosition = Vector2.zero;
            _button.gameObject.SetActive(false);
        }
        else
        {
            _startText.text = "Press";
            _startText.rectTransform.anchoredPosition = new Vector2(-8, 0);
            _button.gameObject.SetActive(true);
            _button.SetImage();
        }
    }
}
