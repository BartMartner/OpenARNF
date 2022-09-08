using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaBeastComponent : Damageable
{
    public MegaBeast megaBeast;
    private BaseEvent _eventAction;
    public BaseEvent eventAction
    {
        get { return _eventAction; }
    }

    public bool canAct
    {
        get
        {
			return state == DamageableState.Alive && (!_monsterSpawner || _monsterSpawner.maxMonsters <= 0 ||
				_monsterSpawner.monstersSpawned < _monsterSpawner.maxMonsters * maxMonsterThreshold);
        }
    }

    public bool triggerEventAction;
    public Vector3 localOffset;

    [Range (0,1)]
    public float maxMonsterThreshold = 1;

    private AdvancedMonsterSpawner _monsterSpawner;

    protected override void Awake()
    {
        base.Awake();

        _eventAction = GetComponent<BaseEvent>();
        _monsterSpawner = GetComponentInChildren<AdvancedMonsterSpawner>();
    }

    protected override void Start()
    {
        base.Start();

        megaBeast = GetComponentInParent<MegaBeast>();
    }

    protected override void Update()
    {
        base.Update();

        if (_eventAction != null && triggerEventAction)
        {
            triggerEventAction = false;
            TriggerEventAction();
        }
    }

    public void TriggerEventAction()
    {
        if (!_eventAction.eventCycleAtive)
        {
            _eventAction.StartEvent();
        }
    }
}
