using UnityEngine;
using System.Collections;
using System.Linq;

public class MinorItemObjectToggle : MinorItemDependantObject
{
    public GameObject minorItemExists;
    public GameObject minorItemDoesNotExists;

    public override void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (roomAbstract.minorItems.Any(i => i.spawnInfo.localID == minorItemLocalID))
        {
            if (minorItemDoesNotExists)
            {
                DestroyImmediate(minorItemDoesNotExists);
            }
            minorItemExists.SetActive(true);
        }
        else
        {
            DestroyImmediate(minorItemExists);
            if (minorItemDoesNotExists)
            {
                minorItemDoesNotExists.SetActive(true);
            }
        }
    }
}
