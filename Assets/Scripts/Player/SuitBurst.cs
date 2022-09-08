using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuitBurst : MonoBehaviour
{
    public float duration;
    public AudioClip sound;
    public float damage = 1;

    private Animator _animator;
    private AudioSource _audioSource;
    private bool _bursting;
    private DamageCreatureTrigger _damageTrigger;
    public Player player;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _damageTrigger = GetComponentInChildren<DamageCreatureTrigger>();        
    }

	public void StartBurst()
    {
        if (!_bursting)
        {
            gameObject.SetActive(true);
            StartCoroutine(WaitForBurst());
        }
    }

    public IEnumerator WaitForBurst()
    {
        _bursting = true;

        if(player && _damageTrigger)
        {
            _damageTrigger.team = player.team;
            _damageTrigger.damage = damage * (1f + (player.damageMultiplier - 1f) * 0.5f);
        }

        if(_animator)
        {
            _animator.Play(0);
        }

        if(_audioSource && sound)
        {
            _audioSource.PlayOneShot(sound);
        }

        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
        _bursting = false;
    }
}
