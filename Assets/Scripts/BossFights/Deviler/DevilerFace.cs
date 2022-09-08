using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevilerFace : MonoBehaviour
{
    public Transform shootPoint;
    public ProjectileStats projectileStats;
    public bool busy { get; private set; }
    private Animator _animator;
    public Animator animator
    {
        get
        {
            if (!_animator) { _animator = GetComponent<Animator>(); }
            return _animator;
        }
    }

    public void Shoot(Transform target, int shots, float delay)
    {
        if (busy)
        {
            Debug.LogWarning("Shoot Called While Deviler Face (" + gameObject.name + ") was busy!");
            return;
        }

        StartCoroutine(ShootTargetRoutine(target,shots,delay));
    }

    public IEnumerator ShootTargetRoutine(Transform target, int shots, float delay)
    {
        busy = true;
        animator.Play("EyeClose");
        yield return new WaitForSeconds(0.25f);
        var origAnimLength = (5f / 15f);
        var shootAnimLength = origAnimLength;
        if(delay < shootAnimLength)
        {
            animator.speed = shootAnimLength / delay;
            shootAnimLength = delay;
        }
        var shootFrameLength = shootAnimLength/5f;

        for (int i = 0; i < shots; i++)
        {
            animator.Play("Shoot", 0, 0);
            yield return new WaitForSeconds(shootFrameLength);
            var delta = (target.position - shootPoint.position);
            delta.z = 0;
            ProjectileManager.instance.Shoot(projectileStats, shootPoint.position, delta.normalized);
            yield return new WaitForSeconds(shootFrameLength * 4);
            if (delay > origAnimLength)
            {
                if (delay > 0.25 + origAnimLength && i != shots-1) { animator.Play("EyeClose"); }
                yield return new WaitForSeconds(delay - origAnimLength);
            }
        }

        animator.speed = 1;
        animator.Play("Default");
        busy = false;
    }

    public void Show(DevilerChild target)
    {
        if(busy)
        {
            Debug.LogWarning("Show Called While Deviler Face (" + gameObject.name + ") was busy!");
            return;
        }

        gameObject.SetActive(true);
        transform.parent = target.transform;
        transform.localPosition = Vector3.one * 0.5f;
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        busy = true;
        animator.Play("Show");
        yield return new WaitForSeconds(0.66f);
        busy = false;
    }

    public void Hide(bool force = false)
    {
        if(force)
        {
            busy = false;
            StopAllCoroutines();
        }

        if (busy)
        {
            Debug.LogWarning("Hide Called While Deviler Face (" + gameObject.name + ") was busy!");
            return;
        }

        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        busy = true;
        animator.Play("Hide");
        yield return new WaitForSeconds(0.66f);
        busy = false;
        gameObject.SetActive(false);
    }
}
