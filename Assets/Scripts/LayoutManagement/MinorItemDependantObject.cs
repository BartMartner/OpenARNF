using UnityEngine;
using System.Collections;
using System.Linq;

public class MinorItemDependantObject : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public int minorItemLocalID;

    public virtual void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (!roomAbstract.minorItems.Any(i => i.spawnInfo.localID == minorItemLocalID))
        {
            DestroyImmediate(gameObject);
        }
    }

}
