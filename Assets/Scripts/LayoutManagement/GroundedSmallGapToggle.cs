using UnityEngine;
using System.Collections;

public class GroundedSmallGapToggle : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public Direction direction;
    public Int2D localGridPosition;

    public GameObject needsSmallGap;
    public GameObject doesNotNeedSmallGap;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        var exit = roomAbstract.exits.Find(e => e.direction == direction && e.localGridPosition == localGridPosition);

        if (exit == null)
        {
            Debug.LogWarning("SetByExitDamageType " + gameObject.name + " had no match exits so its getting destroyed");
            DestroyImmediate(gameObject);
            return;
        }
        else if (exit.toExit.requiresGroundedSmallGaps)
        {
            if (doesNotNeedSmallGap)
            {
                DestroyImmediate(doesNotNeedSmallGap);
            }

            needsSmallGap.SetActive(true);
        }
        else
        {
            DestroyImmediate(needsSmallGap);

            if (doesNotNeedSmallGap)
            {
                doesNotNeedSmallGap.SetActive(true);
            }
        }
    }
}
