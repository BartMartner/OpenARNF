using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnPointTriggered : MonsterSpawnPoint
{
    public Transform spawnPoint;
    private bool _ready;

    protected override IEnumerator Start()
    {
        var sr = spawnPoint.GetComponent<SpriteRenderer>();
        if (sr) { Destroy(sr); }

        if (_room)
        {
            while (_room.random == null) { yield return null; }
            _random = _room.random;
            while (LayoutManager.instance && _room.roomAbstract == null) { yield return null; }
        }

        var game = SaveGameManager.activeGame;
        if (game != null && game.lockDownRoomsCleared.Contains(_room.roomAbstract.roomID))
        {
            Destroy(gameObject);
            yield break;
        }

        Randomize();
    }

    public override void Randomize()
    {
        PopulateRandomPool();
        _ready = true;
    }

    protected override Vector3 GetPosition(EnemySpawnInfo spawnInfo)
    {
        if (spawnPoint) return spawnPoint.position + spawnInfo.offset;
        return base.GetPosition(spawnInfo);
    }

    protected override Quaternion GetRotation(EnemySpawnInfo spawnInfo)
    {
        if (spawnPoint && spawnInfo.matchRotation) return spawnPoint.rotation;
        return base.GetRotation(spawnInfo);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if (player && !player.notTargetable && enabled && _ready)
        {
            SpawnMonster();
        }
    }

#if UNITY_EDITOR
    public override void OnDrawGizmos()
    {
        if (spawns != null && spawns.Length > 0)
        {
            var index = (int)(UnityEditor.EditorApplication.timeSinceStartup % spawns.Length);
            if (spawns[index].prefab)
            {
                var spriteRenderer = spawns[index].prefab.GetComponentInChildren<SpriteRenderer>();
                var sr = GetComponentInChildren<SpriteRenderer>();
                if (sr && spriteRenderer)
                {
                    sr.sprite = spriteRenderer.sprite;
                }
            }
        }
    }
#endif
}
