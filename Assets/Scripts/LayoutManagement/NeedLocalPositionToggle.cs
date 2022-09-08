using UnityEngine;
using System.Collections;
using System.Linq;

public class NeedLocalPositionToggle : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public Int2D localGridPosition;
    public GameObject needed;
    public GameObject notNeeded;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (roomAbstract.exits.Any((e) => e.localGridPosition == localGridPosition))
        {
            DestroyImmediate(notNeeded);
            needed.SetActive(true);
        }
        else
        {
            DestroyImmediate(needed);
            notNeeded.SetActive(true);
        }
    }
}
