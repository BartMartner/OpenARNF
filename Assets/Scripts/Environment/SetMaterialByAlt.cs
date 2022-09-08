using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialByAlt : MonoBehaviour
{
    public List<Material> materials;

	private IEnumerator Start ()
    {
        var room = GetComponentInParent<Room>();

        if (room)
        {
            while (room.roomAbstract == null)
            {
                yield return null;
            }

            if (room.roomAbstract != null && room.roomAbstract.altPalette > 0 && materials.Count > room.roomAbstract.altPalette - 1)
            {
                var sr = GetComponent<SpriteRenderer>();
                sr.material = materials[room.roomAbstract.altPalette - 1];
            }
        }
	}
}
