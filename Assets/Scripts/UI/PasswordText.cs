using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PasswordText : MonoBehaviour
{
    private Text _text;

    public void Awake()
    {
        _text = GetComponent<Text>();
    }

    public void OnEnable()
    {
        if(!_text) { _text = GetComponent<Text>(); }

        var activeGame = SaveGameManager.activeGame;
        if (_text && activeGame != null && activeGame.layout != null)
        {
            switch (activeGame.gameMode)
            {
                case GameMode.ClassicBossRush:
                    _text.text = "CLASSIC BOSS RUSH";
                    _text.color = Color.white;
                    break;
                case GameMode.Spooky:
                    _text.text = "ERROR";
                    _text.color = Color.white;
                    break;
                case GameMode.MirrorWorld:
                    _text.text = "MIRROR WORLD";
                    _text.color = Color.white;
                    break;
                default:
                    var password = activeGame.layout.password;
                    if (!string.IsNullOrEmpty(password))
                    {
                        _text.text = "SEED:\n" + password.Substring(0, 6) + ' ' + password.Substring(6, 6) + '\n' +
                                       password.Substring(12, 6) + ' ' + password.Substring(18, 6);
                    }
                    _text.color = password == activeGame.password ? Color.gray : Color.white;
                    break;
            }
        }
        else
        {
            _text.text = "SEED:\n" + "------ ------" + '\n' + "------ ------";
        }
    }
}
