using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCamera : MonoBehaviour
{
    private Camera _camera;
    public Camera gameCamera;

    public void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (_camera.orthographicSize != gameCamera.orthographicSize)
        {
            _camera.orthographicSize = gameCamera.orthographicSize;
        }
    }
}
