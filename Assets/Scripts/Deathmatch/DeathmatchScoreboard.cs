using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeathmatchScoreboard : MonoBehaviour
{
    public Text header;
    public GameObject redRow;
    public GameObject blueRow;
    public GameObject greenRow;
    public GameObject yellowRow;
    public Text[] redText;
    public Text[] blueText;
    public Text[] greenText;
    public Text[] yellowText;
    public GameObject pressToContinue;

    public IEnumerator Show(string text)
    {
        Time.timeScale = 0;

        gameObject.SetActive(true);
        pressToContinue.SetActive(false);

        header.text = text;

        var scores = DeathmatchManager.instance.score;
        var deaths = DeathmatchManager.instance.deaths;
        var totalScores = DeathmatchManager.instance.totalScore;
        var idsJoined = DeathmatchManager.instance.idsJoined;

        //make totalScores gray
        foreach (var s in totalScores)
        {
            var teamText = GetTeamText(s.Key);
            teamText[1].color = new Color(0.33f,0.33f,0.33f);
        }

        //show match Scores
        var sorted = scores.OrderByDescending(kvp => kvp.Value).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            var team = sorted[i].Key;
            Text[] teamText = GetTeamText(team);
            GameObject row = GetTeamRow(team);
            int id = DeathmatchManager.instance.TeamToIndex(team);

            row.transform.SetSiblingIndex(i + 1);
            row.SetActive(idsJoined.Contains(id));
            teamText[2].text = scores[team].ToString();
            teamText[3].text = deaths[team].ToString();
        }
   
        yield return new WaitForSecondsRealtime(2f);

        //change total Scores
        sorted = totalScores.OrderByDescending(kvp => kvp.Value).ToList();
        for (int i = sorted.Count-1; i >= 0; i--)
        {
            var team = sorted[i].Key;
            int id = DeathmatchManager.instance.TeamToIndex(team);

            if (idsJoined.Contains(id))
            {
                var teamText = GetTeamText(team);
                teamText[1].color = Color.white;
                teamText[1].text = totalScores[team].ToString();

                var row = GetTeamRow(team);
                row.transform.SetSiblingIndex(i + 1);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        yield return new WaitForSecondsRealtime(1f);
        pressToContinue.SetActive(true);

        DeathmatchManager.instance.RestartMatch();
    }

    public GameObject GetTeamRow(Team team)
    {
        switch (team)
        {
            case Team.DeathMatch0:                
                return redRow; 
            case Team.DeathMatch1:
                return blueRow;               
            case Team.DeathMatch2:
                return yellowRow;
            case Team.DeathMatch3:
                return greenRow;
            default:
                return null;
        }
    }


    public Text[] GetTeamText(Team team)
    {
        switch (team)
        {
            case Team.DeathMatch0:
                return redText;
            case Team.DeathMatch1:
                return blueText;
            case Team.DeathMatch2:
                return yellowText;
            case Team.DeathMatch3:
                return greenText;
            default:
                return null;
        }
    }
}
