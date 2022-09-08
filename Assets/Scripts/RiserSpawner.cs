using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiserSpawner : MonoBehaviour
{
    public Riser riserPrefab;
    public float riserInterval = 2f;
    public float xRange = 24;
    public float yRange = 12;
    private float _timer;

    private void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else 
        {
            var closestPlayer = PlayerManager.instance.GetClosestInArc(transform.position, transform.up, 24, 180);
            if(closestPlayer && (closestPlayer.position.y - transform.position.y) < yRange)
            {
                _timer = riserInterval;
                var facing = closestPlayer.position.x > transform.position.x ? Quaternion.identity : Constants.flippedFacing;
                var riser = Instantiate(riserPrefab, transform.position, facing, transform.parent);
                riser.Rise(closestPlayer.transform.position.y);
            }
        }
    }
}
