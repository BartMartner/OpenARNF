using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SearchBurst", menuName = "Player Activated Items/Search Burst", order = 1)]
public class SearchBurst : PlayerActivatedItem
{
    public AudioClip burstSound;
    public GameObject pulsePrefab;

    private bool _active;
    private IEnumerator _coroutine;    

    public override void Initialize(Player player)
    {
        base.Initialize(player);
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (!_active && Usable())
        {
            _player.energy -= energyCost;
            _coroutine = BurstCoroutine();
            _player.StartCoroutine(_coroutine);
        }
    }

    public IEnumerator BurstCoroutine()
    {
        _active = true;

        if (burstSound) { _player.PlayOneShot(burstSound); }
        Instantiate(pulsePrefab, _player.transform.position, Quaternion.identity, _player.transform.parent);

        yield return new WaitForSeconds(0.33f);
        Automap.instance.RevealMap(_player.gridPosition, 1, true);
        yield return new WaitForSeconds(0.33f);
        Automap.instance.RevealMap(_player.gridPosition, 2, true);
        yield return new WaitForSeconds(0.33f);
        Automap.instance.RevealMap(_player.gridPosition, 3, true);

        SaveGameManager.instance.Save();
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
