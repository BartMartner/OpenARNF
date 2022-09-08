using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ExitDependentObject : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public ExitLimitations exitRequirements;

    public virtual void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (!exitRequirements.MatchExists(roomAbstract))
        {
            DestroyImmediate(gameObject);
        }
    }
}
