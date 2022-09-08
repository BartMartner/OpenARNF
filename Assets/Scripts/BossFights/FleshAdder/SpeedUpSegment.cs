using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUpSegment : MonoBehaviour
{
    private ChildDamagable _childDamagable;
    new public GameObject light;
    private ProjectileDeflector _deflector;
    private DamageCreatureTrigger _damageBounds;

    public void Awake()
    {
        _childDamagable = GetComponent<ChildDamagable>();
        _deflector = GetComponent<ProjectileDeflector>();
        _damageBounds = GetComponentInChildren<DamageCreatureTrigger>();
    }

    public void OnSpeedUpStart()
    {
        _childDamagable.enabled = false;
        light.SetActive(true);
        _deflector.enabled = true;
        _damageBounds.damage = 3;
    }

    public void OnSpeedUpEnd()
    {
        _childDamagable.enabled = true;
        light.SetActive(false);
        _deflector.enabled = false;
        _damageBounds.damage = 2;
    }
}
