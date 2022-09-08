using UnityEngine;
using System.Collections;

public class SequencedShooters : MonoBehaviour
{
    public Shooter[] shooters;
    public bool getShootersInChildern = true;
    public float sequenceDelay;

    private bool _shooting;
    public bool shooting
    {
        get { return _shooting; }
    }

    public void Start()
    {
        if(getShootersInChildern)
        {
            shooters = GetComponentsInChildren<Shooter>();
        }
    }

    public void Shoot()
    {
        StartCoroutine(ShootSequence());
    }

    public IEnumerator ShootSequence()
    {
        _shooting = true;
        var length = shooters.Length;
        var delay = new WaitForSeconds(sequenceDelay);
        for (int i = 0; i < length; i++)
        {
            var shooter = shooters[i];
            shooter.Shoot();
            while(shooter.shooting)
            {
                yield return null;
            }

            yield return delay;
        }

        _shooting = false;
    }
}
