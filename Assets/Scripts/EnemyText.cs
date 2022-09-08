using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyText : MonoBehaviour
{
    private Text _text;
    private float _transitionTimer = 0.5f;
    void Awake()
    {
        var game = SaveGameManager.activeGame;
        if (game != null && game.gameMode != GameMode.Exterminator) { Destroy(gameObject); }
        _text = GetComponent<Text>();
        _text.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        if(!LayoutManager.instance || LayoutManager.instance.transitioning)
        {
            _transitionTimer = 0.5f;
            _text.text = string.Empty;
        }
        else if (_transitionTimer > 0)
        {
            _transitionTimer -= Time.deltaTime;
            _text.text = string.Empty;
        }
        else if(_text && !LayoutManager.instance.transitioning)
        {
            if (EnemyManager.instance)
            {
                var e = EnemyManager.instance.enemies.Count;
                if (e > 0)
                {
                    var text = e + " HOSTILE";
                    if (e > 1) { text += "S"; }
                    _text.text = text;
                    _text.color = Constants.damageFlashColor;
                }
                else
                {
                    _text.text = "CLEAR";
                    _text.color = Color.green;
                }
            }
            else
            {
                _text.text = string.Empty;
            }
        }
    }
}
