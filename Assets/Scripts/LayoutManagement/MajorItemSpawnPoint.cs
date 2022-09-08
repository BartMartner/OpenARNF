using UnityEngine;
using System.Collections;

public class MajorItemSpawnPoint : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (roomAbstract.majorItem == 0 || (SaveGameManager.activeGame != null &&
            SaveGameManager.activeGame.itemsCollected.Contains(roomAbstract.majorItem)))
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instantiate(Resources.Load<GameObject>("MajorItemPickUps/" + roomAbstract.majorItem.ToString()), transform.position, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }
}
