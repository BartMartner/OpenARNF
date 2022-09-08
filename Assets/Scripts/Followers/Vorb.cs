using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vorb : ObjectNabber
{
    public AudioClip munchSound;
    public AudioClip spawnSound;
    public ParticleSystem particles;
    public int gibsNeeded = 30;
    public DropType dropType;

    private int _objectsNabbed;
    private int _objectsSpawned;
    private GibType[] _validTypes = new GibType[] { GibType.Meat, GibType.PaleMeat, GibType.PinkMeat, GibType.GreenMeat };
    private float _munchTimer;

    public override GameObject GetClosestObject()
    {
        var g = GibManager.instance.GetClosestGib(transform.position, spotDistance, _validTypes, true);
        if(g) { g.vorbTargeted = true; }
        return g ? g.gameObject : null;
    }

    public override void Update()
    {
        base.Update();
        if(_munchTimer > 0) { _munchTimer -= Time.unscaledDeltaTime; }
    }

    public override void OnObjectReached()
    {
        var gib = _targetObject.GetComponent<Gib>();

        if(gib)
        {
            gib.Recycle();
        }
        else
        {
            Destroy(_targetObject.gameObject);
        }

        _targetObject = null;

        StartCoroutine(JustCollected(0.25f));

        if (_munchTimer <= 0)
        {
            particles.Play();
            _audioSource.PlayOneShot(munchSound);
            _munchTimer = munchSound.length;
        }

        _objectsNabbed++;

        if (_objectsNabbed >= gibsNeeded)
        {
            _objectsNabbed = 0;
            StartCoroutine(SpawnObject());
        }
    }

    private IEnumerator SpawnObject()
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

        _audioSource.PlayOneShot(spawnSound);
        _animator.Play("Spawn");

        yield return new WaitForSeconds(10f / 12f);

        PickUpManager.instance.SpawnPickUp(dropType, transform.position);

        _objectsSpawned++;
        _noMove = false;
    }
}
