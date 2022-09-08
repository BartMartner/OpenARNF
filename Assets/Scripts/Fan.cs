using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    public GameObject fanParticles;
    public GameObject force;
    public GameObject damage;
    public SimpleAnimator simpleAnimator;
    public bool on = true;

    public void Start()
    {
        Set(on);        
    }

    public void Toggle()
    {
        on = !on;
        Set(on);
    }

    public void Set(bool on)
    {
        fanParticles.SetActive(on);
        force.SetActive(on);
        damage.SetActive(on);
        simpleAnimator.Reset();
        simpleAnimator.enabled = on;
    }
}
