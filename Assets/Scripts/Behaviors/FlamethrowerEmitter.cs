using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlamethrowerEmitter : MonoBehaviour, IFlameOrigin
{
    public FlamethrowerFlame flamePrefab;
    public AudioClip flameStart;
    public AudioClip flameLoop;
    public bool emitting;
    public bool hitWalls;
    public bool dps;
    public float flameRange = 6f;
    public float damage;
    public Vector3 direction = Vector3.right;
    public Team team;
    public DamageType damageType;

    public Transform origin { get { return transform; } }

    private Vector3 _target;
    public Vector3 target { get { return _target; } }

    private List<FlamethrowerFlame> _flames;
    private float _flameTimer;
    private float _flameInterval;
    private float _flameSpeed = 2f;
    private const float _flameMovingLifespan = 11 / 24f;
    private const float _flameTotalLifespan = 19 / 24f;
    private int _lastSortingOrder;
    private AudioSource _audioSource;

    public void Awake()
    {
        _flames = new List<FlamethrowerFlame>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }

    public void Update()
    {
        if (emitting)
        {
            _flameSpeed = flameRange / _flameMovingLifespan;
            _flameInterval = _flameMovingLifespan / flameRange;
            _target = transform.position + transform.TransformDirection(direction) * flameRange;
            _flameTimer -= Time.deltaTime;
            if (_flameTimer <= 0)
            {
                SpawnFlame();
                _flameTimer = _flameInterval;
            }
        }
        else
        {
            _flameTimer = 0f;
            _lastSortingOrder = 0;
        }
    }

    public void SpawnFlame()
    {
        _lastSortingOrder++;
        var flame = GetFlame();
        flame.Spawn(_lastSortingOrder, _flameSpeed, _flameMovingLifespan, _flameTotalLifespan, team, dps);
    }

    public FlamethrowerFlame GetFlame()
    {
        var flame = _flames.FirstOrDefault(f => f.gameObject.activeInHierarchy == false);
        if (!flame)
        {
            flame = Instantiate(flamePrefab);
            flame.owner = this;
            flame.player = null;
            flame.hitWalls = hitWalls;
            flame.transform.parent = transform;
            var damageTrigger = flame.GetComponent<DamageCreatureTrigger>();
            damageTrigger.damage = damage;
            damageTrigger.damageType = damageType;
            _flames.Add(flame);
        }

        return flame;
    }

    public void Emit()
    {
        emitting = true;
        if (_audioSource && flameStart && flameLoop)
        {
            _audioSource.PlayOneShot(flameStart);
            _audioSource.clip = flameLoop;
            _audioSource.PlayScheduled(flameStart.length - 0.5f);
        }
    }

    public void Stop()
    {
        if (_audioSource)
        {
            _audioSource.Stop();
        }
        emitting = false;
    }
}
