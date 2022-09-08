using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlowBot : Follower
{
    public float angularSpeed = 360f;
    public float minDistance = 2;
    public float maxDistance = 3;

    public SpriteRenderer lightSprite;
    private Vector3 _rotatePoint;

    public void Update()
    {
        _rotatePoint = player.transform.position;
        var distance = Vector3.Distance(transform.position, _rotatePoint);
        if (distance < minDistance || distance > maxDistance)
        {
            Vector3 direction;
            var speed = Mathf.Clamp(distance * distance / (maxDistance*2), 0.5f, 32);

            if(distance < minDistance)
            {
                direction = transform.position == _rotatePoint ? Vector3.up : (transform.position - _rotatePoint).normalized;
            }
            else
            {
                direction = (_rotatePoint- transform.position).normalized;
            }

            transform.position += direction * Time.deltaTime * speed;
        }
        
        transform.RotateAround(_rotatePoint, Vector3.forward, angularSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.identity;
    }

    public override bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        StartCoroutine(LightFlicker());

        return true;
    }

    public IEnumerator LightFlicker()
    {
        lightSprite.enabled = false;
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < 3; i++)
        {
            lightSprite.enabled = true;
            yield return new WaitForSeconds(1 / 12f);
            lightSprite.enabled = false;
            yield return new WaitForSeconds(1 / 12f);
        }

        lightSprite.enabled = true;
    }
}
