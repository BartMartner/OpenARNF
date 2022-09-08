using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroNPCSpawn : MonoBehaviour
{
    public GameObject NormalMode;
    public GameObject ExterminatorMode;

    public void Start()
    {
        var game = SaveGameManager.activeGame;
        if (game == null)
        {
            Instantiate(NormalMode, transform.position, transform.rotation, transform.parent);
        }
        else
        {
            switch(game.gameMode)
            {
                case GameMode.Exterminator:
                    Instantiate(ExterminatorMode, transform.position, transform.rotation, transform.parent);
                    break;
                default:
                    Instantiate(NormalMode, transform.position, transform.rotation, transform.parent);
                    break;
            }
        }

        Destroy(gameObject);
    }
}
