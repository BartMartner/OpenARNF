using UnityEngine;
using System.Collections;

public class IgnoreTerrainToggle : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public Direction direction;
    public Int2D localGridPosition;

    public GameObject requiresIgnoreTerrain;
    public GameObject doesNotRequireIgnoreTerrain;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        var exit = roomAbstract.exits.Find(e => e.direction == direction && e.localGridPosition == localGridPosition);

        if (exit == null)
        {
            Debug.LogWarning("SetByExitDamageType " + gameObject.name + " had no match exits so its getting destroyed");
            DestroyImmediate(gameObject);
            return;
        }
        else if (exit.toExit.requiresShotIgnoresTerrain)
        {
            if (doesNotRequireIgnoreTerrain)
            {
                DestroyImmediate(doesNotRequireIgnoreTerrain);
            }

            requiresIgnoreTerrain.SetActive(true);
        }
        else
        {
            DestroyImmediate(requiresIgnoreTerrain);

            if (doesNotRequireIgnoreTerrain)
            {
                doesNotRequireIgnoreTerrain.SetActive(true);
            }
        }
    }
}
