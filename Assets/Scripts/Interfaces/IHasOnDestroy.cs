using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasOnDestroy
{
    Action onDestroy { get; set; }
    Transform transform { get; }
    void OnDestroy();
}
