using UnityEngine;
using System.Collections;
using System;

public class AnchoredPingPongMoveTorwardsPlayer : MonoBehaviour, IPausable 
{
    public float speed = 1f;
    public float pingPongSpeed = 2f;
    public float minMaxDistance = 1;
    public float maxMaxDistance = 2;
    public Transform anchor;
    public float timer;

    private Vector3 _direction;
    private IDamageable _target;

    void Update()
    {
        if(BossFightUI.instance && BossFightUI.instance.getReadyVisible) { return; }
        if(LayoutManager.instance && LayoutManager.instance.transitioning) { return; }

        if (anchor)
        {
            timer += Time.deltaTime * pingPongSpeed;

            if (timer > float.MaxValue - 1) { timer = 0; }

            if (!PlayerManager.CanTarget(_target))
            {
                _target = PlayerManager.instance.GetClosestPlayerDamageable(transform.position);
            }
            
            _direction = _target == null ? Quaternion.Euler(0, 0, speed * 15 * Time.deltaTime) * _direction : (_target.position - transform.position).normalized;
            var newPosition = transform.position + _direction * speed * Time.deltaTime;
            var distanceFromAnchor = Vector3.Distance(newPosition, anchor.position);
            var maxDistance = minMaxDistance + Mathf.PingPong(timer, maxMaxDistance - minMaxDistance);
            if (distanceFromAnchor > maxDistance)
            {
                _direction = (anchor.position - newPosition).normalized;
                newPosition += _direction * (distanceFromAnchor - maxDistance);
            }
            transform.position = newPosition;
        }
    }

    public void OnDrawGizmosSelected()
    {
        Extensions.DrawCircle(anchor.position, minMaxDistance);
        Extensions.DrawCircle(anchor.position, maxMaxDistance);
    }

    public void Pause()
    {
        enabled = false;
    }

    public void Unpause()
    {
        enabled = true;
    }
}
