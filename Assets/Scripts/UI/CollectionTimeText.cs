using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CollectionTimeText : MonoBehaviour
{
    private Text _text;
    public GameObject race;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;

    public void Awake ()
    {
        _text = GetComponent<Text>();
        var data = SaveGameManager.activeGame;
        StringBuilder sb = new StringBuilder();
        if (data != null && data.runCompleted)
        {
            if (race) { race.SetActive(data.raceMode); }
            if (data.gameMode != GameMode.BossRush)
            {
                var collectRate = (int)(data.collectRate * 100);
                sb.AppendLine("Collect Rate: " + collectRate + "%");
            }
            else
            {
                sb.AppendLine("Mission Complete");
            }

            var t = TimeSpan.FromSeconds(data.playTime);
            var playTime = t.Hours.ToString("00") + ":" + t.Minutes.ToString("00") + ":" + t.Seconds.ToString("00");
            sb.AppendLine("Play Time: " + playTime);
            sb.AppendLine();

            player2.SetActive(data.player2Entered);
            player3.SetActive(data.player3Entered);
            player4.SetActive(data.player4Entered);
        }
        else
        {
            if (race) { race.SetActive(false); }
            sb.AppendLine("Collect Rate: -");
            sb.AppendLine("Play Time: --:--:--");
            sb.AppendLine();
        }

        _text.text = sb.ToString();
    }
}
