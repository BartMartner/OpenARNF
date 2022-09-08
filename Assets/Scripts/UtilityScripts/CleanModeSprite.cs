using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanModeSprite : MonoBehaviour
{
    public Sprite cleanSprite;
	
	public void Awake ()
    {
#if !CLEAN && !UNITY_SWITCH
        if (SaveGameManager.activeSlot != null && SaveGameManager.activeSlot.blood == false)
#endif
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer)
            {
                renderer.sprite = cleanSprite;
            }
        }
	}
}
