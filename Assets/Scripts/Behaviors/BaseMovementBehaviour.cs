using UnityEngine;
using System.Collections;
using System;

public class BaseMovementBehaviour : MonoBehaviour, IPausable, IReactsToStatusEffect
{
    protected bool _paused;
    public bool neverPause;
    private Damageable _damageable;
    private Enemy _enemy;
    protected float _slowMod = 1;

    protected virtual void Start()
    {
        _damageable = GetComponent<Damageable>();
        if (_damageable) { _damageable.onStartDeath.AddListener(EnableFalse); }

        _enemy = GetComponent<Enemy>();
        if (_enemy)
        {
            _enemy.onSpawnStart.AddListener(EnableFalse);
            //maybe this should be _startedEnabled? No, because if a scripts not enabled on start Start() won't get called before onSpawnEnd
            _enemy.onSpawnEnd.AddListener(SpawnEnd);
        }        
    }

    public virtual void Pause()
    {
        if (enabled && !neverPause)
        {
            _paused = true;
            enabled = false;
        }
    }

    public virtual void Unpause()
    {
        if (_paused)
        {
            enabled = true;
        }
    }

    public virtual void OnEnable()
    {
        _paused = false;
    }

    private void OnDestroy()
    {
        if (_damageable) { _damageable.onStartDeath.RemoveListener(EnableFalse); }

        if (_enemy)
        {
            _enemy.onSpawnStart.RemoveListener(EnableFalse);
            //maybe this should be _startedEnabled? No, because if a scripts not enabled on start Start() won't get called before onSpawnEnd
            _enemy.onSpawnEnd.RemoveListener(SpawnEnd);
        }
    }

    private void EnableFalse() { enabled = false; }

    private void SpawnEnd()
    {
        if (_enemy) { enabled = _enemy.state == DamageableState.Alive; }
    }

    public void OnAddEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = effect.amount; }
    }

    public void OnRemoveEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = 1; }
    }

    public void OnStackEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = Mathf.Clamp(_slowMod * effect.amount, 0.33f, 1); }
    }
}
