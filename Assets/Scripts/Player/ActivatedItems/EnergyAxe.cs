using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnergyAxe", menuName = "Player Activated Items/Energy Axe", order = 1)]
public class EnergyAxe : PlayerActivatedItem
{
    public AudioClip shootSound;
    public ProjectileStats stats;
    public float verticalAdjust = 6f;
    private bool _active;
    private IEnumerator _coroutine;
    private ProjectileStats _statsCopy;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        _statsCopy = new ProjectileStats(stats);
        _statsCopy.team = player.team;
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (!_active && Usable())
        {
            _player.energy -= energyCost;
            _coroutine = AttackCoroutine();
            _player.StartCoroutine(_coroutine);            
        }
    }

    public IEnumerator AttackCoroutine()
    {
        _active = true;
        var direction = new Vector2(_player.transform.right.x, _player.gravityFlipped ? -verticalAdjust : verticalAdjust).normalized;

        if (shootSound)
        {
            _player.PlayOneShot(shootSound);
        }

        _statsCopy.damage = stats.damage * _player.damageMultiplier;
        _statsCopy.gravity = _player.gravityFlipped ? -stats.gravity : stats.gravity;
        _statsCopy.size = _player.projectileStats.size + _player.projectileStats.sizePerSecond * 0.5f;
        ProjectileManager.instance.Shoot(_statsCopy, _player.transform.position, direction);
        yield return new WaitForSeconds(_player.attackDelay * 0.5f);

        _coroutine = null;
        _active = false;
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if (_coroutine != null)
        {
            _player.StopCoroutine(_coroutine);
        }
        return base.Unequip(position, parent, setJustSpawned);
    }
}
