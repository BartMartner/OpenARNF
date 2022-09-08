using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalFollower : Follower
{
    private float _distance = 2f;
    //time it takes for a full rotation;
    private float _rotationTime = 3f;

    public bool matchPlayerFacing = true;

    public override bool orbital
    {
        get { return true; }
    }

    public virtual void Update()
    {
        var position = GetTargetPosition();

        if (Vector3.Distance(transform.position, position) < _distance * 2)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * player.maxSpeed * 2);
        }
        else
        {
            transform.position = position;
        }

        if (matchPlayerFacing)
        {
            transform.rotation = player.transform.rotation;
        }
    }

    private Vector3 GetTargetPosition()
    {
        var position = player.transform.position;
        if (player.orbitalFollowerCount > 0)
        {
            var offsetAngle = ((float)positionNumber / player.orbitalFollowerCount) * 360;
            offsetAngle += (Time.time % _rotationTime) / _rotationTime * 360;

            position += Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * _distance;
        }
        return position;
    }

    public override void OnRespawn()
    {
        base.OnRespawn();
        transform.position = GetTargetPosition();
    }
}
