using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDestructibleDamageType : RoomBasedRandom
{
    public int moreVariance = 2;

    public override void Randomize()
    {
        base.Randomize();

        DamageType damageType = DamageType.Generic;
        if(_room.roomAbstract != null && LayoutManager.instance && LayoutManager.instance.layout != null)
        {            
            var traveresalCaps = LayoutManager.instance.layout.traversalCapabilities;
            var index = Mathf.Clamp(_room.roomAbstract.expectedCapabilitiesIndex + moreVariance, 0, traveresalCaps.Count-1);
            Debug.Log("Rolled capabilites " + index);

            for (int i = index; i >= 0; i--)
            {
                var last = traveresalCaps[i].lastGainedAffordance;
                if(last.requiredDamageType != 0 && last.requiredDamageType != DamageType.Generic)
                {
                    damageType = last.requiredDamageType;
                    Debug.Log("Chose damageType " + damageType + " from capabilites " + i);
                    break;
                }
            }            
        }

        if (damageType != 0 && damageType != DamageType.Generic)
        {
            var tiles = gameObject.GetInterfacesInChildren<ISetByDamageType>();
            foreach (var t in tiles)
            {
                t.SetByDamageType(damageType);
            }
        }
    }
}
