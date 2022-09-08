using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyIfEnvironmentEffect : MonoBehaviour, IAbstractDependantObject
{
    public EnvironmentalEffect[] prohibitedEnvEffects;
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(prohibitedEnvEffects.Contains(roomAbstract.environmentalEffect))
        {
            Destroy(gameObject);
        }
    }
}
