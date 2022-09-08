using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerOrbital : MonoBehaviour
{
    public float speed;
    public bool hasFacing;

    public void Update()
    {
        var direction = (Player.instance.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (hasFacing)
        {
            if (direction.x < 0 && transform.rotation != Constants.flippedFacing)
            {
                transform.rotation = Constants.flippedFacing;
            }
            else if (direction.x > 0 && transform.rotation != Quaternion.identity)
            {
                transform.rotation = Quaternion.identity;
            }
        }
    }
}
