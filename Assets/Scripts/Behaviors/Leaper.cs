using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Controller2D))]
public class Leaper : BaseMovementBehaviour
{ 
    public float leapForce = 5;
    public float leapPreWarmTime;
    public float leapRecoveryTime;
    public float leapWaitTime = 1f;
    public int rays = 3;
    public float rayArc = 30;
    public float checkDistance = 5;
    public bool leapAtObstacles;
    private bool _leaping;
    private bool _justLept;
    private Controller2D _controller2D;

    [Header("Events")]
    public UnityEvent onLeapStart;
    public UnityEvent onLeapLand;
    public UnityEvent onLeapRecovered;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();        
    }

    //TODO: make angle of raycast assignable
    //make this not inherit from enemy and acts as a state somehow
    private void Update()
    {
        if (!_leaping && !_justLept && _controller2D.bottomEdge.touching)
        {
            var rayDirection = (transform.right);
            for (int i = 0; i < rays; i++)
            {
                float angleMod = ((i / (rays - 1f)) * 2f) - 1f;
                Vector3 direction = (Quaternion.AngleAxis(angleMod * rayArc / 2, Vector3.forward) * rayDirection).normalized;
                var hit = Physics2D.Raycast(transform.position, direction.normalized, checkDistance, LayerMask.GetMask("Player"));
                if (hit)
                {
                    var player = hit.transform.GetComponent<Player>();
                    if (!player || !player.notTargetable)
                    {
                        StartCoroutine(Leap());
                    }
                    break;
                }
            }

            if (!_leaping && leapAtObstacles && _controller2D.rightEdge.near && _controller2D.rightEdge.nearAngle % 90 == 0)
            {
                StartCoroutine(Leap());
            }
        }
    }

    private IEnumerator Leap()
    {
        _leaping = true;

        if(onLeapStart != null)
        {
            onLeapStart.Invoke();
        }

        yield return new WaitForSeconds(leapPreWarmTime);
        yield return new WaitForFixedUpdate();

        Vector2 velocity = ((Vector2)transform.right + Vector2.up).normalized * leapForce * _slowMod;

        while (velocity.y > 0 || !_controller2D.bottomEdge.touching)
        {
            _controller2D.Move(velocity * Time.deltaTime);
            yield return null;
            velocity += Physics2D.gravity * Time.deltaTime;
        }

        if (onLeapLand != null)
        {
            onLeapLand.Invoke();
        }

        yield return new WaitForSeconds(leapRecoveryTime);

        _leaping = false;
        _justLept = true;

        if (onLeapRecovered != null)
        {
            onLeapRecovered.Invoke();
        }

        yield return new WaitForSeconds(leapWaitTime);

        _justLept = false;
    }

    public void OnDrawGizmosSelected()
    {
        var rayDirection = (transform.right);
        for (int i = 0; i < rays; i++)
        {
            float angleMod = ((i / (rays - 1f)) * 2f) - 1f;
            Vector3 direction = (Quaternion.AngleAxis(angleMod * rayArc / 2, Vector3.forward) * rayDirection).normalized;
            Debug.DrawLine(transform.position, transform.position + direction * checkDistance, Color.green);
        }
    }

    //for unity events
    public void SetLeapWaitTime(float value)
    {
        leapWaitTime = value;
    }

    public void SetLeapForce(float value)
    {
        leapForce = value;
    }

    public void SetLeapAtObstacles(bool value)
    {
        leapAtObstacles = value;
    }
}
