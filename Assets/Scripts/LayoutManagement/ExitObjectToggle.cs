using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class ExitObjectToggle : ExitDependentObject
{
    public GameObject exitMatch;
    public GameObject exitNotMatch;

    public override void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(exitRequirements.MatchExists(roomAbstract))
        {
            DestroyImmediate(exitNotMatch);
            exitMatch.SetActive(true);
        }
        else
        {
            if (roomAbstract.exits.Any((e) => e.direction == exitRequirements.direction && e.localGridPosition == exitRequirements.localGridPosition))
            {
                Debug.LogError("Room Abstract expects a " + exitRequirements.direction + " exit at " + exitRequirements.localGridPosition + " but no valid exits can be found.");
                DestroyImmediate(exitNotMatch);
                exitMatch.SetActive(true);
            }
            else
            {
                DestroyImmediate(exitMatch);
                exitNotMatch.SetActive(true);
            }
        }
    }
}
