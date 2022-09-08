using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "ToxinCloud", menuName = "Player Activated Items/Toxin Cloud", order = 1)]
public class ToxinCloud : PlayerActivatedItem
{
    public ActivatedScreenEffect toxinCloudPrefab;
    public float damage;
    public StatusEffect statusEffect;
    private ActivatedScreenEffect _toxinCloudInstance;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _toxinCloudInstance = Instantiate(toxinCloudPrefab);
        SceneManager.MoveGameObjectToScene(_toxinCloudInstance.gameObject, _player.gameObject.scene);
        _toxinCloudInstance.onActivate.AddListener(DamageAllEnemies); 
    }

    public void DamageAllEnemies()
    {
        EnemyManager.instance.HurtAllEnemies(damage * _player.damageMultiplier);
        var copy = StatusEffect.CopyOf(statusEffect);
        copy.amount *= _player.damageMultiplier;
        EnemyManager.instance.StatusEffectAllEnemies(copy, _player.team);
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (!_toxinCloudInstance.active && Usable())
        {
            _player.energy -= energyCost;
            _toxinCloudInstance.Activate();
        }
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        _toxinCloudInstance.shouldDestroy = true;
        return base.Unequip(position, parent, setJustSpawned);
    }

    public override bool Usable()
    {
        return _player.energy >= energyCost && !_toxinCloudInstance.active;
    }

    public void OnDestroy()
    {
        if (_toxinCloudInstance) { Destroy(_toxinCloudInstance); }
    }
}
