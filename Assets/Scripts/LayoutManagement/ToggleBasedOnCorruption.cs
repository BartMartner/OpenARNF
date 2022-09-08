using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBasedOnCorruption : MonoBehaviour
{
    public float corruptionThreshold = 2f;
    public GameObject underCorruption;
    public GameObject overCorruption;
    public float debugCorruption;

    protected float _actualCorruption;

    public virtual void Awake()
    {
        _actualCorruption = SaveGameManager.activeGame == null ? debugCorruption : SaveGameManager.activeGame.corruption;

        if (_actualCorruption > corruptionThreshold)
        {
            DestroyImmediate(underCorruption);
            overCorruption.SetActive(true);
        }
        else
        {
            DestroyImmediate(overCorruption);
            underCorruption.SetActive(true);
        }
    }
}
