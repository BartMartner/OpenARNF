using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mothorb : ObjectNabber
{
    public override GameObject GetClosestObject()
    {
        var p = PickUpManager.instance.GetClosestScrapDrop(transform.position, spotDistance);
        return p ? p.gameObject : null;
    }

    public override void Update()
    {
        base.Update();

        if (!_noMove)
        {
            transform.rotation = player.transform.rotation;
        }
    }

    public override void OnObjectReached()
    {
        var pickUp = _targetObject.GetComponent<PickUp>();
        if (pickUp && pickUp.pickUpSound)
        {
            _audioSource.PlayOneShot(pickUp.pickUpSound);
        }

        base.OnObjectReached();

        StartCoroutine(SpawnBot());
    }

    private IEnumerator SpawnBot()
    {
        _animator.Play("Spawn");
        _noMove = true;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(2f / 12f);
            player.SpawnNanobot(transform.position);
        }
        
        yield return new WaitForSeconds(2f/12f);
        _noMove = false;
    }
}
