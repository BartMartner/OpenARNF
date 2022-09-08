using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFlameOrigin
{
    Transform origin { get; }
    Vector3 target { get; }
    FlamethrowerFlame GetFlame();
}
