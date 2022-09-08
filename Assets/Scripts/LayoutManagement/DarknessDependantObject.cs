using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessDependantObject : MonoBehaviour, IAbstractDependantObject
{
    public float maximumLight = 1;
    public float minimumLight = 0;

    public int priority;
    public int m_priority
    {
        get
        {
            return priority;
        }

        set
        {
            priority = value;
        }
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(minimumLight > 0 && roomAbstract.environmentalEffect == EnvironmentalEffect.Darkness)
        {
            Destroy(gameObject);
        }

        if(roomAbstract.light > maximumLight || roomAbstract.light < minimumLight)
        {
            Destroy(gameObject);
        }
    }
}
