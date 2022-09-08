using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeText : MonoBehaviour
{
    public bool deathMatch;
    private Text _timeText;
    private bool _expanded = true;

    void Start()
    {
        _timeText = GetComponent<Text>();
        if(deathMatch)
        {
            _expanded = SaveGameManager.deathmatchSettings != null && SaveGameManager.deathmatchSettings.mode == DeathmatchMode.TimeLimit;
        }
        else
        {
            _expanded = PlayerManager.instance.players.Count > 0 && SaveGameManager.activeSlot != null && SaveGameManager.activeSlot.timeAlwaysVisible;
        }
        
        _timeText.enabled = _expanded;
    }

    // Update is called once per frame
    void Update()
    {
        float playTime = 0f;

        if (deathMatch)
        {
            playTime = DeathmatchManager.instance ? DeathmatchManager.instance.time : 0;
            _expanded = SaveGameManager.deathmatchSettings != null && SaveGameManager.deathmatchSettings.mode == DeathmatchMode.TimeLimit;
        }
        else
        {
            var player = PlayerManager.instance.player1;
            if (player) { playTime = player.playTime; }

            if (SaveGameManager.activeSlot != null)
            {
                _expanded = player && (!Automap.instance || !Automap.instance.gridSelectMode) &&
                    (SaveGameManager.activeSlot.timeAlwaysVisible || player.controller.GetButton("ExpandMap"));
            }
        }

        _timeText.enabled = _expanded;

        if (_expanded)
        {
            var t = TimeSpan.FromSeconds(playTime);
            if (deathMatch)
            {
                _timeText.text = t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            }
            else
            {
                _timeText.text = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            }
        }
    }
}
