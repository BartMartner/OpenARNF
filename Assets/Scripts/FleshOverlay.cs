using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshOverlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SaveGameManager.activeSlot != null)
        {
            if(SaveGameManager.activeGame!= null && SaveGameManager.activeGame.gameMode == GameMode.BossRush) { return; }

            if (!SaveGameManager.activeSlot.achievements.Contains(AchievementID.TheFlesheningII))
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}
