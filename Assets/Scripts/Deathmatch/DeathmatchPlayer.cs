using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeathmatchPlayer : Player, IHasTeam
{
    public bool respawning;
    private ProjectileStats _startingProjectileStats;

    protected override void Start()
    {
        _startingHealth = 14;
        _startingEnergy = 14;

        base.Start();

        aegisTime = 0.05f;
        _startingProjectileStats = new ProjectileStats(projectileStats);
    }

    public void ClearItems()
    {
        healthUps = 0;
        energyUps = 0;
        damageUps = 0;
        attackUps = 0;
        speedUps = 0;
        shotSpeedUps = 0;

        //get rid of bolt helm and other items that add event listeners
        var toRemove = _listenersApplied.ToArray();
        foreach (var item in toRemove) { RemoveItem(item, false, false); }

        itemsPossessed.Clear();
        _itemsApplied.Clear();

        projectileStats = new ProjectileStats(_startingProjectileStats);

        Destroy(activatedItem);
        activatedItem = null;

        _selectedEnergyWeapon = null;
        foreach (var weapon in energyWeapons)
        {
            Destroy(weapon);
        }
        energyWeapons.Clear();

        _activeSpecialMove = null;
        foreach (var move in specialMoves)
        {
            Destroy(move);
        }
        specialMoves.Clear();

        foreach (var burst in hurtBursts)
        {
            Destroy(burst.gameObject);
        }
        hurtBursts.Clear();
        itemHurtActions.Clear();

        foreach (var e in jumpEvents)
        {
            Destroy(e.gameObject);
        }
        jumpEvents.Clear();

        foreach (var f in followers)
        {
            if (f) { Destroy(f.gameObject); }
        }

        followers.Clear();
        orbitalFollowerCount = 0;
        trailFollowerCount = 0;

        MatchItems(true); //calls ResetAbilities();

        mainRenderer.material = material;
    }

    public override void Respawn(bool fatigue = true)
    {
        base.Respawn(fatigue);

        _collider2D.enabled = true;
        FXManager.instance.SpawnFX(FXType.Teleportation, transform.position, false, false, transform.rotation != Quaternion.identity);
        energy = maxEnergy;
        health = maxHealth;
        fatigued = false;
    }

    public override IEnumerator DeathRoutine()
    {
        if (DeathmatchManager.instance) { DeathmatchManager.instance.deaths[team]++; }

        if (_activeSpecialMove != null)
        {
            _activeSpecialMove.DeathStop();
        }

        if (activatedItem)
        {
            Destroy(activatedItem);
        }

        statusEffects.Clear();

        mainRenderer.enabled = true;
        mainRenderer.material.SetColor("_FlashColor", Constants.blasterGreen);

        var timer = 0f;
        var time = 1f;
        while (timer < time)
        {
            var progress = (time - timer) / time;
            timer += Time.unscaledDeltaTime;
            mainRenderer.material.SetFloat("_FlashAmount", progress);
            yield return null;
        }

        DamageLatchers(10000, DamageType.Generic);

        _collider2D.enabled = false;
        mainRenderer.enabled = false;
        DeathmatchManager.instance.SpawnDeathParticles(transform.position);

        if (followers.Count > 0)
        {
            yield return StartCoroutine(DestroyFollowers());
        }
        else
        {
            yield return new WaitForSeconds(1);
        }

        DeathmatchManager.instance.RespawnPlayer(this);
    }

    public IEnumerator DestroyFollowers()
    {
        var count = followers.Count;
        var delay = new WaitForSeconds(1f / count);
        for (int i = count-1; i >= 0; i--)
        {
            var f = followers[i];
            FXManager.instance.SpawnFX(FXType.ExplosionMedium, f.position);
            Destroy(f.gameObject);
            yield return delay;
        }

        followers.Clear();
        orbitalFollowerCount = 0;
        trailFollowerCount = 0;
    }

    public override bool HurtKnocback(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false, bool knockBack = true)
    {
        if (health <= 0) return false;
        var result = base.HurtKnocback(damage, source, damageType, ignoreAegis, knockBack);
        if(result && health <= 0)
        {
            var hasTeam = source.GetComponent<IHasTeam>();
            if(hasTeam == null || !DeathmatchManager.instance.AwardPoints(hasTeam.team, team))
            {
                //if the player dies from a hazard they lose a point
                DeathmatchManager.instance.AwardPoints(team, team);
            }
        }
        return result;
    }

    public override void HandleDamage(float damageAmount)
    {
        damageAmount = Mathf.Clamp(damageAmount * 0.5f, 0, maxHealth * 0.6f);
        base.HandleDamage(damageAmount);
    }
}
