using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathFindingSensitive
{
    GameObject gameObject { get; }
    bool pathFindingSensitive { get; }
    Action pfTryRefresh { get; set; }
    Collider2D[] collider2Ds { get; }
}
