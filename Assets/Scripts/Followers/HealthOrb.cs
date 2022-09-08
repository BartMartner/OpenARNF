using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthOrb : MonoBehaviour
{
    public float delay = 8f;
    public UnityEvent onSpawn;

    private float _timer = 0f;
    private Follower follower;

    public void Start()
    {
        follower = GetComponent<Follower>();
    }

    public void Update()
    {
        if (Time.timeScale == 0) { return; }

        var spawn = DeathmatchManager.instance ? true : EnemyManager.instance.enemies.Count > 0;
        if (spawn && follower.player && follower.player.health < follower.player.maxHealth)
        {
            if (_timer < delay)
            {
                _timer += Time.deltaTime;
            }
            else
            {
                _timer = 0;
                PickUpManager.instance.SpawnPickUp(DropType.SmallHealth, transform.position);
                if (onSpawn != null) { onSpawn.Invoke(); }
            }
        }
    }
}
