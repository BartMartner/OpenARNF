using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersatileTraversalToggle : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public Direction direction;
    public Int2D localGridPosition;

    public GameObject noTraversal;
    public GameObject smallGap;
    public GameObject ignoreTerrain;
    public GameObject phaseWall;

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
            if (noTraversal) { DestroyImmediate(noTraversal); }
            if (ignoreTerrain) { DestroyImmediate(ignoreTerrain); }
            if (phaseWall) { DestroyImmediate(phaseWall); }
            if (smallGap) { smallGap.SetActive(true); }
            else { Debug.LogWarning("Not smallGap GameObject found!"); }
        }
        else if (exit.toExit.requiresShotIgnoresTerrain)
        {
            if (noTraversal) { DestroyImmediate(noTraversal); }
            if (smallGap) { DestroyImmediate(smallGap); }
            if (phaseWall) { DestroyImmediate(phaseWall); }
            if (ignoreTerrain) { ignoreTerrain.SetActive(true); }
            else { Debug.LogWarning("Not ignoreTerrain GameObject found!"); }
        }
        else if (exit.toExit.requiresPhaseThroughWalls)
        {
            if (noTraversal) { DestroyImmediate(noTraversal); }
            if (smallGap) { DestroyImmediate(smallGap); }
            if (ignoreTerrain) { DestroyImmediate(ignoreTerrain); }
            if (phaseWall) { phaseWall.SetActive(true); }
            else { Debug.LogWarning("Not phaseWall GameObject found!"); }
        }
        else
        {
            if (smallGap) { DestroyImmediate(smallGap); }
            if (ignoreTerrain) { DestroyImmediate(ignoreTerrain); }
            if (phaseWall) { DestroyImmediate(phaseWall); }
            if (noTraversal) { noTraversal.SetActive(true); }
        }
    }
}
