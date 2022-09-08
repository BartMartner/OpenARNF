using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PermanentStateObject : MonoBehaviour, IAbstractDependantObject
{
    public int localID;
    private int _globalID;

    public UnityEvent<int> onSetState;

    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    protected bool softFail;

    public virtual void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        int state = 0;

        if (roomAbstract.permanentStateObjectGlobalIds.Count > localID && SaveGameManager.activeGame != null)
        {
            _globalID = roomAbstract.permanentStateObjectGlobalIds[localID];
            SaveGameManager.activeGame.permanentStateObjects.TryGetValue(_globalID, out state);
            SetStateFromSave(state);
        }
        else
        {
            softFail = true;
        }
    }

    public virtual void SetStateFromSave(int state)
    {
        if (onSetState != null)
        {
            onSetState.Invoke(state);
        }
    }

    public void SaveState(int state)
    {
        if(SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeGame.permanentStateObjects[_globalID] = state;
            SaveGameManager.instance.Save();
        }
    }
}
