using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDeathmatchScreenPosition : MonoBehaviour
{
    public int playerId;
    private Vector2 _lastScreenSize;

    public void LateUpdate()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);
        if (screenSize != _lastScreenSize)
        {
            var actualAspectRatio = screenSize.x / screenSize.y;
            var positon = new Vector2(actualAspectRatio * 0.5f, 0.5f);
            if (playerId == 0 || playerId == 2) { positon.x *= -1; }
            if (playerId == 2 || playerId == 3) { positon.y *= -1; }
            transform.position = positon;
            _lastScreenSize = screenSize;
        }
    }
}
