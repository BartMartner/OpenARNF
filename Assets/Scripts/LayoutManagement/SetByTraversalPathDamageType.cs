using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetByTraversalPathDamageType : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public int traversalPathIndex;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        DamageType damageType = DamageType.Generic;
        if (traversalPathIndex >= 0 && traversalPathIndex < roomAbstract.traversalPathRequirements.Count)
        {
            var d = roomAbstract.traversalPathRequirements[traversalPathIndex].requiredDamageType;
            if (d != 0)
            {
                damageType = d;
            }
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
                i.SetByDamageType(damageType);
            }
        }
    }
}
