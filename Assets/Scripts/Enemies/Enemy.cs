using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.Events;

public class Enemy : Damageable, ISpawnable, IPausable, IHasTeam
{
    public float spawnTime;
    public UnityEvent onSpawnStart;
    public UnityEvent onSpawnEnd;
    public float aegisOverride;

    public bool selfDestruct;
    private bool _paused;
    private List<IPausable> _pausables;

    private Team _team = Team.Enemy;
    public Team team { get { return _team; } set { _team = value; } }
    public bool isBoss { get; set; }

    protected override void Awake()
    {
        base.Awake();

        _pausables = gameObject.GetInterfacesInChildren<IPausable>().ToList();
        _pausables.Remove(this);
    }

    protected override void Start()
    {
        base.Start();
        EnemyManager.instance.enemies.Add(this);
        aegisTime = aegisOverride;
    }

    public virtual void OnDestroy()
    {
        if (EnemyManager.instance)
        {
            EnemyManager.instance.enemies.Remove(this);
        }
    }

    public void Spawn()
    {
        if (spawnTime > 0) { StartCoroutine(WaitForSpawn()); }
    }

    public IEnumerator WaitForSpawn()
    {
        if (onSpawnStart != null) { onSpawnStart.Invoke(); }
        yield return new WaitForSeconds(spawnTime);
        if (onSpawnEnd != null) { onSpawnEnd.Invoke(); }
    }

    public void Pause()
    {
        if (enabled)
        {
            _paused = true;
            enabled = false;
            if (_pausables != null && _pausables.Count > 0)
            {
                foreach (var p in _pausables)
                {
                    p.Pause();
                }
            }
        }
    }

    public void Unpause()
    {
        if (_paused)
        {
            _paused = false;
            enabled = true;
            if (_pausables != null && _pausables.Count > 0)
            {
                foreach (var p in _pausables) { p.Unpause(); }
            }
        }
    }

    public override bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        var result = base.Hurt(damage, source, damageType, ignoreAegis);        
        if (DeathmatchManager.instance && result && health <= 0)
        {
            Debug.Log(source.name);
            var hasTeam = source.GetComponent<IHasTeam>();
            if (hasTeam != null)
            {
                Debug.Log(hasTeam.team);
                DeathmatchManager.instance.AwardPoints(hasTeam.team, Team.Enemy);
            }
        }
        return result;
    }

    public override void EndDeath()
    {
        base.EndDeath();

        var slot = SaveGameManager.activeSlot;
        var game = SaveGameManager.activeGame;
        if (slot != null && slot.totalKills < long.MaxValue && (game == null || game.allowAchievements))
        {
            slot.totalKills++;
            AchievementManager.instance.CheckForKillAchievements();
        }
    }

    public void SelfDestruct()
    {
        selfDestruct = true;
        StartDeath();
    }

    public virtual void OnEnable()
    {
        _paused = false;
    }
}
