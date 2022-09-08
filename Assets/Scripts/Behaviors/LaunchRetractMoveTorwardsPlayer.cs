using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaunchRetractMoveTorwardsPlayer : MonoBehaviour, IPausable
{
    public float speed = 1f;
    public float retractDistance = 0f;
    public float retractSpeed = 1f;
    public float launchDistance = 5f;
    public float launchSpeed = 5f;
    private float _maxDistance = 0f;
    public Transform anchor;
    private Controller2D _controller2D;
    private bool _lerping;

    public UnityEvent onLauchStart;
    public UnityEvent onLauchEnd;
    public UnityEvent onRetractStart;
    public UnityEvent onRertactEnd;

    // Use this for initialization
    private void Awake ()
    {
        _controller2D = GetComponent<Controller2D>();
	}
	
	// Update is called once per frame
	private void Update ()
    {
        var direction = (PlayerManager.instance.player1.transform.position - transform.position).normalized;
        var newPosition = transform.position + direction * speed * Time.deltaTime;
        var distanceFromAnchor = Vector3.Distance(newPosition, anchor.position);
        if (distanceFromAnchor > _maxDistance)
        {
            direction = (anchor.position - newPosition).normalized;
            newPosition += direction * (distanceFromAnchor - _maxDistance);
        }

        if (_controller2D)
        {
            var movement = newPosition - transform.position;
            if (movement != Vector3.zero)
            {
                _controller2D.Move(movement);
            }
        }
        else
        {
            transform.position = newPosition;
        }
    }

    public void OnDrawGizmosSelected()
    {
        Extensions.DrawCircle(anchor.position, retractDistance);
        Extensions.DrawCircle(anchor.position, launchDistance);
    }

    public void LaunchAtPlayer()
    {
        if (!_lerping && _maxDistance != launchDistance)
        {
            StartCoroutine(LerpMaxDistance(launchDistance, launchSpeed, onLauchStart, onLauchEnd));
        }
    }

    public void Retract()
    {
        StartCoroutine(LerpMaxDistance(retractDistance, retractSpeed, onRetractStart, onRertactEnd));
    }

    public IEnumerator LerpMaxDistance(float newMaxDistance, float changeSpeed, UnityEvent onStart, UnityEvent onEnd)
    {
        _lerping = true;

        if(onStart != null)
        {
            onStart.Invoke();
        }

        var origSpeed = speed;
        speed = changeSpeed;
        while(_maxDistance != newMaxDistance)
        {
            _maxDistance = Mathf.MoveTowards(_maxDistance, newMaxDistance, changeSpeed * Time.deltaTime);
            yield return null;
        }
        speed = origSpeed;

        if(onEnd != null)
        {
            onEnd.Invoke();
        }

        _lerping = false;
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
