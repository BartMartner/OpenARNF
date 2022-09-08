using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInGameMode : MonoBehaviour
{
    public GameMode gameMode;

    public void Awake()
    {
        if(SaveGameManager.activeGame != null && gameMode.HasFlag(SaveGameManager.activeGame.gameMode))
        {
            Destroy(gameObject);
        }
    }
}
