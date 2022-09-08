using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LilShrine : ObjectNabber
{
    public Animator effectPrefab;
    protected int _scrapConsumed;
    protected int _holyNumber;
    protected List<IEnumerable> effects = new List<IEnumerable>();

    public override GameObject GetClosestObject()
    {
        var p = PickUpManager.instance.GetClosestScrapDrop(transform.position, spotDistance);
        return p ? p.gameObject : null;
    }

    public override void OnObjectReached()
    {
        var pickUp = _targetObject.GetComponent<PickUp>();
        if (pickUp)
        {
            _scrapConsumed++;
            if (pickUp.pickUpSound)
            {
                _audioSource.PlayOneShot(pickUp.pickUpSound);
            }
        }

        if (_scrapConsumed >= _holyNumber)
        {
            _scrapConsumed -= _holyNumber;
            StartCoroutine(Effect());
        }

        base.OnObjectReached();
    }

    private IEnumerator Effect()
    {
        _noMove = true;
        if (effectPrefab)
        {
            var anim = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform.parent);
            yield return new WaitForSeconds(1.5f);
            Destroy(anim.gameObject);
        }
        var effect = effects[Random.Range(0, effects.Count)];
        yield return StartCoroutine(effect.GetEnumerator());
        _noMove = false;
    }
}
