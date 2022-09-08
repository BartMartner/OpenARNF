using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShooter : MonoBehaviour, IHasOnDestroy
{
    public LaserStats stats;
    public float duration;
    private ILaser _iLaser;
    public Action onDestroy { get; set; }

    public void Shoot()
    {
        if(_iLaser != null) { Stop(); }
        _iLaser = LaserManager.instance.AttachAndFireLaser(stats, Vector3.zero, transform.rotation, duration, this);
    }

    public void Stop()
    {
        if (_iLaser != null)
        {
            _iLaser.Stop();
            _iLaser = null;
        }
    }

    public void OnDestroy()
    {
        if (onDestroy != null) { onDestroy(); }
    }

    public void DestroySelf() { Destroy(gameObject); }
}
