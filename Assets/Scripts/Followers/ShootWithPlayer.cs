using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWithPlayer : Shooter
{
    public bool attacking;
    public float attackDelay = 0.4f;
    public Player player;

    protected override void Start()
    {
        base.Start();
        if(!player)
        {
            StartCoroutine(WaitForPlayer());
        }
    }

    public IEnumerator WaitForPlayer()
    {
        var follower = GetComponent<Follower>();
        if (follower)
        {
            while (!follower.player)
            {
                yield return null;
            }

            player = follower.player;
            projectileStats.team = player.team;
        }
        else
        {
            player = PlayerManager.instance.player1;
        }
    }

    public void Update()
    {
        if(LayoutManager.instance && LayoutManager.instance.transitioning) { return; }

        if (player.state == DamageableState.Alive)
        {
            bool attackDown = player.controller.GetButton(player.attackString);
            if (!attacking && attackDown)
            {
                StartCoroutine(Attack());
            }
        }
    }

    public IEnumerator Attack()
    {
        attacking = true;

        Shoot();

        var timer = 0f;
        while (timer < attackDelay)
        {
            timer += player.controller.GetButton(player.attackString) ? Time.deltaTime : Time.deltaTime * 2;
            yield return null;
        }

        attacking = false;
    }
}
