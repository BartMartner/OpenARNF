using System;
using System.Collections.Generic;
using System.Linq;

public static partial class Extensions
{
    public static bool HasFlag(this DamageType keys, DamageType flag)
    {
        return (keys & flag) == flag;
    }

    public static IEnumerable<DamageType> GetFlags(this DamageType keys)
    {
        return Enum.GetValues(typeof(DamageType)).Cast<DamageType>().Where(f => keys.HasFlag(f));
    }
}