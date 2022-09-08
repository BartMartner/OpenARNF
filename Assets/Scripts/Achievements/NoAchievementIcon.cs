using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoAchievementIcon : MonoBehaviour
{
    public Sprite race;

    void Start()
    {
        var activeGame = SaveGameManager.activeGame;
        if (activeGame == null) return;
        
        if(activeGame.allowAchievements)
        {
            Destroy(gameObject);
        }
        else if (activeGame.raceMode)
        {
            var image = GetComponent<Image>();
            image.sprite = race;
        }
    }
}
