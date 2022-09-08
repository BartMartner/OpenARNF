using UnityEngine;
using System.Collections;

public class FacePlayer : BaseMovementBehaviour
{
    public bool invert;
    public void Update()
    {   
        var playerDeltaX = Player.instance.transform.position.x - transform.position.x;
        if (invert)
        {
            transform.rotation = playerDeltaX > 0 ? Constants.flippedFacing : Quaternion.identity;
        }
        else
        {
            transform.rotation = playerDeltaX > 0 ? Quaternion.identity : Constants.flippedFacing;
        }
    }
}
