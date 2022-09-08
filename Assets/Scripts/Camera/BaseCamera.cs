using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCamera : MonoBehaviour
{
    new public Camera camera;
    public const float defaultSize = 7;
    public const float defaultHalfWidth = 12;

    public float currentZoomScale
    {
        get { return camera ? camera.orthographicSize / defaultSize : 1; }
    }

    public float boundsMaxY
    {
        get { return transform.position.y + (camera ? camera.orthographicSize : defaultSize); }
    }

    public float orthographicSize
    {
        get { return camera ? camera.orthographicSize : defaultSize; }
    }

    public float halfWidth
    {
        get
        {
            if (camera && camera.targetTexture)
            {
                return ((float)camera.targetTexture.width / camera.targetTexture.height) * camera.orthographicSize;
            }
            else
            {
                return camera ? ((float)Screen.width / Screen.height) * orthographicSize : defaultHalfWidth;
            }
        }
    }
}
