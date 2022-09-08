using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Player
{
    private StatusEffect _toxinEffect;
    private int _wadjetKillCount;

    [Header("Cloaking")]
    public Material cloakMaterial;
    public AudioClip cloakSound;
    public AudioClip decloakSound;
    public bool cloaked { get; private set; }

    private Dictionary<int, Material> _followerMaterials = new Dictionary<int, Material>();
    public List<TemporaryStatMod> tempStatMods = new List<TemporaryStatMod>();
    private int _currentModFlash = 0;

    public void HiveShell()
    {
        StartCoroutine(WaitSpawnNanobot());
    }

    public IEnumerator WaitSpawnNanobot()
    {
        yield return new WaitForSeconds(0.25f);
        if (AudioManager.instance) { AudioManager.instance.PlayClipAtPoint(AudioManager.instance.tweep03, transform.position); }
        SpawnNanobot(transform.position);
    }

    public void BoltHelm(ProjectileStats stats, AimingInfo aimingInfo, int arcShots, float fireArc)
    {
        AudioManager.instance.PlayClipAtPoint(attackSound, helmPoint.position, 0.5f);
        if (arcShots > 0)
        {
            ProjectileManager.instance.ArcShoot(stats, helmPoint.position, aimingInfo.direction, arcShots, fireArc);
        }
        else
        {
            ProjectileManager.instance.Shoot(stats, new AimingInfo(helmPoint.position, aimingInfo.direction));
        }
    }

    public void PowerJump()
    {
        var burst = Instantiate(Resources.Load<BaseEvent>("ItemFX/JumpBurst"), controller2D.bottomMiddle, Quaternion.identity);
        var damage = burst.GetComponentInChildren<DamageCreatureTrigger>(true);
        damage.damage = 5 * damageMultiplier;
        if (team != Team.Player) { Constants.SetCollisionForTeam(damage.collider2D, team); }
        burst.StartEvent();
        burst.onEventEnd.AddListener(() => Destroy(burst.gameObject));
    }

    public void ToxinPauldrons()
    {
        if (_toxinEffect == null) { _toxinEffect = StatusEffect.CopyOf(ResourcePrefabManager.instance.LoadStatusEffect("StatusEffects/NormalPoison")); }
        ProjectileManager.instance.SpawnDamageCloud(_toxinEffect, this, 3, team, 5);
    }

    public void HiveBolt(Enemy enemy)
    {
        if (enemy == null)
        {
            SpawnNanobot(transform.position);
        }
        else
        {
            SpawnNanobot(enemy.position);
        }
    }

    public void ShieldBolt(Enemy enemy)
    {
        _wadjetKillCount++;
        if (_wadjetKillCount >= 7) { AddTempShield(15); }
    }

    public void AddTempShield(float time)
    {
        var tempShield = gameObject.AddComponent<TemporaryShield>();
        tempShield.Equip(this, time);
    }

    public void AddTempStatMod(PlayerStatType statType, float rank)
    {
        var sign = Mathf.Sign(rank);
        TemporaryStatMod statMod = tempStatMods.FirstOrDefault(s => s.statType == statType && Mathf.Sign(s.rank) == sign);
        if (!statMod) { statMod = gameObject.AddComponent<TemporaryStatMod>(); }
        statMod.Equip(this, statType, rank);
    }

    public void Cloak()
    {
        cloaked = true;
        mainRenderer.material = cloakMaterial;
        SetDefaultFlashColor(Color.black, 0.25f);
        StartFlash(1, 1, Color.white, 1, true);
        notTargetable = true;
        PlayOneShot(cloakSound);
        foreach (var f in followers)
        {
            if (f.mainRenderer)
            {
                _followerMaterials[f.GetInstanceID()] = f.mainRenderer.material;
                f.mainRenderer.material = cloakMaterial;
            }
        }
    }

    public void Decloak(bool sound)
    {
        cloaked = false;
        mainRenderer.material = _defaultMaterial;
        notTargetable = false;
        SetDefaultFlashColor(Color.white, 0);
        
        if (sound)
        {
            StartFlash(1, 1, Color.white, 1, true);
            PlayOneShot(decloakSound);
        }

        Material m;
        foreach (var f in followers)
        {
            if (f && f.mainRenderer && _followerMaterials.TryGetValue(f.GetInstanceID(), out m))
            {
                f.mainRenderer.material = m;
            }
        }
        _followerMaterials.Clear();
    }
}
