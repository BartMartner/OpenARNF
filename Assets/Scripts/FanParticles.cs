using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanParticles : MonoBehaviour, IAbstractDependantObject
{
    new public ParticleSystem particleSystem;
    public ParticleSystem waterParticles;
    public ForceBounds forceBounds;

    public int m_priority { get; set; }

    private bool _water = false;

    public void Start()
    {
        MatchForceBounds();
    }

    public void MatchForceBounds()
    {
        if(!particleSystem) particleSystem = GetComponent<ParticleSystem>();

        if (forceBounds)
        {
            var main = particleSystem.main;
            main.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            var speed = _water ? forceBounds.maxVelocity * 0.5f : forceBounds.maxVelocity;
            main.startSpeed = speed;
            var collider = forceBounds.GetComponent<BoxCollider2D>();
            if (collider)
            {
                main.startLifetime = collider.size.y / speed;
                
                var rateOverTime = 30 * (collider.size.y / speed);
                if (_water) rateOverTime *= 2;
                var emission = particleSystem.emission;
                emission.rateOverTime = rateOverTime;

                var maxParticles = Mathf.CeilToInt(emission.rateOverTime.constantMax * main.startLifetime.constantMax) * 2;
                if (_water) maxParticles *= 4;
                main.maxParticles = maxParticles;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        MatchForceBounds();
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(roomAbstract.environmentalEffect == EnvironmentalEffect.Underwater)
        {
            _water = true;
            Destroy(particleSystem);
            waterParticles.gameObject.SetActive(true);
            particleSystem = waterParticles;
            MatchForceBounds();
        }
    }
}
