using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemLiquidToggle : MonoBehaviour, ILiquidSensitive
{
    public ParticleSystem inLiquidParticles;
    public ParticleSystem notInLiquidParticles;

    public bool inLiquid { get; set; }
    public bool electrifiesWater { get { return false; } }

    public bool OnEnterLiquid(Water water)
    {
        inLiquidParticles.Play();
        if (notInLiquidParticles) notInLiquidParticles.Stop();
        return true;
    }

    public void OnExitLiquid()
    {
        inLiquidParticles.Stop();
        if (notInLiquidParticles) notInLiquidParticles.Play();
    }
}
