using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RadialBolts", menuName = "Player Activated Items/Radial Bolts", order = 1)]
public class RadialBolts : PlayerActivatedItem
{
    public AudioClip shootSound;

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
        var stats = new ProjectileStats(_player.projectileStats);
        stats.canOpenDoors = false;
        _active = true;

        var shotsFired = 0;
        var burstCount = 24;
        var burstArc = 360f;
        var burstTime = 2;

        while (shotsFired < burstCount)
        {
            shotsFired++;

            float angleMod = (((float)shotsFired / (burstCount - 1f)) * 2f) - 1f;
            Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * burstArc / 2, Vector3.forward) * Vector2.up).normalized;

            _player.PlayOneShot(shootSound, 0.33f);

            ProjectileManager.instance.Shoot(stats, _player.transform.position, shotDirection);            

            yield return new WaitForSeconds(burstTime / burstCount);
        }

        _coroutine = null;
        _active = false;
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if(_coroutine != null)
        {
            _player.StopCoroutine(_coroutine);
        }
        return base.Unequip(position, parent, setJustSpawned);
    }
}
