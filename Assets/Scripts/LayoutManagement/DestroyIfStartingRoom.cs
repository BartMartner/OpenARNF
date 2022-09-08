using UnityEngine;
using System.Collections;

public class DestroyIfStartingRoom : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(roomAbstract.isStartingRoom)
        {
            DestroyImmediate(gameObject);
        }
    }
}
