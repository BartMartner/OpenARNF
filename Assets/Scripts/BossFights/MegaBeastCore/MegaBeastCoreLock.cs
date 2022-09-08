using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaBeastCoreLock : MonoBehaviour
{
    public Shooter sineShooter1;
    public Shooter sineShooter2;

    public SequencedShooters arcSequence;

    public Shooter spiralShooter;

    private Enemy _enemy;
    private Animator _animator;

    public bool notDead
    {
        get
        {
            return _enemy && _enemy.state != DamageableState.Dead; 
        }
    }

    public void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _animator = GetComponent<Animator>();
    }

    public bool shooting
    {
        get
        {
            return sineShooter1.shooting || sineShooter2.shooting || arcSequence.shooting || spiralShooter.shooting;
        }
    }

    public void RandomShot()
    {
        StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        _animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);

        int pick = Random.Range(0, 3);
        switch (pick)
        {
            case 0:
                sineShooter1.Shoot();
                sineShooter2.Shoot();
                break;
            case 1:
                arcSequence.Shoot();
                break;
            case 2:
                spiralShooter.Shoot();
                break;
        }
    }
}
