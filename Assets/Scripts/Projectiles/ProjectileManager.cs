using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager instance;

    public GameObject explosion16x16Prefab;
    public GameObject explosion32x32Prefab;
    public GameObject explosion48x48Prefab;
    public GameObject explosion64x64Prefab;
    public GameObject electricExplosion16x16Prefab;
    public GameObject electricExplosion32x32Prefab;
    public GameObject electricExplosion48x48Prefab;
    public GameObject electricExplosion64x64Prefab;
    public GameObject acidCloud32x32Prefab;
    public GameObject acidCloud64x64Prefab;
    public GameObject poisonCloud32x32Prefab;
    public GameObject poisonCloud64x64Prefab;
    public Projectile genericPrefab;
    public Projectile blasterBoltPrefab;
    public Projectile electroBoltPrefab;
    public Projectile tumorBulletPrefab;
    public Projectile rocketPrefab;
    public Projectile bioPlasmaPrefab;
    public Projectile greenBioPlasmaPrefab;
    public Projectile yellowBioPlasmaPrefab;
    public Projectile bigSlimeColumnBulletPrefab;
    public Projectile fireBoltPrefab;
    public Projectile electroFireBoltPrefab;
    public Projectile penetrativeBoltPrefab;
    public Projectile buzzsawPrefab;
    public Projectile explosiveBoltPrefab;
    public Projectile phaseBoltPrefab;
    public Projectile eyePlasmaPrefab;
    public Projectile molotovPrefab;
    public Projectile energyAxePrefab;
    public Projectile waveBombPrefab;
    public Projectile bulletPrefab;
    public Projectile electroplosiveBoltPrefab;
    public Projectile explosiveFireBoltPrefab;
    public Projectile electroplosiveFireBoltPrefab;
    public Projectile smithBoltPrefab;
    public Projectile slimeBulletPrefab;
    public Projectile necroluminantSprayPrefab;
    public Projectile toxinBoltPrefab;
    public Projectile redBioPlasmaPrefab;
    public Projectile webBoltPrefab;
    public Projectile bounceBoltPrefab;
    public Projectile pulseGrenadePrefab;

    private Dictionary<ProjectileType, List<Projectile>> _projectiles = new Dictionary<ProjectileType, List<Projectile>>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (ProjectileType pType in Enum.GetValues(typeof(ProjectileType)))
	    {
            _projectiles.Add(pType, new List<Projectile>());
            NewProjectile(pType);
        }

        if(LayoutManager.instance)
        {
            LayoutManager.instance.onRoomLoaded += OnRoomLoaded;
        }
    }

    public void OnRoomLoaded()
    {
        foreach (var kvp in _projectiles)
        {
            var toRemove = new List<Projectile>();
            var list = kvp.Value;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (i <= 60)
                {
                    if (p.gameObject.activeInHierarchy && (p.lifeCounter > 1 || p.stats.team == Team.Enemy))
                    {
                        p.Recycle();
                    }
                }
                else
                {
                    toRemove.Add(p);
                    Destroy(p.gameObject);
                }
            }

            foreach (var p in toRemove)
            {
                list.Remove(p);
            }

            toRemove.Clear();
        }
    }

    public Projectile NewProjectile(ProjectileType pType)
    {
        Projectile newProjectile = null;

        switch(pType)
        {
            case ProjectileType.Generic:
                newProjectile = Instantiate(genericPrefab);
                break;
            case ProjectileType.BlasterBolt:
                newProjectile = Instantiate(blasterBoltPrefab);
                break;
            case ProjectileType.ElectroBolt:
                newProjectile = Instantiate(electroBoltPrefab);
                break;
            case ProjectileType.TumorBullet:
                newProjectile = Instantiate(tumorBulletPrefab);
                break;
            case ProjectileType.Rocket:
                newProjectile = Instantiate(rocketPrefab);
                break;
            case ProjectileType.BioPlasma:
                newProjectile = Instantiate(bioPlasmaPrefab);
                break;
            case ProjectileType.GreenBioPlasma:
                newProjectile = Instantiate(greenBioPlasmaPrefab);
                break;
            case ProjectileType.YellowBioPlasma:
                newProjectile = Instantiate(yellowBioPlasmaPrefab);
                break;
            case ProjectileType.BigSlimeColumnBullet:
                newProjectile = Instantiate(bigSlimeColumnBulletPrefab);
                break;
            case ProjectileType.FireBolt:
                newProjectile = Instantiate(fireBoltPrefab);
                break;
            case ProjectileType.ElectroFireBolt:
                newProjectile = Instantiate(electroFireBoltPrefab);
                break;
            case ProjectileType.PenetrativeBolt:
                newProjectile = Instantiate(penetrativeBoltPrefab);
                break;
            case ProjectileType.Buzzsaw:
                newProjectile = Instantiate(buzzsawPrefab);
                break;
            case ProjectileType.ExplosiveBolt:
                newProjectile = Instantiate(explosiveBoltPrefab);
                break;
            case ProjectileType.PhaseBolt:
                newProjectile = Instantiate(phaseBoltPrefab);
                break;
            case ProjectileType.EyePlasma:
                newProjectile = Instantiate(eyePlasmaPrefab);
                break;
            case ProjectileType.Molotov:
                newProjectile = Instantiate(molotovPrefab);
                break;
            case ProjectileType.EnergyAxe:
                newProjectile = Instantiate(energyAxePrefab);
                break;
            case ProjectileType.WaveBomb:
                newProjectile = Instantiate(waveBombPrefab);
                break;
            case ProjectileType.Bullet:
                newProjectile = Instantiate(bulletPrefab);
                break;
            case ProjectileType.ElectroplosiveBolt:
                newProjectile = Instantiate(electroplosiveBoltPrefab);
                break;
            case ProjectileType.ExplosiveFireBolt:
                newProjectile = Instantiate(explosiveFireBoltPrefab);
                break;
            case ProjectileType.ElectroplosiveFireBolt:
                newProjectile = Instantiate(electroplosiveFireBoltPrefab);
                break;
            case ProjectileType.SmithBolt:
                newProjectile = Instantiate(smithBoltPrefab);
                break;
            case ProjectileType.SlimeBullet:
                newProjectile = Instantiate(slimeBulletPrefab);
                break;
            case ProjectileType.NecroluminantSpray:
                newProjectile = Instantiate(necroluminantSprayPrefab);
                break;
            case ProjectileType.RedBioPlasma:
                newProjectile = Instantiate(redBioPlasmaPrefab);
                break;
            case ProjectileType.ToxinBolt:
                newProjectile = Instantiate(toxinBoltPrefab);
                break;
            case ProjectileType.WebBolt:
                newProjectile = Instantiate(webBoltPrefab);
                break;
            case ProjectileType.BounceBolt:
                newProjectile = Instantiate(bounceBoltPrefab);
                break;
            case ProjectileType.PulseGrenade:
                newProjectile = Instantiate(pulseGrenadePrefab);
                break;
        }

        if (newProjectile)
        {
            newProjectile.transform.parent = transform;
            newProjectile.gameObject.SetActive(false);
            _projectiles[pType].Add(newProjectile);
        }

        return newProjectile;
    }

    public void ClearProjectiles(ProjectileType[] projectileTypes)
    {
        for (int i = 0; i < projectileTypes.Length; i++)
        {
            var pType = projectileTypes[i];
            List<Projectile> list;
            if(_projectiles.TryGetValue(pType, out list))
            {
                for (int j = 0; j < list.Count; j++) { Destroy(list[j].gameObject); }
                list.Clear();
            }
        }
    }

    public void Shoot(ProjectileStats stats, AimingInfo aimingInfo)
    {
        Shoot(stats, aimingInfo.origin, aimingInfo.direction);
    }

    public void Shoot(ProjectileStats stats, Vector3 origin, Vector3 direction)
    {
        List<Projectile> pList;

        if (_projectiles.TryGetValue(stats.type, out pList))
        {
            Projectile p = null;

            for (int i = 0; i < pList.Count; i++)
            {
                if (!pList[i].gameObject.activeInHierarchy)
                {
                    p = pList[i];
                    break;
                }
            }

            if (!p)
            {
                p = NewProjectile(stats.type);
            }

            p.Shoot(stats, origin, direction);
        }
    }

    public bool ProjectileMotionShoot(ProjectileStats stats, Vector3 origin, Vector3 target)
    {
        Vector3 angle1, angle2;
        var solutions = SolveBallisticArc(origin, stats.speed, target, stats.gravity, out angle1, out angle2);
        if (solutions > 0)
        {
            var angle = solutions == 1 || angle2.y > angle1.y ? angle1 : angle2;
            Shoot(stats, origin, angle);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ArcShoot(ProjectileStats stats, Vector3 origin, Vector3 direction, int arcShots, float fireArc)
    {
        if (fireArc != 0 && arcShots > 1)
        {
                for (int i = 0; i < arcShots; i++)
                {
                    float divisor = fireArc < 360 ? arcShots - 1 : arcShots;
                    float angleMod = (((float)i / divisor) * 2f) - 1f;
                    Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * fireArc / 2, Vector3.forward) * direction).normalized;
                    Shoot(stats, origin, shotDirection);
                }
        }
        else
        {
            Debug.LogWarning("ArcShot Should not be called for a single shot or with a fire arc of 0");
        }
    }

    public void BurstShoot(ProjectileStats stats, Vector3 origin, Vector3 direction, int burstCount, float burstTime, int arcShots = 1, float fireArc = 0)
    {
        StartCoroutine(BurstShot(stats, origin, direction, burstCount, burstTime, arcShots, fireArc));
    }

    private IEnumerator BurstShot(ProjectileStats stats, Vector3 origin, Vector3 direction, int burstCount, float burstTime, int arcShots, float fireArc)
    {
        var shotsFired = 0;
        while (shotsFired < burstCount)
        {
            shotsFired++;           

            if (fireArc != 0 && arcShots > 1)
            {
                ArcShoot(stats, origin, direction, arcShots, fireArc);
            }
            else
            {
                Shoot(stats, origin, direction);
            }

            yield return new WaitForSeconds(burstTime/burstCount);
        }
    }

    public void ArcBurstShoot(ProjectileStats stats, Vector3 origin, Vector3 direction, int burstCount, float burstTime, float burstArc, int arcShots = 1, float fireArc = 0)
    {
        StartCoroutine(ArcBurstShot(stats, origin, direction, burstCount, burstTime, burstArc, arcShots, fireArc));
    }

    private IEnumerator ArcBurstShot(ProjectileStats stats, Vector3 origin, Vector3 direction, int burstCount, float burstTime, float burstArc, int arcShots, float fireArc)
    {
        var shotsFired = 0;

        while (shotsFired < burstCount)
        {
            shotsFired++;

            float divisor = burstArc < 360 ? burstCount - 1 : burstCount;
            float angleMod = (((float)shotsFired / divisor) * 2f) - 1f;
            Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * burstArc / 2, Vector3.forward) * direction).normalized;

            if (fireArc != 0 && arcShots > 1)
            {
                ArcShoot(stats, origin, shotDirection, arcShots, fireArc);
            }
            else
            {
                Shoot(stats, origin, shotDirection);
            }

            yield return new WaitForSeconds(burstTime / burstCount);
        }
    }

    public void SpawnExplosion(Vector3 position, Explosion size, Team team, float damagePerSecond = 3f, bool ignoreDoors = true, DamageType damageType = DamageType.Generic)
    {
        GameObject prefab = null;

        switch(size)
        {
            case Explosion.E16x16:
                prefab = explosion16x16Prefab;
                break;
            case Explosion.E32x32:
                prefab = explosion32x32Prefab;
                break;
            case Explosion.E48x48:
                prefab = explosion48x48Prefab;
                break;
            case Explosion.E64x64:
                prefab = explosion64x64Prefab;
                break;
            case Explosion.Elec16x16:
                prefab = electricExplosion16x16Prefab;
                break;
            case Explosion.Elec32x32:
                prefab = electricExplosion32x32Prefab;
                break;
            case Explosion.Elec48x48:
                prefab = electricExplosion48x48Prefab;
                break;
            case Explosion.Elec64x64:
                prefab = electricExplosion64x64Prefab;
                break;
        }

        var splosion = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        var damageTrigger = splosion.GetComponent<DamageCreatureTrigger>();
        damageTrigger.damage = damagePerSecond;
        damageTrigger.perSecond = true;
        damageTrigger.ignoreAegis = true;
        damageTrigger.ignoreDoors = ignoreDoors;
        damageTrigger.team = team;

        Constants.SetCollisionForTeam(damageTrigger.collider2D, team);
    }

    public void SpawnDamageCloud(StatusEffect effect, Damageable damageable, float size, Team team, float lifespan)
    {
        SpawnDamageCloud(effect, damageable.position, size, team, lifespan);
    }

    public void SpawnDamageCloud(StatusEffect effect, Transform transform, float size, Team team, float lifespan)
    {
        SpawnDamageCloud(effect, transform.position, size, team, lifespan);
    }

    public void SpawnDamageCloud(StatusEffect effect, Vector3 position, float size, Team team, float lifespan)
    {
        GameObject prefab = null;
        float divisor = 1;

        switch (effect.type)
        {
            case StatusEffectsType.Acid:
                if (Mathf.Abs(size - 2) < Mathf.Abs(size - 4))
                {
                    prefab = acidCloud32x32Prefab;
                    divisor = 2;
                }
                else
                {
                    prefab = acidCloud64x64Prefab;
                    divisor = 4;
                }
                break;
            case StatusEffectsType.Poison:
                if (Mathf.Abs(size - 2) < Mathf.Abs(size - 4))
                {
                    prefab = poisonCloud32x32Prefab;
                    divisor = 2;
                }
                else
                {
                    prefab = poisonCloud64x64Prefab;
                    divisor = 4;
                }
                break;
        }

        var scale = size / divisor;

        var cloud = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        cloud.transform.localScale = Vector3.one * scale;

        var damageTrigger = cloud.GetComponent<DamageCreatureTrigger>();
        damageTrigger.statusEffects.Add(effect);        

        var timed = cloud.GetComponent<TimedObject>();
        timed.lifespan = lifespan;

        Constants.SetCollisionForTeam(damageTrigger.collider2D, team, true);
    }

    public int SolveBallisticArc(Vector3 proj_pos, float proj_speed, Vector3 target, float gravity, out Vector3 s0, out Vector3 s1)
    {
        // Handling these cases is up to your project's coding standards
        Debug.Assert(proj_pos != target && proj_speed > 0 && gravity > 0, "fts.solve_ballistic_arc called with invalid data");

        // C# requires out variables be set
        s0 = Vector3.zero;
        s1 = Vector3.zero;

        // Derivation
        //   (1) x = v*t*cos O
        //   (2) y = v*t*sin O - .5*g*t^2
        // 
        //   (3) t = x/(cos O*v)                                        [solve t from (1)]
        //   (4) y = v*x*sin O/(cos O * v) - .5*g*x^2/(cos^2 O*v^2)     [plug t into y=...]
        //   (5) y = x*tan O - g*x^2/(2*v^2*cos^2 O)                    [reduce; cos/sin = tan]
        //   (6) y = x*tan O - (g*x^2/(2*v^2))*(1+tan^2 O)              [reduce; 1+tan O = 1/cos^2 O]
        //   (7) 0 = ((-g*x^2)/(2*v^2))*tan^2 O + x*tan O - (g*x^2)/(2*v^2) - y    [re-arrange]
        //   Quadratic! a*p^2 + b*p + c where p = tan O
        //
        //   (8) let gxv = -g*x*x/(2*v*v)
        //   (9) p = (-x +- sqrt(x*x - 4gxv*(gxv - y)))/2*gxv           [quadratic formula]
        //   (10) p = (v^2 +- sqrt(v^4 - g(g*x^2 + 2*y*v^2)))/gx        [multiply top/bottom by -2*v*v/x; move 4*v^4/x^2 into root]
        //   (11) O = atan(p)

        Vector3 diff = target - proj_pos;
        Vector3 diffXZ = new Vector3(diff.x, 0f, diff.z);
        float groundDist = diffXZ.magnitude;

        float speed2 = proj_speed * proj_speed;
        float speed4 = proj_speed * proj_speed * proj_speed * proj_speed;
        float y = diff.y;
        float x = groundDist;
        float gx = gravity * x;

        float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

        // No solution
        if (root < 0)
            return 0;

        root = Mathf.Sqrt(root);

        float lowAng = Mathf.Atan2(speed2 - root, gx);
        float highAng = Mathf.Atan2(speed2 + root, gx);
        int numSolutions = lowAng != highAng ? 2 : 1;

        Vector3 groundDir = diffXZ.normalized;
        s0 = groundDir * Mathf.Cos(lowAng) * proj_speed + Vector3.up * Mathf.Sin(lowAng) * proj_speed;
        if (numSolutions > 1)
            s1 = groundDir * Mathf.Cos(highAng) * proj_speed + Vector3.up * Mathf.Sin(highAng) * proj_speed;

        return numSolutions;
    }
}
