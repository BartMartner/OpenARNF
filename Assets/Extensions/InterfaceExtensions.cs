using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public static partial class Extensions
{
    public static IEnumerable<T> GetInterfaces<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return Enumerable.Empty<T>();
        }

        return inObj.GetComponents<Component>().OfType<T>();
    }

    public static IEnumerable<T> GetInterfacesInChildren<T>(this GameObject inObj, bool includeInactive = false) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            Debug.LogError(typeof(T).ToString() + ": is not an actual interface!");
            return Enumerable.Empty<T>();
        }

        return inObj.GetComponentsInChildren<Component>(includeInactive).OfType<T>();
    }
}
