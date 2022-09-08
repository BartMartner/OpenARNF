using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : PickUp
{
    public float velocity;
    
    public virtual void OnDestroy()
    {
        if (PickUpManager.instance)
        {
            PickUpManager.instance.allDrops.Remove(this);
        }
    }

    public virtual IEnumerator Start()
    {
        allowCoOp = true;
        PickUpManager.instance.allDrops.Add(this);
        yield return new WaitForSeconds(15);
        StartCoroutine(BlinkAndDisappear(3));
    }

    public void Update()
    {
        var closestPlayer = PlayerManager.instance.GetClosestPlayer(transform.position);
        if (closestPlayer)
        {
            var playerDelta = closestPlayer.position - transform.position;
            var maxRange = 3 + closestPlayer.pickUpRangeBonus;
            var newVelocity = Mathf.Clamp(maxRange - playerDelta.magnitude, 0, maxRange) * 8 / maxRange; //at 0 delta velocity 8, at maxRange or more delta velocity is 0.
            velocity = Mathf.Lerp(velocity, newVelocity, 0.5f);
            transform.position += playerDelta.normalized * velocity * Time.deltaTime;
        }
    }

    public IEnumerator BlinkAndDisappear(float time)
    {
        var timer = 0f;
        var flashTime = 0.1f;
        while (timer < time)
        {
            _spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashTime);
            timer += flashTime;

            _spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashTime);
            timer += flashTime;
        }

        Destroy(gameObject);
    }
}
