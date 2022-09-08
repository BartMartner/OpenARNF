using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSpitter : BaseMovementBehaviour
{
    public float revealRange = 6;
    public string showAnim = "Show";
    public string hideAnim = "Hide";
    public TimedShooter shooter;

    private Animator _animator;
    private bool _busy;
    private bool _hidden = true;
    private float coolDown;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(_busy || shooter.shooting) { return; }

        if(coolDown > 0)
        {
            coolDown -= Time.deltaTime;
            return;
        }

        var closestPlayer = PlayerManager.instance.GetClosestPlayer(transform.position);
        var distance = closestPlayer ? Vector3.Distance(transform.position, closestPlayer.position) : 99;
        if (_hidden)
        {
            if (distance < revealRange) { StartCoroutine(Show()); }
        }
        else
        {
            if (distance > revealRange) { StartCoroutine(Hide()); }
        }
    }

    private IEnumerator Show()
    {
        _busy = true;
        _hidden = false;
        _animator.Play(showAnim);
        yield return new WaitForSeconds(1);
        _busy = false;
        shooter.gameObject.SetActive(true);
        coolDown = 2;
    }

    private IEnumerator Hide()
    {
        _busy = true;
        _animator.Play(hideAnim);
        yield return new WaitForSeconds(1);
        _hidden = true;
        _busy = false;
        shooter.gameObject.SetActive(false);
        coolDown = 2;
    }
}
