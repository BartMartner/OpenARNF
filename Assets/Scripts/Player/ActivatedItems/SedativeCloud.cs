using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SedativeCloud", menuName = "Player Activated Items/Sedative Cloud", order = 1)]
public class SedativeCloud : PlayerActivatedItem
{
    public ActivatedScreenEffect effectPrefab;
    public float damage;
    public StatusEffect statusEffect;
    private ActivatedScreenEffect _instance;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _instance = Instantiate(effectPrefab);
        SceneManager.MoveGameObjectToScene(_instance.gameObject, _player.gameObject.scene);
        _instance.onActivate.AddListener(SlowAllEnemies);
    }

    public void SlowAllEnemies()
    {
        EnemyManager.instance.StatusEffectAllEnemies(statusEffect, _player.team);
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (!_instance.active && Usable())
        {
            _player.energy -= energyCost;
            _instance.Activate();
        }
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        _instance.shouldDestroy = true;
        return base.Unequip(position, parent, setJustSpawned);
    }

    public override bool Usable()
    {
        return _player.energy >= energyCost && !_instance.active;
    }

    public void OnDestroy()
    {
        if (_instance) { Destroy(_instance); }
    }
}
