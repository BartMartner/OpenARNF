using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveBomb", menuName = "Player Activated Items/Wave Bomb", order = 1)]
public class WaveBomb : PlayerActivatedItem
{
    public AudioClip shootSound;
    public ProjectileStats stats;
    public float verticalAdjust = 1f;
    private bool _active;
    private IEnumerator _coroutine;

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
        var direction = new Vector2(_player.transform.right.x, verticalAdjust).normalized;

        if (shootSound)
        {
            _player.PlayOneShot(shootSound);
        }

        var useStats = new ProjectileStats(stats);
        useStats.team = _player.team;
        useStats.damage = useStats.damage * _player.damageMultiplier;
        ProjectileManager.instance.Shoot(useStats, _player.transform.position, direction);
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
