using UnityEngine;
using System.Collections;

public class TimedShooter : Shooter, IPausable
{
    public float shootTime = 1;
    public float shootCounter;
    public bool randomStartDelay;
    public bool resetOnPause;

    protected override void Start()
    {
        base.Start();
        if (randomStartDelay) { shootCounter = Random.Range(0, shootTime * 0.8f); }
    }

    private void Update()
    {
        if(LayoutManager.instance && LayoutManager.instance.transitioning) { return; }

        if (!_shooting)
        {
            shootCounter += Time.deltaTime;
            if (shootCounter > shootTime - preShootDelay) { Shoot(); }
        }
    }

    public override IEnumerator ProjectileMotionShootCoroutine(Vector3 angle)
    {
        shootCounter = 0;
        return base.ProjectileMotionShootCoroutine(angle);
    }

    public override IEnumerator ShootCoroutine()
    {
        shootCounter = 0;
        return base.ShootCoroutine();
    }

    public override void Unpause()
    {
        base.Unpause();
        if (resetOnPause) { shootCounter = 0; }
    }
}
