using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TheThief : ObjectNabber
{
    public const string itemsSpawnedKey = "thfItms";
    public GameObject projectorPrefab;
    private int _pickUpsDestroyed;
    private int _itemsSpawned;

    public override IEnumerator Start()
    {
        if (SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeGame.otherInts.TryGetValue(itemsSpawnedKey, out _itemsSpawned);
        }

        return base.Start();
    }

    public override GameObject GetClosestObject()
    {
        var p = PickUpManager.instance.GetClosestPickUp(transform.position, spotDistance);
        return p ? p.gameObject : null;
    }

    public override void OnObjectReached()
    {
        var pickUp = _targetObject.GetComponent<PickUp>();
        if (pickUp && pickUp.pickUpSound)
        {
            _audioSource.PlayOneShot(pickUp.pickUpSound);
        }

        base.OnObjectReached();

        _pickUpsDestroyed++;

        if(_pickUpsDestroyed >= 15)
        {
            _pickUpsDestroyed = 0;
            StartCoroutine(SpawnModule());
        }
    }

    private IEnumerator SpawnModule()
    {
        _noMove = true;

        var velocity = Vector3.zero;
        _targetPosition = player.transform.position + Vector3.up * _distance;

        while (Vector3.Distance(transform.position, _targetPosition) > 0.5f)
        {
            _targetPosition = player.transform.position + Vector3.up * _distance;
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref velocity, 1f, 48, Time.deltaTime);
            yield return null;
        }

        _animator.Play("Spawn");

        yield return new WaitForSeconds(10f / 12f);

        var badChance = Mathf.Clamp((_itemsSpawned - 3) * 0.2f, 0, 0.5f);
        if (UnityEngine.Random.value < badChance)
        {
            var room = LayoutManager.CurrentRoom ?? FindObjectOfType<Room>();
            if (room)
            {
                Instantiate(projectorPrefab, transform.position, Quaternion.identity, room.transform);
            }
        }
        else
        {
            var types = Enum.GetValues(typeof(MinorItemType)).Cast<MinorItemType>().ToList();
            types.Remove(MinorItemType.None);
            types.Remove(MinorItemType.RedScrap);
            types.Remove(MinorItemType.GreenScrap);
            types.Remove(MinorItemType.BlueScrap);

            var type = types[UnityEngine.Random.Range(0, types.Count)];

            var prefab = ResourcePrefabManager.instance.LoadGameObject("PickUps/" + type.ToString()).GetComponent<MinorItemPickUp>(); ;
            var minorItem = Instantiate(prefab, transform.position, Quaternion.identity, transform.parent) as MinorItemPickUp;
            minorItem.data.globalID = -99;
        }

        if (FXManager.instance)
        {
            FXManager.instance.SpawnFX(FXType.AnimeSplode, transform.position);
        }

        _itemsSpawned++;
        _noMove = false;

        if (SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeGame.otherInts[itemsSpawnedKey] = _itemsSpawned;
            SaveGameManager.instance.Save();
        }
    }
}
