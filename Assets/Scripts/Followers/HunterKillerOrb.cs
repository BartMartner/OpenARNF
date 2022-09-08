using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterKillerOrb : TrailFollower
{
    public Enemy enemy;
    public AudioClip spotSound;

    private float _timer;
    private bool _coolDown;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();        
    }

    public override void Update()
    {
        if(!enemy)
        {
            base.Update();

            if (!_coolDown)
            {
                enemy = EnemyManager.instance.GetClosest(transform.position, 6);                
                if (enemy)
                {
                    _audioSource.PlayOneShot(spotSound);
                }
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, enemy.position, 9 * Time.deltaTime);

            if(enemy.state != DamageableState.Alive || enemy.notTargetable)
            {
                enemy = null;
                StartCoroutine(CoolDown());
            }
        }

        _animator.SetBool("Attacking", enemy != null);
    }

    private IEnumerator CoolDown()
    {
        _coolDown = true;
        yield return new WaitForSeconds(2.5f);
        _coolDown = false;
    }
}
