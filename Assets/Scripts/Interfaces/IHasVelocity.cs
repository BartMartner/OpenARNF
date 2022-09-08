using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasVelocity
{
    Transform transform { get; }
    Vector2 velocity { get; set; }
}
