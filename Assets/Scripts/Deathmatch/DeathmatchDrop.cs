using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeathmatchDrop : PickUp
{
    public float respawnTime = 10f;
    public float velocity;

    private Vector3 _originalPosition;

    public virtual void OnDestroy()
    {
        if (PickUpManager.instance)
        {
            PickUpManager.instance.allDrops.Remove(this);
        }
    }

    public virtual void Start()
    {
        allowCoOp = true;
        PickUpManager.instance.allDrops.Add(this);
        _originalPosition = transform.position;
    }

    public void Update()
    {
        var player = DeathmatchManager.instance.GetClosestPlayerInRange(transform, 8, false);
        if (player && PickUpNeeded(player) && player.pickUpRangeBonus > 0)
        {
            var playerDelta = player.position - transform.position;
            var maxRange = 3 + player.pickUpRangeBonus;
            float newVelocity = 0;
            if (maxRange > 0)
            {
                newVelocity = Mathf.Clamp(maxRange - playerDelta.magnitude, 0, maxRange) * 8 / maxRange; //at 0 delta velocity 8, at maxRange or more delta velocity is 0.    
            }

            velocity = Mathf.Lerp(velocity, newVelocity, 0.5f);
            transform.position += playerDelta.normalized * velocity * Time.deltaTime;
        }
    }

    public virtual bool PickUpNeeded(Player player)
    {
        return true;
    }

    public override void OnPickUp(Player player)
    {
        if (onPickUp != null) { onPickUp.Invoke(player); }
        if (pickUpSound) { AudioSource.PlayClipAtPoint(pickUpSound, transform.position); }
        StartCoroutine(WaitRespawn());
    }

    private IEnumerator WaitRespawn()
    {
        var collider = GetComponent<Collider2D>();
        var renderers = GetComponentsInChildren<SpriteRenderer>();

        collider.enabled = false;
        foreach (var r in renderers)
        {
            r.enabled = false;
        }

        yield return new WaitForSeconds(respawnTime);

        if (pickUpSound) { AudioSource.PlayClipAtPoint(pickUpSound, transform.position); }

        transform.position = _originalPosition;

        collider.enabled = true;
        foreach (var r in renderers)
        {
            r.enabled = true;
        }
    }
}
