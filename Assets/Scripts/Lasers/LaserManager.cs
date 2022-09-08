using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//GOALS 
/* Backwards Compatibility: Allow lasers top be spawned via the LaserManager or included in objects. Don't make one or the other mandatory.
 * Allow the Laser Manager to attach lasers to an object, and ensure those lasers are recycled if that object is destroyed or no longer needed.
*/
public class LaserManager : MonoBehaviour
{
    public static LaserManager instance;

    public Laser energyPrefab;
    public Laser bigEnergyPrefab;
    public Laser slimePrefab;
    public Laser heatPrefab;
    public Laser bigHeatPrefab;
    public Laser bioPlasmaPrefab;
    public Laser blueBioPlasmaPrefab;
    public Laser tutSmithPrefab;
    public Laser bigSlimePrefab;
    public Laser bigBioPlasmaPrefab;
    public Laser railGunPrefab;
    public LineLaser basicBeamPrefab;

    private Dictionary<LaserType, List<ILaser>> _lasers = new Dictionary<LaserType, List<ILaser>>();

    private void Awake()
    {
        instance = this;
        foreach (LaserType lType in System.Enum.GetValues(typeof(LaserType)))
        {
            _lasers.Add(lType, new List<ILaser>());            
        }
    }

    public ILaser NewLaser(LaserType lType)
    {
        ILaser newLaser = null;

        switch (lType)
        {
            case LaserType.Slime:
                newLaser = Instantiate(slimePrefab);
                break;
            case LaserType.Heat:
                newLaser = Instantiate(heatPrefab);
                break;
            case LaserType.BigHeat:
                newLaser = Instantiate(bigHeatPrefab);
                break;
            case LaserType.Bioplasma:
                newLaser = Instantiate(bioPlasmaPrefab);
                break;
            case LaserType.Energy:
                newLaser = Instantiate(energyPrefab);
                break;
            case LaserType.BigEnergy:
                newLaser = Instantiate(bigEnergyPrefab);
                break;
            case LaserType.BlueBioPlasma:
                newLaser = Instantiate(blueBioPlasmaPrefab);
                break;
            case LaserType.BasicBeam:
                newLaser = Instantiate(basicBeamPrefab);
                break;
            case LaserType.TutSmith:
                newLaser = Instantiate(tutSmithPrefab);
                break;
            case LaserType.BigSlime:
                newLaser = Instantiate(bigSlimePrefab);
                break;
            case LaserType.BigBioPlasma:
                newLaser = Instantiate(bigBioPlasmaPrefab);
                break;
            case LaserType.RailGun:
                newLaser = Instantiate(railGunPrefab);
                break;
        }

        if (newLaser != null)
        {
            newLaser.managed = true;
            newLaser.transform.parent = transform;
            newLaser.gameObject.SetActive(false);
            _lasers[lType].Add(newLaser);
        }

        return newLaser;
    }

    public ILaser GetLaser(LaserType lType)
    {
        List<ILaser> lList;

        if (_lasers.TryGetValue(lType, out lList))
        {
            ILaser l = null;

            for (int i = 0; i < lList.Count; i++)
            {
                var test = lList[i];
                if (!test.gameObject.activeInHierarchy && test.transform.parent == transform)
                {
                    l = lList[i];
                    break;
                }
            }

            if (l == null) { l = NewLaser(lType); }

            return l;
        }

        return null;
    }

    public ILaser AttachAndFireLaser(LaserStats stats, Vector3 offset, Quaternion angle, float duration, IHasOnDestroy attachTo)
    {
        var laser = GetLaser(stats.laserType);
        laser.AttachTo(attachTo);

        var origin = attachTo.transform.position + offset;
        if (duration > 0)
        {
            StartCoroutine(FireLaserRoutine(laser, stats, origin, angle, duration));
        }
        else
        {
            FireLaser(laser, stats, origin, angle);
        }

        return laser;
    }

    public ILaser FireLaser(LaserStats stats, Vector3 origin, Quaternion angle, float duration)
    {
        var laser = GetLaser(stats.laserType);
        if (duration > 0)
        {
            StartCoroutine(FireLaserRoutine(laser, stats, origin, angle, duration));
        }
        else
        {
            FireLaser(laser, stats, origin, angle);
        }
        return laser;
    }

    public ILaser FireLaser(LaserStats stats, Vector3 origin, float eulerAngle, float duration)
    {
        var angle = Quaternion.Euler(0, 0, eulerAngle);
        var laser = GetLaser(stats.laserType);
        if (duration > 0)
        {
            StartCoroutine(FireLaserRoutine(laser, stats, origin, angle, duration));
        }
        else
        {
            FireLaser(laser, stats, origin, angle);
        }
        return laser;
    }

    public IEnumerator FireLaserRoutine(ILaser laser, LaserStats stats, Vector3 origin, Quaternion angle, float duration)
    {
        laser.transform.position = origin;
        laser.transform.rotation = angle;
        laser.AssignStats(stats);
        laser.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        laser.Stop(); //laser will call gameObject.SetActive(false) or Recycle on self
    }

    public void FireLaser(ILaser laser, LaserStats stats, Vector3 origin, Quaternion angle)
    {
        laser.transform.position = origin;
        laser.transform.rotation = angle;
        laser.AssignStats(stats);
        laser.gameObject.SetActive(true);        
    }

    public void Recycle(ILaser laser)
    {
        if (!this) return;

        if (!laser.managed)
        {
            Debug.LogError("trying to recylce a laser not marked as managed!");
        }
        else 
        {
            var l = laser as Laser; //handles weird things that happen if laser is destroyed
            var ll = laser as LineLaser; //handles weird things that happen if laser is destroyed

            if ((l || ll) && laser != null)
            {
                if (l && !l.enabled)
                {
                    Debug.LogError("A disabled laser was recylced! Enabling");
                    l.enabled = true;
                }
                laser.transform.parent = transform;
                laser.gameObject.SetActive(false);
            }
        }
    }
}
