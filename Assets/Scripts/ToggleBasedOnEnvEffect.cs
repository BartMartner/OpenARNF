using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBasedOnEnvEffect : MonoBehaviour, IAbstractDependantObject
{
    public EnvironmentalEffect envEffect;
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public GameObject effectPresent;
    public GameObject effectNotPresent;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (roomAbstract.environmentalEffect == envEffect)
        {
            if (effectPresent) { effectPresent.SetActive(true); }
            if (effectNotPresent) { Destroy(effectNotPresent); }
        }
        else
        {
            if (effectNotPresent) { effectNotPresent.SetActive(true); }
            if (effectPresent) { Destroy(effectPresent); }
        }
    }
}
