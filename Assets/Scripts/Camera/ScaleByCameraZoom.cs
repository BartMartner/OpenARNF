using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleByCameraZoom : MonoBehaviour
{
    private Vector3 _originalScale;
    private float _lastScale = 1;

    public void Awake()
    {
        _originalScale = transform.localScale;
    }

	void LateUpdate ()
    {
        if (PlayerManager.instance && _lastScale != PlayerManager.instance.mainCamera.currentZoomScale)
        {
            transform.localScale = _originalScale * PlayerManager.instance.mainCamera.currentZoomScale;
            _lastScale = PlayerManager.instance.mainCamera.currentZoomScale;
        }
	}
}
