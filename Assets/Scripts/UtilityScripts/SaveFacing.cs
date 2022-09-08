using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFacing : PermanentStateObject
{
    public override void SetStateFromSave(int state)
    {
        base.SetStateFromSave(state);
        transform.rotation = state == 0 ? Quaternion.identity : Constants.flippedFacing;
    }

    public void SaveState()
    {
        SaveState(transform.rotation == Quaternion.identity ? 0 : 1);
    }
}
