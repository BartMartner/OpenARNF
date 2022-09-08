using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchBGCamera : MonoBehaviour
{
    private Camera _camera;
    public Camera gameCamera;

    public void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        _camera.enabled = !LayoutManager.instance || LayoutManager.instance.currentEnvironment == EnvironmentType.Glitch;

        if (_camera.orthographicSize != gameCamera.orthographicSize)
        {
            _camera.orthographicSize = gameCamera.orthographicSize;
        }
    }
}
