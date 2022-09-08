using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetByExitDamageType : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public Direction direction;
    public Int2D localGridPosition;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        var exit = roomAbstract.exits.Find(e => e.direction == direction && e.localGridPosition == localGridPosition);
        if(exit == null)
        {
            Debug.LogWarning("SetByExitDamageType " + gameObject.name + " had no match exits so its getting destroyed");
            DestroyImmediate(gameObject);
            return;
        }

        var iSetByDamageTypes = new List<ISetByDamageType>(gameObject.GetInterfacesInChildren<ISetByDamageType>());
        if (iSetByDamageTypes.Count <= 0)
        {
            Debug.LogWarning("SetByExitDamageType " + gameObject.name + " had no ISetByDamageTypes in children");
        }
        else
        {
            foreach (var i in iSetByDamageTypes)
            {
                i.SetByDamageType(exit.toExit.requiredDamageType);
            }
        }
    }
}
