using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionBasedOnPlayerEntrance : MonoBehaviour
{
    public Transform[] exits;
    public Transform[] spawnPositions;
    public bool destroySpawnPositions = true;
    public bool matchRotation = true;

    public IEnumerator Start()
    {
        if(exits.Length != spawnPositions.Length)
        {
            Debug.LogError("exits.Length != spawnPositions.Length on " + gameObject.name);
            yield break;
        }

        var parentRoom = GetComponentInParent<Room>();

        while(!PlayerManager.instance || (LayoutManager.instance &&
             (LayoutManager.CurrentRoom != parentRoom ||
             (LayoutManager.CurrentRoom && LayoutManager.CurrentRoom.roomAbstract == null))))
        {
            yield return null;
        }

        var closestIndex = 0;
        var closestDistance = 1000f;
        var player = PlayerManager.instance.player1;

        for (int i = 0; i < exits.Length; i++)
        {
            var e = exits[i];

            if (!e) continue;

            var distance = Vector3.Distance(e.position, player.position);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }

        transform.position = spawnPositions[closestIndex].position;

        if(matchRotation) { transform.rotation = spawnPositions[closestIndex].rotation; }
        
        if(destroySpawnPositions)
        {
            for (int i = 0; i < spawnPositions.Length; i++)
            {
                Destroy(spawnPositions[i].gameObject);
            }
        }

        Destroy(this);
    }
}
