using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class FXManager : MonoBehaviour
{
    public static FXManager instance;

    public FX bloodSplatSmall;
    public FX smokePuffSmall;
    public FX flameSmall;
    public FX explosionSmall;
    public FX explosionMedium;
    public FX teleportation;
    public FX animeSplode;
    public FX dashPuff;
    public FX acidBubbles;
    public FX poisonBubbles;
    public FX splash32;
    public FX acidSplash32;
    public FX muzzleFlash;
    public Creep creepPrefab;
    public AudioClip[] splatSounds;
    public AudioClip[] splodeSounds;
    public AudioClip[] teleportSounds;
    public AudioClip[] shringSounds;
    public AudioClip[] acidSounds;
    public AudioClip[] splashSounds;
    private Dictionary<FXType, List<FX>> _fx = new Dictionary<FXType, List<FX>>();
    private List<Creep> _creeps = new List<Creep>();    

    private Collider2D[] _hitResults = new Collider2D[5];

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    { 
        foreach (FXType pType in Enum.GetValues(typeof(FXType)))
        {
            _fx.Add(pType, new List<FX>());
            NewFX(pType);
        }
    }

    public FX NewFX(FXType fxType)
    {
        if(fxType == FXType.None)
        {
            return null;
        }

        FX newFX = null;

        switch (fxType)
        {
            case FXType.BloodSplatSmall:
                newFX = Instantiate(bloodSplatSmall);
                break;
            case FXType.SmokePuffSmall:
                newFX = Instantiate(smokePuffSmall);
                break;
            case FXType.FlameSmall:
                newFX = Instantiate(flameSmall);
                break;
            case FXType.ExplosionSmall:
                newFX = Instantiate(explosionSmall);
                break;
            case FXType.ExplosionMedium:
                newFX = Instantiate(explosionMedium);
                break;
            case FXType.Teleportation:
                newFX = Instantiate(teleportation);
                break;
            case FXType.AnimeSplode:
                newFX = Instantiate(animeSplode);
                break;
            case FXType.DashPush:
                newFX = Instantiate(dashPuff);
                break;
            case FXType.AcidBubbles:
                newFX = Instantiate(acidBubbles);
                break;
            case FXType.PoisonBubbles:
                newFX = Instantiate(poisonBubbles);
                break;
            case FXType.Splash32:
                newFX = Instantiate(splash32);
                break;
            case FXType.AcidSplash32:
                newFX = Instantiate(acidSplash32);
                break;
            case FXType.MuzzleFlash:
                newFX = Instantiate(muzzleFlash);
                break;
        }

        if (newFX)
        {
            newFX.transform.parent = transform;
            newFX.gameObject.SetActive(false);
            _fx[fxType].Add(newFX);
        }

        return newFX;
    }

    public Creep NewCreep()
    {
        Creep newCreep = null;
        newCreep = Instantiate(creepPrefab);
        newCreep.transform.parent = transform;
        newCreep.gameObject.SetActive(false);
        if (_creeps.Count < 50)
        {
            _creeps.Add(newCreep);
        }
        else
        {
            newCreep.recycle = false;
        }
        return newCreep;
    }

    public bool TrySpawnCreep(Vector3 origin, Vector3 direction, float distance, CreepStats stats, float radius = 0)
    {
        RaycastHit2D hit;
        if (radius == 0)
        {
            hit = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Default"));
        }
        else
        {
            hit = Physics2D.CircleCast(origin, 0.2f, direction, distance, LayerMask.GetMask("Default"));
        }

        if (hit.collider)
        {
            var previous = Physics2D.queriesHitTriggers;
            _hitResults = new Collider2D[5];
            Physics2D.queriesHitTriggers = true;
            Physics2D.OverlapPointNonAlloc(hit.point, _hitResults);
            Physics2D.queriesHitTriggers = previous;
            if (_hitResults != null && _hitResults.Any((c) => c != null && c.isActiveAndEnabled && c.CompareTag("Creep")))
            {
                return false;
            }

            Creep creep = null;
            for (int i = 0; i < _creeps.Count; i++)
            {
                if (!_creeps[i].gameObject.activeInHierarchy)
                {
                    creep = _creeps[i];
                    break;
                }
            }

            if(!creep)
            {
                creep = NewCreep();
            }

            var rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            creep.Spawn(hit.point, rotation, stats);

            return true;
        }
        else
        {
            return false;
        }        
    }

    public void KillAllCreep()
    {
        foreach (var creep in _creeps)
        {
            if (creep.isActiveAndEnabled)
            {
                creep.Kill();
            }
        }
    }

    public void PlaySplodeSound()
    {
        AudioManager.instance.PlayOneShot(splodeSounds[Random.Range(0, splodeSounds.Length)]);
    }

    public void SpawnFX(FXType fxType, Vector3 position, bool randomRotation = false, bool noSound = false, bool flipX = false, bool flipY = false)
    {
        if (fxType == FXType.None) return;

        List<FX> fxList;

        if (_fx.TryGetValue(fxType, out fxList))
        {
            FX fx = null;

            for (int i = 0; i < fxList.Count; i++)
            {
                if (!fxList[i].gameObject.activeInHierarchy)
                {
                    fx = fxList[i];
                    break;
                }
            }

            if (!fx)
            {
                fx = NewFX(fxType);
            }

            fx.Spawn(position);

            if(!noSound)
            {
                AudioClip clip = null;
                switch(fxType)
                {
                    case FXType.BloodSplatSmall:
                        clip = splatSounds[Random.Range(0, splatSounds.Length)];
                        break;
                    case FXType.ExplosionSmall:
                    case FXType.ExplosionMedium:
                        clip = splodeSounds[Random.Range(0, splodeSounds.Length)];
                        break;
                    case FXType.Teleportation:
                        clip = teleportSounds[Random.Range(0, teleportSounds.Length)];
                        break;
                    case FXType.AnimeSplode:
                        clip = shringSounds[Random.Range(0, shringSounds.Length)];
                        break;
                    case FXType.AcidBubbles:
                        clip = acidSounds[Random.Range(0, acidSounds.Length)];
                        break;
                    case FXType.Splash32:
                        clip = splashSounds[Random.Range(0, splashSounds.Length)];
                        break;
                }

                if (clip) { AudioManager.instance.PlayClipAtPoint(clip, position); }
            }

            if(randomRotation)
            {
                fx.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            }
            else
            {
                if (flipX && flipY)
                {
                    fx.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                else if (flipX)
                {
                    fx.transform.rotation = Constants.flippedFacing;
                }
                else if (flipY)
                {
                    fx.transform.rotation = Quaternion.Euler(180, 0, 0);
                }
                else
                {
                    fx.transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    public void SpawnFX(FXType fxType, Vector3 position, Quaternion rotation)
    {
        if (fxType == FXType.None) return;

        List<FX> fxList;

        if (_fx.TryGetValue(fxType, out fxList))
        {
            FX fx = null;

            for (int i = 0; i < fxList.Count; i++)
            {
                if (!fxList[i].gameObject.activeInHierarchy)
                {
                    fx = fxList[i];
                    break;
                }
            }

            if (!fx)
            {
                fx = NewFX(fxType);
            }

            fx.Spawn(position);
            fx.transform.rotation = rotation;
        }
    }

    public void SpawnFXs(FXType fxType, Bounds area, int amount, float time)
    {
        if (fxType == FXType.None) return;
        StartCoroutine(SpawnFXsOverTime(fxType, area, amount, time));
    }

    public IEnumerator SpawnFXsOverTime(FXType fxType, Bounds area, int amount, float time)
    {
        List<FX> fxList;

        if (_fx.TryGetValue(fxType, out fxList))
        {
            var delay = new WaitForSeconds(time / (float)amount);

            for (int a = 0; a < amount; a++)
            {
                FX fx = null;

                for (int i = 0; i < fxList.Count; i++)
                {
                    if (!fxList[i].gameObject.activeInHierarchy)
                    {
                        fx = fxList[i];
                        break;
                    }
                }

                if (!fx)
                {
                    fx = NewFX(fxType);
                }

                var position = Extensions.randomInsideBounds(area);

                fx.Spawn(position);
                yield return delay;
            }
        }
    }
}
