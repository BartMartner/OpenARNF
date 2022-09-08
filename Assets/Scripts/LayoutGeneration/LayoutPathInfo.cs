using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LayoutPathInfo
{
    public int index;
    public Direction primaryDirection;
    public EnvironmentType environment;

    public LayoutPathInfo() { }

    public LayoutPathInfo(LayoutPathInfo original)
    {
        index = original.index;
        primaryDirection = original.primaryDirection;
        environment = original.environment;
    }
}
