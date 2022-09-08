using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILaser
{
    Transform transform { get; }
    GameObject gameObject { get; }
    bool managed { get; set; }    

    void AttachTo(IHasOnDestroy attachTo);
    void AssignStats(LaserStats stats);
    void Stop();
    void ImmediateStop();
}
