using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class MinorItemSpawnInfo
{
    [JsonProperty(PropertyName = "id")]
    public int localID;

    [JsonProperty(PropertyName = "locGrd")]
    public Int2D localGridPosition;

    [JsonProperty(PropertyName = "conExt")]
    public List<Int2DDirection> conflictingExits = new List<Int2DDirection>();
}

public class MinorItemSpawnPoint : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public MinorItemSpawnInfo info = new MinorItemSpawnInfo();
    public DestructibleTileBounds linkedDestructable;

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (roomAbstract.minorItems.Count <= 0)
        {
            Destroy(gameObject);
            return;
        }

        var data = roomAbstract.minorItems.Find(d => d.spawnInfo.localID == info.localID);
        if (data == null || SaveGameManager.activeGame.minorItemIdsCollected.Contains(data.globalID))
        {
            Destroy(gameObject);
            return;
        }

        MinorItemPickUp prefab = ResourcePrefabManager.instance.LoadGameObject("PickUps/" + data.type.ToString()).GetComponent<MinorItemPickUp>(); ;
        MinorItemPickUp item = Instantiate(prefab, transform.position, Quaternion.identity, transform.parent) as MinorItemPickUp;

        if (item != null)
        {
            item.data = data;
            item.linkedDestructable = linkedDestructable;
        }
        else
        {
            Debug.LogError("MinorItemSpawnPoint could not find prefab to match MinorItemType");
        }
    }
}
