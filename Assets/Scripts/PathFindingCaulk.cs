using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingCaulk : MonoBehaviour, IPathFindingSensitive
{
    public bool pathFindingSensitive { get { return true; } }
    public Action pfTryRefresh { get; set; }
    private Collider2D[] _collider2Ds;
    public Collider2D[] collider2Ds
    {
        get
        {
            if (_collider2Ds == null) { _collider2Ds = GetComponentsInChildren<Collider2D>(); }
            return _collider2Ds;
        }
    }
}
