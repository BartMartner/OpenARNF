using UnityEngine;
using System.Collections;

public class ScreenSizer : MonoBehaviour
{
    public Vector2 referenceSize = new Vector2(384,224);
    private Vector2 _lastScreenSize;

    public void LateUpdate()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);
        if (screenSize != _lastScreenSize)
        {
            var actualAspectRatio = screenSize.x / screenSize.y;
            var targetAspectRatio = referenceSize.x / referenceSize.y;
            var newScale = new Vector3(actualAspectRatio, actualAspectRatio * 1/targetAspectRatio, 0);
            if (newScale.y > 1)
            {
                if (actualAspectRatio > 1.77 && actualAspectRatio < 1.78)
                {
                    newScale.y = 1;
                }
                else
                {
                    var adjust = 1 / newScale.y;
                    newScale.y *= adjust;
                    newScale.x *= adjust;
                }
            }

            var activeGame = SaveGameManager.activeGame;
            if(activeGame != null && activeGame.gameMode == GameMode.MirrorWorld)
            {
                newScale.y *= -1;
            }

            transform.localScale = newScale;
            _lastScreenSize = screenSize;
        }
    }
}
