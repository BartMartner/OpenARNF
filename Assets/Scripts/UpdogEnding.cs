using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdogEnding : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if(SaveGameManager.activeGame == null || !SaveGameManager.activeGame.itemsPossessed.Contains(MajorItem.UpDog))
        {
            Destroy(gameObject);
        }
    }
}
