using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSpinner : MonoBehaviour, IHasOnDestroy
{
    public LaserStats laserStats;
    public int lasers;
    public float warmUp = 1;
    public float spinTime = 1;
    public float targetRotation = 90;
    public float coolDown = 1;
    public AudioClip lasersStart;

    private AudioSource _audioSource; //should have the looping sound;

    public Action onDestroy { get; set; }

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Activate()
    {
        StartCoroutine(LaserSpin());
    }

    public void Stop()
    {
        StopAllCoroutines();
        _audioSource.Stop();
    }

    private IEnumerator LaserSpin()
    {
        transform.rotation = Quaternion.identity;

        _audioSource.PlayOneShot(lasersStart);

        var totalTime = warmUp + spinTime + coolDown;

        for (int i = 0; i < lasers; i++)
        {
            var angle = Quaternion.Euler(0,0,i * (360f/lasers));
            LaserManager.instance.AttachAndFireLaser(laserStats, Vector3.zero, angle, totalTime, this);
        }

        _audioSource.Play();

        yield return new WaitForSeconds(warmUp);

        var timer = 0f;
        var euler = Vector3.zero;
        while (timer < spinTime)
        {
            timer += Time.deltaTime;
            euler.z = Mathf.Lerp(0, targetRotation, timer / spinTime);
            transform.rotation = Quaternion.Euler(euler);
            yield return null;
        }

        yield return new WaitForSeconds(coolDown);

        _audioSource.Stop();
    }

    public void OnDestroy()
    {
        if (onDestroy != null) onDestroy();
    }
}
