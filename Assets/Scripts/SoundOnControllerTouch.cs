using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnControllerTouch : MonoBehaviour
{
    private BoundsCheck _boundsCheck;
    public AudioClip sound;

    private void Awake()
    {
        _boundsCheck = GetComponent<BoundsCheck>();
    }

    void Update()
    {
        if(_boundsCheck.AnyJustTouched())
        {
            AudioManager.instance.PlayClipAtPoint(sound, transform.position);
        }
    }
}
