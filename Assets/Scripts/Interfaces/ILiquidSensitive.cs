using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILiquidSensitive
{
    GameObject gameObject { get; }
    bool OnEnterLiquid(Water water);
    void OnExitLiquid();
    bool inLiquid { get; set; }
    bool electrifiesWater { get; }
}
